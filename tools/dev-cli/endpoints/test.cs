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
// --coverage wraps each runfile in the pinned dotnet-coverage tool (.config/dotnet-tools.json) and
// writes one Cobertura file per runfile to artifacts/coverage; CI uploads that folder.
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

  [Option("coverage", "c", Description = "Collect Cobertura code coverage into artifacts/coverage")]
  public bool Coverage { get; set; }

  internal sealed class Handler : IRequestHandler<TestCommand, Unit>
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
        return Unit.Value;
      }

      Terminal.WriteLine($"Running {testFiles.Length} Jaribu test file(s)...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      string coverageDirectory = Path.Combine(repoRoot, "artifacts", "coverage");

      if (command.Coverage)
      {
        Directory.CreateDirectory(coverageDirectory);
        int restoreExitCode = await DotNet.Tool()
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .Restore()
          .RunAsync();

        if (restoreExitCode != 0)
        {
          Environment.ExitCode = restoreExitCode;
          Terminal.WriteErrorLine("❌ dotnet tool restore failed; cannot collect coverage");
          return Unit.Value;
        }
      }

      int failedCount = 0;

      foreach (string testFile in testFiles)
      {
        string relativePath = Path.GetRelativePath(repoRoot, testFile);
        Terminal.WriteLine($"\n▶ {relativePath}");

        int exitCode = command.Coverage
          ? await RunWithCoverageAsync(repoRoot, coverageDirectory, testFile, command.Filter)
          : await RunTestFileAsync(testFile, command.Filter);

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
        return Unit.Value;
      }

      Terminal.WriteLine($"\n✅ All {testFiles.Length} test file(s) passed!");
      return Unit.Value;
    }

    private static Task<int> RunWithCoverageAsync(string repoRoot, string coverageDirectory, string testFile, string? filter)
    {
      DotNetToolBuilder toolBuilder = DotNet.Tool()
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation();

      if (filter is not null)
      {
        toolBuilder = toolBuilder.WithEnvironmentVariable("JARIBU_FILTER_TAG", filter);
      }

      return toolBuilder
        .Run("dotnet-coverage")
        .WithArguments(
          "collect",
          "--output-format", "cobertura",
          "--output", Path.Combine(coverageDirectory, $"{Path.GetFileNameWithoutExtension(testFile)}.cobertura.xml"),
          "--", "dotnet", "run", testFile)
        .RunAsync();
    }

    private static Task<int> RunTestFileAsync(string testFile, string? filter)
    {
      DotNetRunBuilder runBuilder = DotNet.Run()
        .WithFile(testFile)
        .WithNoValidation();

      if (filter is not null)
      {
        runBuilder = runBuilder.WithEnvironmentVariable("JARIBU_FILTER_TAG", filter);
      }

      return runBuilder.RunAsync();
    }
  }
}
