// ═══════════════════════════════════════════════════════════════════════════════
// CLEAN COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Cleans the TimeWarp.Builder solution and deletes all bin/obj directories.

namespace DevCli.Commands;

/// <summary>
/// Clean the solution and all build artifacts.
/// </summary>
[NuruRoute("clean", Description = "Clean solution and build artifacts")]
internal sealed class CleanCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<CleanCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(CleanCommand command, CancellationToken ct)
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

      Terminal.WriteLine("Cleaning TimeWarp.Builder solution...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      int exitCode = await DotNet.Clean()
        .WithProject(Path.Combine(repoRoot, "timewarp-builder.slnx"))
        .WithVerbosity("minimal")
        .RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        Terminal.WriteErrorLine("dotnet clean failed!");
        return Unit.Value;
      }

      // Also delete obj and bin directories for a thorough clean
      Terminal.WriteLine("\nDeleting obj and bin directories...");
      string[] directoriesToDelete =
      [
        .. Directory.GetDirectories(repoRoot, "obj", SearchOption.AllDirectories)
          .Concat(Directory.GetDirectories(repoRoot, "bin", SearchOption.AllDirectories))
          .Where(d => !d.Contains(Path.Combine("tools", "dev-cli"))),
      ];

      foreach (string dir in directoriesToDelete)
      {
        try
        {
          Directory.Delete(dir, recursive: true);
          Terminal.WriteLine($"  Deleted: {dir}");
        }
        catch (Exception ex)
        {
          Terminal.WriteLine($"  Warning: Could not delete {dir}: {ex.Message}");
        }
      }

      Terminal.WriteLine("\n✅ Clean completed successfully!");
      return Unit.Value;
    }
  }
}
