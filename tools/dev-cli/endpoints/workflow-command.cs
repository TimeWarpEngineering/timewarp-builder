#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.Builder.
#endregion
#region Design
// PR/merge: self-install -> clean -> build -> test -> pack. Pack writes the
// IsPackable set to artifacts/packages; workflow.yml uploads that directory as
// the Packages-* artifact on a green master push.
// Self-install uses the runfile because bin/dev is gitignored and missing on a
// fresh CI checkout. Later PR/merge steps use ./bin/dev (bin/dev.exe on Windows).
// Release: tag-gate -> check-version -> locate-run -> download-artifact -> verify -> push.
// Release promotes the CI artifact and does not rebuild. The only push is nuget.org,
// with --api-key set to the short-lived nuget/login key.
// workflow_dispatch without --mode stays merge and does not publish. dev release
// remains the human tag + GitHub Release cut; this command is what Actions calls.
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI/CD pipeline (mode-aware)")]
internal sealed partial class WorkflowCommand : ICommand<Unit>
{
  internal const string NuGetOrgSource = "https://api.nuget.org/v3/index.json";

  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME)")]
  public string? Mode { get; set; }

  [Option("api-key", Description = "Short-lived nuget.org API key from nuget/login. Required in release mode.")]
  public string? ApiKey { get; set; }

  internal sealed partial class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService RepoConfigService;
    private readonly IPackableProjectService PackableProjectService;
    private CancellationToken Ct;
    private string? ApiKey;
    private string RepoRoot = null!;
    private string DevBin = null!;

    public Handler
    (
      ITerminal terminal,
      NuGetVersionService nuGetVersionService,
      IRepoConfigService repoConfigService,
      IPackableProjectService packableProjectService
    )
    {
      Terminal = terminal;
      NuGetVersionService = nuGetVersionService;
      RepoConfigService = repoConfigService;
      PackableProjectService = packableProjectService;
    }

    public async Task<Unit> Handle(WorkflowCommand command, CancellationToken cancellationToken)
    {
      ArgumentNullException.ThrowIfNull(command);

      Ct = cancellationToken;
      ApiKey = command.ApiKey;

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      CiMode mode;
      try
      {
        mode = CiModeDetector.DetermineMode(command.Mode, eventName);
      }
      catch (ArgumentException exception)
      {
        Terminal.WriteErrorLine(exception.Message);
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (string.IsNullOrEmpty(command.Mode))
      {
        string displayEventName = eventName ?? "(not set)";
        Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      }

      Terminal.WriteLine($"\nCI/CD Pipeline — Mode: {mode}\n");

      if (mode == CiMode.Release)
      {
        await RunReleaseAsync();
      }
      else
      {
        await RunPrAsync();
      }

      return Unit.Value;
    }

    private async Task RunPrAsync()
    {
      Terminal.WriteLine("Pipeline: self-install -> clean -> build -> test -> pack\n");
      Environment.ExitCode = 0;

      if (!FindRepoRoot())
      {
        return;
      }

      if (!await SelfInstallAsync())
      {
        return;
      }

      if (!await RunDevStepAsync("clean", "Clean"))
      {
        return;
      }

      if (!await RunDevStepAsync("build", "Build"))
      {
        return;
      }

      if (!await RunDevStepAsync("test", "Test"))
      {
        return;
      }

      if (!await RunDevStepAsync("pack", "Pack"))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private bool FindRepoRoot()
    {
      string? root = Git.FindRoot();
      if (root is null || !File.Exists(Path.Combine(root, "timewarp-builder.slnx")))
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      RepoRoot = root;
      string binaryName = OperatingSystem.IsWindows() ? "dev.exe" : "dev";
      DevBin = Path.Combine(RepoRoot, "bin", binaryName);
      return true;
    }

    private async Task<bool> SelfInstallAsync()
    {
      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--file", "tools/dev-cli/dev.cs", "--", "self-install")
        .WithWorkingDirectory(RepoRoot)
        .WithNoValidation()
        .RunAsync(Ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("\nPipeline FAILED — Self-install failed".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> RunDevStepAsync(string subcommand, string stepName)
    {
      int exitCode = await Shell.Builder(DevBin)
        .WithArguments(subcommand)
        .WithWorkingDirectory(RepoRoot)
        .WithNoValidation()
        .RunAsync(Ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} failed".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private void AbortPipeline(string reason)
    {
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  Pipeline ABORTED — {reason}");
      Terminal.WriteLine("===============================================================================");
      Environment.ExitCode = 1;
    }
  }
}
