// ═══════════════════════════════════════════════════════════════════════════════
// CHECK-VERSION COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Verifies the source version is not already published on NuGet.org.

#region Purpose
// 'dev check-version' endpoint: verifies the version in source/Directory.Build.props is not already published on NuGet.org.
#endregion

namespace DevCli.Commands;

using System.Xml.Linq;

/// <summary>
/// Verify the version is ready to release.
/// </summary>
[NuruRoute("check-version", Description = "Verify version is ready to release")]
internal sealed class CheckVersionCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<CheckVersionCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(CheckVersionCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot() ??
        throw new InvalidOperationException("Could not find git repository root (.git not found)");

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
        return Value;
      }

      Terminal.WriteLine($"✅ TimeWarp.Builder {version} is not yet published. Ready to release.");
      return Value;
    }
  }
}
