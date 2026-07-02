// ═══════════════════════════════════════════════════════════════════════════════
// TEST COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Runs the test suite for TimeWarp.Builder.

#region Purpose
// 'dev test' endpoint: runs each Jaribu test runfile under tests/, skipping gracefully when none exist.
#endregion

#region Design
// Tests are standalone Jaribu runfiles (see the jaribu skill), not dotnet-test projects,
// so each *.cs under tests/ is executed via 'dotnet run' and exit codes are aggregated.
// --filter maps to the JARIBU_FILTER_TAG environment variable Jaribu uses for tag filtering.
#endregion

namespace DevCli.Commands;

/// <summary>
/// Run the TimeWarp.Builder test suite.
/// </summary>
[NuruRoute("test", Description = "Run the test suite")]
internal sealed class TestCommand : ICommand<Unit>
{
  [Option("filter", "f", Description = "Jaribu tag filter (sets JARIBU_FILTER_TAG)")]
  public string? Filter { get; set; }

  internal sealed class Handler : ICommandHandler<TestCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(TestCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot() ?? throw new InvalidOperationException("Could not find git repository root (.git not found)");
      if (!File.Exists(Path.Combine(repoRoot, "timewarp-builder.slnx")))
      {
        throw new InvalidOperationException("Could not find repository root (timewarp-builder.slnx not found)");
      }

      string testsDirectory = Path.Combine(repoRoot, "tests");

      string[] testFiles = Directory.Exists(testsDirectory)
        ? [.. Directory.GetFiles(testsDirectory, "*.cs", SearchOption.AllDirectories).Order(StringComparer.Ordinal)]
        : [];

      if (testFiles.Length == 0)
      {
        Terminal.WriteLine("No tests found. Skipping test step.");
        return Value;
      }

      Terminal.WriteLine($"Running {testFiles.Length} Jaribu test file(s)...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      int failedCount = 0;

      foreach (string testFile in testFiles)
      {
        string relativePath = Path.GetRelativePath(repoRoot, testFile);
        Terminal.WriteLine($"\n▶ {relativePath}");

        ShellBuilder runBuilder = Shell.Builder("dotnet")
          .WithArguments("run", testFile)
          .WithNoValidation();

        if (command.Filter is not null)
        {
          runBuilder = runBuilder.WithEnvironmentVariable("JARIBU_FILTER_TAG", command.Filter);
        }

        int exitCode = await runBuilder.RunAsync();

        if (exitCode != 0)
        {
          failedCount++;
          Terminal.WriteErrorLine($"❌ Failed: {relativePath}");
        }
      }

      if (failedCount > 0)
      {
        Environment.ExitCode = 1;
        Terminal.WriteErrorLine($"\n❌ {failedCount} of {testFiles.Length} test file(s) failed");
        return Value;
      }

      Terminal.WriteLine($"\n✅ All {testFiles.Length} test file(s) passed!");
      return Value;
    }
  }
}
