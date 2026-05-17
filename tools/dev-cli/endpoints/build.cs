// ═══════════════════════════════════════════════════════════════════════════════
// BUILD COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Builds the TimeWarp.Builder library.

namespace DevCli.Commands;

/// <summary>
/// Build the TimeWarp.Builder library.
/// </summary>
[NuruRoute("build", Description = "Build the TimeWarp.Builder library")]
internal sealed class BuildCommand : ICommand<Unit>
{
  [Option("configuration", "c", Description = "Build configuration (Debug or Release)")]
  public string Config { get; set; } = "Release";

  [Option("verbose", "v", Description = "Verbose output")]
  public bool Verbose { get; set; }

  internal sealed class Handler : ICommandHandler<BuildCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(BuildCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot() ??
        throw new InvalidOperationException("Could not find git repository root (.git not found)");

      if (!File.Exists(Path.Combine(repoRoot, "timewarp-builder.slnx")))
      {
        throw new InvalidOperationException("Could not find repository root (timewarp-builder.slnx not found)");
      }

      string projectPath = Path.Combine(repoRoot, "source", "timewarp-builder", "timewarp-builder.csproj");
      string verbosity = command.Verbose ? "normal" : "minimal";

      Terminal.WriteLine("Building TimeWarp.Builder...");
      Terminal.WriteLine($"Configuration: {command.Config}");
      Terminal.WriteLine($"Working from: {repoRoot}");

      int exitCode = await DotNet.Build()
        .WithProject(projectPath)
        .WithConfiguration(command.Config)
        .WithVerbosity(verbosity)
        .RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        Terminal.WriteErrorLine("Build failed!");
        return Unit.Value;
      }

      Terminal.WriteLine("\n✅ Build completed successfully!");
      return Unit.Value;
    }
  }
}
