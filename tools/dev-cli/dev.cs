#!/usr/bin/env -S dotnet --
// ═══════════════════════════════════════════════════════════════════════════════
// DEV CLI - TIMEWARP.BUILDER DEVELOPMENT TOOL
// ═══════════════════════════════════════════════════════════════════════════════
//
// Usage:
//   As runfile:  dotnet run --file tools/dev-cli/dev.cs -- <command>
//   As AOT:      ./bin/dev <command>
//
// Run `./bin/dev --help` for available commands.
//
// To bootstrap:
//   dotnet run --file tools/dev-cli/dev.cs -- self-install
//   direnv allow
//   dev --help
// ═══════════════════════════════════════════════════════════════════════════════

#region Purpose
// Entry point for the dev CLI.
#endregion
#region Design
// Thin wrapper around TimeWarp.Nuru. Shared endpoints (clean, self-install,
// check-version, release) come from TimeWarp.Nuru.DevCli and require the DI
// registrations below (NURU050 names any that are missing). Repo commands:
// build, test, pack, format, verify-samples, and workflow.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-builder")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
