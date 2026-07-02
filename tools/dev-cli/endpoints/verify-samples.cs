// ═══════════════════════════════════════════════════════════════════════════════
// VERIFY-SAMPLES COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Verifies that all code samples in the repository compile.

#region Purpose
// 'dev verify-samples' endpoint: builds each sample to verify it compiles, skipping gracefully when samples/ is absent.
#endregion

namespace DevCli.Commands;

/// <summary>
/// Verify code samples compile.
/// </summary>
[NuruRoute("verify-samples", Description = "Verify code samples compile")]
internal sealed class VerifySamplesCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<VerifySamplesCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(VerifySamplesCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot() ??
        throw new InvalidOperationException("Could not find git repository root (.git not found)");

      string samplesDirectory = Path.Combine(repoRoot, "samples");

      if (!Directory.Exists(samplesDirectory))
      {
        Terminal.WriteLine("No samples directory found. Skipping sample verification.");
        return Value;
      }

      string[] sampleFiles = Directory.GetFiles(samplesDirectory, "*.cs", SearchOption.AllDirectories);

      if (sampleFiles.Length == 0)
      {
        Terminal.WriteLine("No sample files found. Skipping sample verification.");
        return Value;
      }

      Terminal.WriteLine($"Verifying {sampleFiles.Length} sample file(s)...");

      int failedCount = 0;

      foreach (string sampleFile in sampleFiles)
      {
        string relativePath = Path.GetRelativePath(repoRoot, sampleFile);
        Terminal.WriteLine($"  Compiling: {relativePath}");

        int exitCode = await DotNet.Build()
          .WithProject(sampleFile)
          .WithConfiguration("Release")
          .WithVerbosity("minimal")
          .RunAsync();

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"    ❌ Failed: {relativePath}");
          failedCount++;
        }
        else
        {
          Terminal.WriteLine($"    ✅ Success: {relativePath}");
        }
      }

      if (failedCount > 0)
      {
        Environment.ExitCode = 1;
        Terminal.WriteErrorLine($"\n❌ {failedCount} sample(s) failed to compile");
        return Value;
      }

      Terminal.WriteLine($"\n✅ All {sampleFiles.Length} sample(s) verified successfully!");
      return Value;
    }
  }
}
