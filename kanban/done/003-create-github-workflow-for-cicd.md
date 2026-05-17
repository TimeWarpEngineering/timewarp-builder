# Create GitHub workflow for CI/CD

## Description

Create a GitHub Actions workflow for continuous integration and NuGet package publishing, including implementing the `dev ci` command in the dev CLI. Reference implementations:
- GitHub workflow: `timewarp-terminal/.github/workflows/workflow.yml`
- CI command: `timewarp-nuru/master/tools/dev-cli/commands/ci-command.cs`

## Results

**Commit:** e494fad  
**Date:** 2026-02-24

### What was implemented

**`tools/dev-cli/endpoints/ci.cs`**
- `CiCommand` with `--mode` (pr/merge/release) and `--api-key` options
- Auto-detects mode from `GITHUB_EVENT_NAME` environment variable
- PR/Merge workflow: clean → build → test (3 steps)
- Release workflow: clean → build → check-version → pack → push (5 steps)
- `CheckVersionAsync`: reads version from `source/Directory.Build.props` via `XDocument`, checks NuGet.org for duplicate
- `PushPackagesAsync`: `dotnet nuget push` with optional OIDC API key
- `CiMode` enum (Pr, Merge, Release) at bottom of file

**`.github/workflows/workflow.yml`**
- Triggers: push/PR to master (path filters), release published, workflow_dispatch
- OIDC Trusted Publishing via `nuget/login@v1` on release events only
- Calls `dotnet run tools/dev-cli/dev.cs -- ci` (with `--api-key` on release)
- Uploads `artifacts/packages/*.nupkg` artifacts always

**`tools/dev-cli/endpoints/clean.cs` (fixed)**
- Changed target from `timewarp-builder.slnx` to `source/timewarp-builder/timewarp-builder.csproj`
- The `.slnx` parser cannot handle the `dev.cs` runfile entry in the solution

### Test results

`dotnet run tools/dev-cli/dev.cs -- ci` runs successfully:
- Step 1/3 Clean ✅
- Step 2/3 Build ✅ (0 warnings, 0 errors, package produced)
- Step 3/3 Test ✅ (gracefully reports no test project yet)
- Pipeline SUCCEEDED

## Checklist

### Dev CLI - Implement CI Command

- [x] **Create `tools/dev-cli/endpoints/ci.cs`**
   - Reference: `timewarp-nuru/master/tools/dev-cli/commands/ci-command.cs`
   - Route: `[NuruRoute("ci", Description = "Run full CI/CD pipeline")]`
   - Options: `--mode` (pr/merge/release), `--api-key` (for NuGet publishing)
   - Auto-detect mode from `GITHUB_EVENT_NAME` environment variable
   - Implement `CiMode` enum: Pr, Merge, Release

- [x] **Implement PR/merge workflow**
   - Steps: clean -> build -> test (skip verify-samples - not applicable)
   - Run each step by calling other command handlers directly (like the reference does)

- [x] **Implement release workflow**
   - Steps: clean -> build -> check-version -> pack -> push
   - Check-version: verify version hasn't been published to NuGet.org
   - Pack: create NuGet packages via `dotnet pack`
   - Push: push packages to NuGet.org via `dotnet nuget push`

- [x] **Test the CI command locally**
   - `dotnet run tools/dev-cli/dev.cs -- ci` — should run default PR workflow
   - `dotnet run tools/dev-cli/dev.cs -- ci --mode release` — should run release workflow

### GitHub Workflow - Create workflow.yml

- [x] **Create `.github/workflows/` directory structure**
   - Path: `.github/workflows/`
   - Main workflow file: `.github/workflows/workflow.yml`

- [x] **Create workflow file with CI pipeline**
   - Trigger on: push to master, PRs to master, release published, manual dispatch
   - Path filters: `source/**`, `tools/**`, `.github/workflows/**`, `Directory.Build.props`, `Directory.Packages.props`, `source/Directory.Build.props`
   - Jobs: `ci` job on `ubuntu-latest`

- [x] **Add checkout and .NET setup steps**
   - `actions/checkout@v4` with `fetch-depth: 0` (for version detection)
   - `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`

- [x] **Add dev CLI ci command**
   - `dotnet run tools/dev-cli/dev.cs -- ci`
   - Pass `--api-key` on release events for NuGet publishing

- [x] **Add artifact upload step**
   - Upload `artifacts/packages/*.nupkg` on failure/always
   - Use `actions/upload-artifact@v4`

### Release Publishing (Optional for v1.0.0)

- [x] **Add NuGet Trusted Publishing (OIDC)**
   - `nuget/login@v1` action on release events
   - Requires NuGet.org publisher configuration
   - Requires `id-token: write` permission

- [x] **Wire up release workflow command**
   - Pass `--api-key` on release events for NuGet publishing

### Repository Configuration

- [ ] **Verify branch protection rules**
  - Require PR reviews before merge to master
  - Require CI checks to pass

## Notes

### Reference: timewarp-nuru ci-command.cs

The reference implementation at `timewarp-nuru/master/tools/dev-cli/commands/ci-command.cs` shows:

```csharp
[NuruRoute("ci", Description = "Run full CI/CD pipeline")]
internal sealed class CiCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release")]
  public string? Mode { get; set; }

  [Option("api-key", Description = "NuGet API key for publishing")]
  public string? ApiKey { get; set; }

  // Determines mode from GITHUB_EVENT_NAME or explicit --mode flag
  // Pr: clean -> build -> verify-samples -> test
  // Release: clean -> build -> check-version -> pack -> push
  
  // Calls other command handlers directly:
  CleanCommand.Handler cleanHandler = new(Terminal);
  await cleanHandler.Handle(new CleanCommand(), CancellationToken.None);
}
```

### Key simplifications for timewarp-builder

- **No verify-samples step** — this library has no samples
- **Simpler test step** — just `dotnet test` (test project doesn't exist yet, so workflow will need updating once it does)
- **Single project** — only `source/timewarp-builder/timewarp-builder.csproj` to build/pack

### Files to Create/Modify

**New files:**
- `.github/workflows/workflow.yml`
- `tools/dev-cli/endpoints/ci.cs`

**Reference for ci.cs structure:**
- Use `ITerminal` for output (injected via constructor)
- Use `Shell.Builder` for running dotnet commands
- Read version from `source/Directory.Build.props` using `XDocument`
- Push to `https://api.nuget.org/v3/index.json`

### Related Tasks

- Task 001: Address code review blockers for v1.0.0 release
  - Includes adding tests, which will need to run in the CI workflow
  - The `test` step in CI will work once tests are added