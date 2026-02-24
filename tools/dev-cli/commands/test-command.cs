// ═══════════════════════════════════════════════════════════════════════════════
// TEST COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Runs the test suite for TimeWarp.Builder.

namespace DevCli.Commands;

/// <summary>
/// Run the TimeWarp.Builder test suite.
/// </summary>
[NuruRoute("test", Description = "Run the test suite")]
internal sealed class TestCommand : ICommand<Unit>
{
  [Option("filter", "f", Description = "Test filter expression")]
  public string? Filter { get; set; }

  [Option("no-build", null, Description = "Skip build before testing")]
  public bool NoBuild { get; set; }

  [Option("verbose", "v", Description = "Verbose output")]
  public bool Verbose { get; set; }

  internal sealed class Handler : ICommandHandler<TestCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(TestCommand command, CancellationToken ct)
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

      string testsDirectory = Path.Combine(repoRoot, "tests");

      if (!Directory.Exists(testsDirectory))
      {
        Terminal.WriteErrorLine("No tests directory found. Create tests/ and add a test project first.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      Terminal.WriteLine("Running TimeWarp.Builder tests...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      ShellBuilder testBuilder = Shell.Builder("dotnet")
        .WithArguments("test", Path.Combine(repoRoot, "timewarp-builder.slnx"), "--verbosity", command.Verbose ? "normal" : "minimal");

      if (command.NoBuild)
      {
        testBuilder = testBuilder.WithArguments("--no-build");
      }

      if (command.Filter is not null)
      {
        testBuilder = testBuilder.WithArguments("--filter", command.Filter);
      }

      int exitCode = await testBuilder.WithNoValidation().RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        Terminal.WriteErrorLine($"Tests failed with exit code {exitCode}");
        return Unit.Value;
      }

      Terminal.WriteLine("\n✅ Tests completed successfully!");
      return Unit.Value;
    }
  }
}
