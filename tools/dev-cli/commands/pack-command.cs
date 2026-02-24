// ═══════════════════════════════════════════════════════════════════════════════
// PACK COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Creates NuGet packages for the TimeWarp.Builder library.

namespace DevCli.Commands;

/// <summary>
/// Create NuGet packages for TimeWarp.Builder.
/// </summary>
[NuruRoute("pack", Description = "Create NuGet packages")]
internal sealed class PackCommand : ICommand<Unit>
{
  [Option("configuration", "c", Description = "Build configuration (Debug or Release)")]
  public string Config { get; set; } = "Release";

  [Option("verbose", "v", Description = "Verbose output")]
  public bool Verbose { get; set; }

  internal sealed class Handler : ICommandHandler<PackCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(PackCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();

      if (repoRoot is null)
      {
        throw new InvalidOperationException("Could not find git repository root (.git not found)");
      }

      if (!File.Exists(Path.Combine(repoRoot, "timewarp-builder.slnx")))
      {
        throw new InvalidOperationException("Could not find repository root (timewarp-builder.slnx not found)");
      }

      string projectPath = Path.Combine(repoRoot, "source", "timewarp-builder", "timewarp-builder.csproj");
      string outputPath = Path.Combine(repoRoot, "artifacts", "packages");
      string verbosity = command.Verbose ? "normal" : "minimal";

      Terminal.WriteLine("Creating NuGet packages for TimeWarp.Builder...");
      Terminal.WriteLine($"Configuration: {command.Config}");
      Terminal.WriteLine($"Output: {outputPath}");

      Directory.CreateDirectory(outputPath);

      int exitCode = await DotNet.Pack()
        .WithProject(projectPath)
        .WithConfiguration(command.Config)
        .WithOutput(outputPath)
        .WithVerbosity(verbosity)
        .RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        Terminal.WriteErrorLine("Pack failed!");
        return Unit.Value;
      }

      // List produced packages
      string[] packages = Directory.GetFiles(outputPath, "*.nupkg");
      Terminal.WriteLine("\n✅ Pack completed successfully!");
      Terminal.WriteLine($"Packages in {outputPath}:");
      foreach (string package in packages)
      {
        Terminal.WriteLine($"  {Path.GetFileName(package)}");
      }

      return Unit.Value;
    }
  }
}
