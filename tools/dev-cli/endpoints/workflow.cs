// ═══════════════════════════════════════════════════════════════════════════════
// CI COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Orchestrates the full CI/CD pipeline with mode detection.
// Auto-detects mode from GITHUB_EVENT_NAME or accepts explicit --mode flag.
//
// Modes:
//   pr/merge:  clean -> build -> test
//   release:   clean -> build -> check-version -> pack -> push

namespace DevCli.Commands;

using System.Xml.Linq;

/// <summary>
/// Run the full CI/CD pipeline.
/// </summary>
[NuruRoute("workflow", Description = "Run full CI/CD pipeline")]
internal sealed class CiCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME if not specified)")]
  public string? Mode { get; set; }

  [Option("api-key", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<CiCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(CiCommand command, CancellationToken ct)
    {
      CiMode mode = DetermineMode(command.Mode);

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  CI/CD Pipeline - Mode: {mode}");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("");

      if (mode == CiMode.Release)
      {
        await RunReleaseWorkflowAsync(command.ApiKey);
      }
      else
      {
        await RunPrWorkflowAsync();
      }

      return Value;
    }

    private CiMode DetermineMode(string? explicitMode)
    {
      if (!string.IsNullOrEmpty(explicitMode))
      {
        return explicitMode.ToLowerInvariant() switch
        {
          "pr" => CiMode.Pr,
          "merge" => CiMode.Merge,
          "release" => CiMode.Release,
          _ => CiMode.Pr
        };
      }

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");

      CiMode mode = eventName switch
      {
        "pull_request" => CiMode.Pr,
        "push" => CiMode.Merge,
        "release" => CiMode.Release,
        "workflow_dispatch" => CiMode.Release,
        _ => CiMode.Pr
      };

      string displayEventName = eventName ?? "(not set)";
      Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      return mode;
    }

    private async Task RunPrWorkflowAsync()
    {
      Terminal.WriteLine("Pipeline: clean -> build -> test");
      Terminal.WriteLine("");

      // Step 1: Clean
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/3: Clean");
      Terminal.WriteLine("===============================================================================");
      CleanCommand.Handler cleanHandler = new(Terminal);
      await cleanHandler.Handle(new CleanCommand(), CancellationToken.None);

      // Step 2: Build
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/3: Build");
      Terminal.WriteLine("===============================================================================");
      BuildCommand.Handler buildHandler = new(Terminal);
      await buildHandler.Handle(new BuildCommand(), CancellationToken.None);

      // Step 3: Test
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/3: Test");
      Terminal.WriteLine("===============================================================================");
      TestCommand.Handler testHandler = new(Terminal);
      await testHandler.Handle(new TestCommand(), CancellationToken.None);

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED");
      Terminal.WriteLine("===============================================================================");
    }

    private async Task RunReleaseWorkflowAsync(string? apiKey)
    {
      Terminal.WriteLine("Pipeline: clean -> build -> check-version -> pack -> push");
      Terminal.WriteLine("");

      string? repoRoot = Git.FindRoot() ??
        throw new InvalidOperationException("Could not find git repository root (.git not found)");

      // Step 1: Clean
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/5: Clean");
      Terminal.WriteLine("===============================================================================");
      CleanCommand.Handler cleanHandler = new(Terminal);
      await cleanHandler.Handle(new CleanCommand(), CancellationToken.None);

      // Step 2: Build
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/5: Build");
      Terminal.WriteLine("===============================================================================");
      BuildCommand.Handler buildHandler = new(Terminal);
      await buildHandler.Handle(new BuildCommand(), CancellationToken.None);

      // Step 3: Check Version
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/5: Check Version");
      Terminal.WriteLine("===============================================================================");
      await CheckVersionAsync(repoRoot);

      // Step 4: Pack
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 4/5: Pack");
      Terminal.WriteLine("===============================================================================");
      PackCommand.Handler packHandler = new(Terminal);
      await packHandler.Handle(new PackCommand(), CancellationToken.None);

      // Step 5: Push
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 5/5: Push to NuGet");
      Terminal.WriteLine("===============================================================================");
      await PushPackagesAsync(repoRoot, apiKey);

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED - Package published to NuGet.org");
      Terminal.WriteLine("===============================================================================");
    }

    private async Task CheckVersionAsync(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");

      if (!File.Exists(propsPath))
      {
        throw new FileNotFoundException($"Could not find {propsPath}");
      }

      XDocument doc = XDocument.Load(propsPath);
      string? version = doc.Descendants("Version").FirstOrDefault()?.Value;

      if (string.IsNullOrEmpty(version))
      {
        throw new InvalidOperationException("Could not find version in source/Directory.Build.props");
      }

      Terminal.WriteLine($"Checking if TimeWarp.Builder {version} is already published on NuGet.org...");

      CommandOutput result = await Shell.Builder("dotnet")
        .WithArguments("package", "search", "TimeWarp.Builder", "--exact-match", "--prerelease", "--source", "https://api.nuget.org/v3/index.json")
        .WithNoValidation()
        .CaptureAsync();

      if (result.Stdout.Contains($"| {version} |", StringComparison.Ordinal))
      {
        Terminal.WriteErrorLine($"TimeWarp.Builder {version} is already published. Increment the version in source/Directory.Build.props.");
        Environment.ExitCode = 1;
        throw new InvalidOperationException($"Version {version} already published.");
      }

      Terminal.WriteLine($"✅ TimeWarp.Builder {version} is not yet published. Ready to release.");
    }

    private async Task PushPackagesAsync(string repoRoot, string? apiKey)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      XDocument doc = XDocument.Load(propsPath);
      string? version = doc.Descendants("Version").FirstOrDefault()?.Value;

      if (string.IsNullOrEmpty(version))
      {
        throw new InvalidOperationException("Could not determine version for push");
      }

      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      string nupkgPath = Path.Combine(artifactsDir, $"TimeWarp.Builder.{version}.nupkg");

      if (!File.Exists(nupkgPath))
      {
        throw new FileNotFoundException($"Package not found: {nupkgPath}");
      }

      Terminal.WriteLine($"Pushing TimeWarp.Builder.{version}.nupkg...");

      List<string> args = ["nuget", "push", nupkgPath, "--source", "https://api.nuget.org/v3/index.json", "--skip-duplicate"];

      if (!string.IsNullOrEmpty(apiKey))
      {
        args.AddRange(["--api-key", apiKey]);
      }

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments([.. args])
        .RunAsync();

      if (exitCode != 0)
      {
        throw new InvalidOperationException("Failed to push TimeWarp.Builder!");
      }

      Terminal.WriteLine("\n✅ TimeWarp.Builder pushed successfully!");
    }
  }
}

internal enum CiMode
{
  Pr,
  Merge,
  Release
}
