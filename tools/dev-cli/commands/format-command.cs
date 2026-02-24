// ═══════════════════════════════════════════════════════════════════════════════
// FORMAT COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Check or fix code formatting using dotnet format.

namespace DevCli.Commands;

/// <summary>
/// Check or fix code formatting.
/// </summary>
[NuruRoute("format", Description = "Check or fix code formatting")]
internal sealed class FormatCommand : ICommand<Unit>
{
  [Option("fix", "f", Description = "Fix formatting issues instead of just checking")]
  public bool Fix { get; set; }

  [Option("verbose", "v", Description = "Verbose output")]
  public bool Verbose { get; set; }

  internal sealed class Handler : ICommandHandler<FormatCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(FormatCommand command, CancellationToken ct)
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

      string solutionPath = Path.Combine(repoRoot, "timewarp-builder.slnx");

      Terminal.WriteLine(command.Fix ? "Fixing code formatting..." : "Checking code formatting...");

      List<string> formatArgs = ["format", solutionPath, "--severity", "warn"];

      if (!command.Fix)
      {
        formatArgs.Add("--verify-no-changes");
      }

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments([.. formatArgs])
        .WithNoValidation()
        .RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        string message = command.Fix
          ? "Format failed!"
          : "Code style violations found! Run 'dev format --fix' to fix them.";
        Terminal.WriteErrorLine(message);
        return Unit.Value;
      }

      Terminal.WriteLine(command.Fix ? "✅ Formatting fixed!" : "✅ Code style check passed!");
      return Unit.Value;
    }
  }
}
