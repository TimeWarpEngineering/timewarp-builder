#!/usr/bin/env -S dotnet --
// ═══════════════════════════════════════════════════════════════════════════════
// DEV CLI - TIMEWARP.BUILDER DEVELOPMENT TOOL
// ═══════════════════════════════════════════════════════════════════════════════
//
// This is the development CLI for TimeWarp.Builder that provides:
// - Build, test, pack, and clean commands
// - AOT-compiled binary for fast execution
//
// Usage:
//   As runfile:  dotnet tools/dev-cli/dev.cs <command>
//   As AOT:      ./bin/dev <command>
//
// Commands:
//   dev build          - Build the TimeWarp.Builder library
//   dev test           - Run tests
//   dev clean          - Clean solution and artifacts
//   dev pack           - Create NuGet packages
//   dev format         - Check/fix code formatting
//   dev check-version  - Verify version is ready to release
//   dev verify-samples - Verify code samples compile
//   dev workflow       - Run full CI/CD pipeline
//   dev self-install   - AOT compile and install dev CLI to ./bin
//
// To bootstrap:
//   dotnet run tools/dev-cli/dev.cs -- self-install
//   direnv allow
//   dev --help
// ═══════════════════════════════════════════════════════════════════════════════

#region Purpose
// Runfile entry point for the dev CLI: builds the Nuru app and dispatches to the endpoint commands.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-builder")
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
