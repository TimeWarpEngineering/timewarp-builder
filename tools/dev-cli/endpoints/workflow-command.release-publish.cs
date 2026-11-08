#region Purpose
// Locates the successful CI run, downloads its Packages artifact, and pushes that set to nuget.org.
#endregion
#region Design
// pull_request runs are not candidates. A push run is preferred over a release run
// at the same commit. The download directory is cleared first so a leftover nupkg
// cannot pass the package-set check. Push uses --skip-duplicate so a resumed run
// is idempotent at the HTTP layer.
#endregion

namespace DevCli.Commands;

internal sealed partial class WorkflowCommand
{
  internal sealed partial class Handler
  {
    private async Task<LocateRunOutcome> LocateCiRunAsync()
    {
      CommandOutput headResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithNoValidation()
        .CaptureAsync(Ct);

      if (headResult.ExitCode != 0)
      {
        throw new InvalidOperationException($"Could not determine HEAD commit: {headResult.Stderr.Trim()}");
      }

      string headSha = headResult.Stdout.Trim();

      CommandOutput runListResult;
      try
      {
        runListResult = await Shell.Builder("gh")
          .WithArguments("run", "list", "--workflow", "workflow.yml", "--commit", headSha, "--status", "success", "--json", "databaseId,event,headSha,createdAt")
          .WithNoValidation()
          .CaptureAsync(Ct);
      }
      catch (System.ComponentModel.Win32Exception)
      {
        return new LocateRunOutcome(LocateRunStatus.GhUnavailable, headSha, [], null);
      }

      if (runListResult.ExitCode != 0)
      {
        return new LocateRunOutcome(LocateRunStatus.GhFailed, headSha, [], runListResult.Stderr.Trim());
      }

      List<CiRunSummary>? runs = JsonSerializer.Deserialize(runListResult.Stdout, DevCliJsonContext.Default.ListCiRunSummary);
      IReadOnlyList<CiRunSummary> candidateRuns = CiRunPromotion.OrderCandidateRuns(runs ?? [], headSha);

      return candidateRuns.Count == 0
        ? new LocateRunOutcome(LocateRunStatus.NoMatchingRun, headSha, [], null)
        : new LocateRunOutcome(LocateRunStatus.Found, headSha, candidateRuns, null);
    }

    private async Task<DownloadArtifactOutcome> DownloadPackagesArtifactAsync(string repoRoot, IReadOnlyList<CiRunSummary> candidateRuns)
    {
      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      List<ExpiredArtifactEncounter> expiredEncounters = [];

      foreach (CiRunSummary run in candidateRuns)
      {
        CommandOutput artifactsResult = await Shell.Builder("gh")
          .WithArguments("api", $"repos/{{owner}}/{{repo}}/actions/runs/{run.DatabaseId}/artifacts")
          .WithNoValidation()
          .CaptureAsync(Ct);

        if (artifactsResult.ExitCode != 0)
        {
          throw new InvalidOperationException($"Failed to list artifacts for run {run.DatabaseId}: {artifactsResult.Stderr.Trim()}");
        }

        RunArtifactListResponse? artifactList = JsonSerializer.Deserialize(artifactsResult.Stdout, DevCliJsonContext.Default.RunArtifactListResponse);
        PackagesArtifactOutcome selectOutcome = CiRunPromotion.SelectPackagesArtifact(artifactList?.Artifacts ?? []);

        if (selectOutcome.Status == PackagesArtifactStatus.Expired)
        {
          expiredEncounters.Add(new ExpiredArtifactEncounter(run.DatabaseId, run.Event, selectOutcome.ExpiredNames));
          continue;
        }

        if (selectOutcome.Status == PackagesArtifactStatus.NoneMatching)
        {
          continue;
        }

        RunArtifact artifact = selectOutcome.Artifact!;

        if (Directory.Exists(artifactsDir))
        {
          Directory.Delete(artifactsDir, recursive: true);
        }

        Directory.CreateDirectory(artifactsDir);

        Terminal.WriteLine($"Downloading artifact '{artifact.Name}' from run {run.DatabaseId} ({run.Event})...");

        int exitCode = await Shell.Builder("gh")
          .WithArguments("run", "download", run.DatabaseId.ToString(CultureInfo.InvariantCulture), "--name", artifact.Name, "--dir", artifactsDir)
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync(Ct);

        if (exitCode != 0)
        {
          throw new InvalidOperationException($"Failed to download artifact '{artifact.Name}' from run {run.DatabaseId}.");
        }

        return new DownloadArtifactOutcome(DownloadArtifactStatus.Downloaded, run, artifact.Name, expiredEncounters);
      }

      return new DownloadArtifactOutcome(DownloadArtifactStatus.Exhausted, null, null, expiredEncounters);
    }

    private async Task PushPackagesAsync
    (
      string repoRoot,
      IReadOnlyList<PackableProject> projects,
      string version,
      string apiKey
    )
    {
      foreach (PackableProject project in projects)
      {
        string nupkgPath = Path.Combine(repoRoot, "artifacts", "packages", $"{project.PackageId}.{version}.nupkg");
        if (!File.Exists(nupkgPath))
        {
          throw new FileNotFoundException($"Package not found: {nupkgPath}");
        }

        Terminal.WriteLine($"Pushing {project.PackageId}.{version}.nupkg to NuGet.org...");

        int exitCode = await Shell.Builder("dotnet")
          .WithArguments("nuget", "push", nupkgPath, "--source", NuGetOrgSource, "--api-key", apiKey, "--skip-duplicate")
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync(Ct);

        if (exitCode != 0)
        {
          throw new InvalidOperationException($"Failed to push {project.PackageId} to NuGet.org.");
        }
      }

      Terminal.WriteLine("\nPackages pushed to NuGet.org.");
    }

    private enum LocateRunStatus
    {
      Found,
      GhUnavailable,
      GhFailed,
      NoMatchingRun
    }

    private sealed record LocateRunOutcome(LocateRunStatus Status, string HeadSha, IReadOnlyList<CiRunSummary> CandidateRuns, string? Detail);

    private enum DownloadArtifactStatus
    {
      Downloaded,
      Exhausted
    }

    private sealed record DownloadArtifactOutcome(DownloadArtifactStatus Status, CiRunSummary? Run, string? ArtifactName, IReadOnlyList<ExpiredArtifactEncounter> ExpiredEncounters);

    private sealed record ExpiredArtifactEncounter(long RunId, string Event, IReadOnlyList<string> ArtifactNames);
  }
}
