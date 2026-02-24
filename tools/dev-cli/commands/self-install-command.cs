// ═══════════════════════════════════════════════════════════════════════════════
// SELF-INSTALL COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// AOT compiles and installs the dev CLI to ./bin for fast execution via direnv PATH.

namespace DevCli.Commands;

/// <summary>
/// AOT compile and install dev CLI to ./bin directory.
/// </summary>
[NuruRoute("self-install", Description = "AOT compile and install dev CLI to ./bin")]
internal sealed class SelfInstallCommand : ICommand<Unit>
{
  [Option("verbose", "v", Description = "Verbose output")]
  public bool Verbose { get; set; }

  internal sealed class Handler : ICommandHandler<SelfInstallCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(SelfInstallCommand command, CancellationToken ct)
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

      string devCliSource = Path.Combine(repoRoot, "tools", "dev-cli", "dev.cs");
      string outputPath = Path.Combine(repoRoot, "bin");
      string rid = GetRuntimeIdentifier();

      Terminal.WriteLine("Installing dev CLI as AOT binary...");
      Terminal.WriteLine($"Source: {devCliSource}");
      Terminal.WriteLine($"Output: {outputPath}/dev");
      Terminal.WriteLine($"Runtime: {rid}");

      Directory.CreateDirectory(outputPath);

      int exitCode = await DotNet.Publish()
        .WithProject(devCliSource)
        .WithConfiguration("Release")
        .WithRuntime(rid)
        .WithSelfContained()
        .WithOutput(outputPath)
        .RunAsync();

      if (exitCode != 0)
      {
        Environment.ExitCode = exitCode;
        Terminal.WriteErrorLine("AOT compilation failed!");
        return Unit.Value;
      }

      string binaryName = rid.StartsWith("win", StringComparison.OrdinalIgnoreCase) ? "dev.exe" : "dev";
      string binaryPath = Path.Combine(outputPath, binaryName);

      if (File.Exists(binaryPath))
      {
        FileInfo info = new(binaryPath);
        Terminal.WriteLine($"\n✅ AOT binary installed: {binaryPath}");
        Terminal.WriteLine($"   Size: {info.Length / 1024.0 / 1024.0:F1} MB");
        Terminal.WriteLine("\nRun 'direnv allow' to add ./bin to PATH, then use: dev <command>");
      }
      else
      {
        throw new InvalidOperationException($"Binary not found at expected location: {binaryPath}");
      }

      return Unit.Value;
    }

    private static string GetRuntimeIdentifier()
    {
      if (OperatingSystem.IsWindows())
      {
        return Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
      }

      if (OperatingSystem.IsMacOS())
      {
        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
      }

      if (OperatingSystem.IsLinux())
      {
        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
      }

      return "linux-x64";
    }
  }
}
