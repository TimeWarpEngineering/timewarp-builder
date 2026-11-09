#region Purpose
// Release pipeline: tag gate, version check, promote the CI artifact, verify, and push.
#endregion
#region Design
// Release: tag-gate → check-version → locate-run → download-artifact → verify → push.
// Nothing is rebuilt. The downloaded Packages-* bytes are the ones CI packed.
// The only destination is nuget.org.
#endregion

namespace DevCli.Commands;

internal sealed partial class WorkflowCommand
{
  internal sealed partial class Handler
  {
    private async Task RunReleaseAsync()
    {
      Terminal.WriteLine("Pipeline: tag-gate → check-version → locate-run → download-artifact → verify → push\n");
      Environment.ExitCode = 0;

      string? repoRoot = Git.FindRoot();
      if (repoRoot is null || !File.Exists(Path.Combine(repoRoot, "timewarp-builder.slnx")))
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        AbortPipeline("repository root not found");
        return;
      }

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/6: Release Gate — Tag Assertions");
      Terminal.WriteLine("===============================================================================");

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      string? propsVersion = PropsVersionReader.Read(repoRoot);

      if (eventName == "release")
      {
        string? refName = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        TagAssertionResult tagResult = TagAssertion.Validate(refName, propsVersion);

        if (!tagResult.IsValid)
        {
          Terminal.WriteErrorLine($"Release gate failed: {tagResult.Error}");
          AbortPipeline("release tag does not match source version");
          return;
        }

        Terminal.WriteLine($"Tag assertion passed: {tagResult.ExpectedTag}");
      }
      else
      {
        Terminal.WriteLine("Tag assertion skipped: GITHUB_EVENT_NAME is not 'release' (break-glass/local release has no triggering ref tag to assert against; the tag-pin check below still applies).");
      }

      if (string.IsNullOrWhiteSpace(propsVersion))
      {
        Terminal.WriteErrorLine("Release gate failed: could not read <Version> from source/Directory.Build.props.");
        AbortPipeline("props version unreadable");
        return;
      }

      TagPinOutcome tagPinOutcome = await CheckTagPinAsync(propsVersion);
      string pinTag = $"v{propsVersion}";

      switch (tagPinOutcome.Status)
      {
        case TagPinStatus.NoTag:
          Terminal.WriteLine($"Tag pin: {pinTag} not yet tagged.");
          break;

        case TagPinStatus.Match:
          Terminal.WriteLine($"Tag pin passed: HEAD is at {pinTag}.");
          break;

        case TagPinStatus.Mismatch:
          Terminal.WriteErrorLine($"Release gate failed: tag {pinTag} already exists at commit {ShortSha(tagPinOutcome.TagCommit)}; this run is at {ShortSha(tagPinOutcome.HeadCommit)}. A partial-publish resume must run from the tag's commit (or bump the version if source changed).");
          AbortPipeline("tag pin mismatch");
          return;

        case TagPinStatus.GitError:
          Terminal.WriteErrorLine($"Release gate failed: tag pin check could not run — {tagPinOutcome.Detail}");
          AbortPipeline("tag pin check could not run");
          return;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled tag pin status '{tagPinOutcome.Status}'.");
          AbortPipeline("unhandled tag pin status");
          return;
      }

      AncestorCheckOutcome ancestorOutcome = await CheckHeadAncestorOfMasterAsync();

      switch (ancestorOutcome.Status)
      {
        case AncestorCheckStatus.NotAncestor:
          Terminal.WriteErrorLine("Release gate failed: current commit is not an ancestor of master. Releases must be cut from commits on master.");
          AbortPipeline("commit not on master");
          return;

        case AncestorCheckStatus.MasterUnresolvable:
          Terminal.WriteErrorLine("Release gate failed: cannot resolve origin/master or master — ensure the checkout has full history (fetch-depth: 0) and a master ref exists.");
          AbortPipeline("master ref unresolvable");
          return;

        case AncestorCheckStatus.GitError:
          Terminal.WriteErrorLine($"Release gate failed: ancestor check could not run — {ancestorOutcome.Detail}");
          AbortPipeline("ancestor check could not run");
          return;

        case AncestorCheckStatus.Ancestor:
          Terminal.WriteLine("Ancestor check passed: HEAD is on master.");
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled ancestor check status '{ancestorOutcome.Status}'.");
          AbortPipeline("unhandled ancestor check status");
          return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/6: Check Version");
      Terminal.WriteLine("===============================================================================");
      CheckVersionCommand.Handler checkVersionHandler = new(
        Terminal,
        NuGetVersionService,
        RepoConfigService,
        PackableProjectService);
      await checkVersionHandler.Handle(new CheckVersionCommand(), Ct);

      if (Environment.ExitCode != 0)
      {
        AbortPipeline("version already released");
        return;
      }

      IReadOnlyList<PackableProject> packableProjects = await PackableProjectService
        .GetPackableProjectsAsync(repoRoot, Ct);

      if (packableProjects.Count == 0)
      {
        Terminal.WriteErrorLine("Release gate failed: no packable projects found under source/.");
        AbortPipeline("no packable projects found");
        return;
      }

      Terminal.WriteLine($"Packable set ({packableProjects.Count}) at {propsVersion}: {string.Join(", ", packableProjects.Select(project => project.PackageId))}");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/6: Locate CI Run");
      Terminal.WriteLine("===============================================================================");

      LocateRunOutcome locateOutcome = await LocateCiRunAsync();

      switch (locateOutcome.Status)
      {
        case LocateRunStatus.GhUnavailable:
          Terminal.WriteErrorLine("Release gate failed: release mode promotes CI-built artifacts and requires the gh CLI. On runners GH_TOKEN is provided by workflow.yml; locally install gh and run 'gh auth login'.");
          AbortPipeline("gh CLI unavailable");
          return;

        case LocateRunStatus.GhFailed:
          Terminal.WriteErrorLine($"Release gate failed: gh run list failed — {locateOutcome.Detail}. If this is transient (network/rate limit), retry; for auth issues run 'gh auth login'.");
          AbortPipeline("gh run list failed");
          return;

        case LocateRunStatus.NoMatchingRun:
          Terminal.WriteErrorLine($"Release gate failed: no successful CI run of workflow.yml exists for commit {locateOutcome.HeadSha}. Only tested CI artifacts are published — this commit must pass CI first. If a run failed, fix and re-run it (gh run rerun <run-id>).");
          AbortPipeline("no successful CI run found");
          return;

        case LocateRunStatus.Found:
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled locate-run status '{locateOutcome.Status}'.");
          AbortPipeline("unhandled locate-run status");
          return;
      }

      Terminal.WriteLine($"Found {locateOutcome.CandidateRuns.Count} candidate CI run(s) for commit {ShortSha(locateOutcome.HeadSha)}.");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 4/6: Download Artifact");
      Terminal.WriteLine("===============================================================================");

      DownloadArtifactOutcome downloadOutcome = await DownloadPackagesArtifactAsync(repoRoot, locateOutcome.CandidateRuns);

      if (downloadOutcome.Status == DownloadArtifactStatus.Exhausted)
      {
        if (downloadOutcome.ExpiredEncounters.Count > 0)
        {
          string expiredDetail = string.Join("; ", downloadOutcome.ExpiredEncounters.Select(encounter => $"run {encounter.RunId} ({encounter.Event}): {string.Join(", ", encounter.ArtifactNames)}"));
          Terminal.WriteErrorLine($"Release gate failed: every candidate CI run's Packages-* artifact has expired — {expiredDetail}. Re-run CI to produce a fresh tested artifact (gh run rerun {downloadOutcome.ExpiredEncounters[0].RunId}).");
        }
        else
        {
          Terminal.WriteErrorLine($"Release gate failed: no candidate CI run for commit {locateOutcome.HeadSha} uploaded a Packages-* artifact. Re-run CI to produce one (gh run rerun {locateOutcome.CandidateRuns[0].DatabaseId}).");
        }

        AbortPipeline("no usable CI artifact found");
        return;
      }

      Terminal.WriteLine($"Downloaded '{downloadOutcome.ArtifactName}' from run {downloadOutcome.Run!.DatabaseId} ({downloadOutcome.Run.Event}).");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 5/6: Verify Package Set");
      Terminal.WriteLine("===============================================================================");

      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      string[] actualNupkgPaths = Directory.Exists(artifactsDir) ? Directory.GetFiles(artifactsDir, "*.nupkg") : [];
      IReadOnlyList<string> actualFileNames = [.. actualNupkgPaths.Select(Path.GetFileName)!];

      PackageSetVerification verification = CiRunPromotion.VerifyPackageSet(actualFileNames, packableProjects, propsVersion);

      if (!verification.IsMatch)
      {
        if (verification.Missing.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact is missing package(s): {string.Join(", ", verification.Missing)}.");
        }

        if (verification.Unexpected.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact has unexpected package(s): {string.Join(", ", verification.Unexpected)}.");
        }

        Terminal.WriteErrorLine($"CI run likely predates the version bump — re-run CI on commit {locateOutcome.HeadSha} and retry.");
        AbortPipeline("downloaded package set does not match derived packable set");
        return;
      }

      Terminal.WriteLine("Package set verified.");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 6/6: Push to NuGet.org");
      Terminal.WriteLine("===============================================================================");

      if (string.IsNullOrWhiteSpace(ApiKey))
      {
        Terminal.WriteErrorLine("Release gate failed: --api-key is required. It is the short-lived key from nuget/login.");
        AbortPipeline("missing nuget.org API key");
        return;
      }

      try
      {
        await PushPackagesAsync(repoRoot, packableProjects, propsVersion, ApiKey);
      }
      catch (Exception exception) when (exception is InvalidOperationException or FileNotFoundException)
      {
        Terminal.WriteErrorLine($"Release gate failed: {exception.Message}");
        AbortPipeline("package push failed");
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED - Packages published to NuGet.org");
      Terminal.WriteLine("===============================================================================");
    }
  }
}
