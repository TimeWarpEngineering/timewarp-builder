# Round 1 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/Directory.Build.props:50
- Description: Bumping `TimeWarp.SourceGenerators` to `1.0.0-beta.14` activates `TWA0038` (`FileLengthAnalyzer`, warning enabled by default, 500-line cap) across all projects importing root `Directory.Build.props`. Compiling `tools/dev-cli/dev.cs` pulls in `TimeWarp.Nuru.DevCli` `4.0.0-beta.12` C# `contentFiles` from `$(NuGetPackageRoot)`, where `endpoints/release-command.cs` (609 lines) and `services/nuget-version-service.cs` (704 lines) exceed 500 lines and fail the build with `error TWA0038` under `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. `./bin/dev build` did not catch this because `bin/dev` is a pre-built gitignored AOT binary that only builds `source/timewarp-builder/timewarp-builder.csproj`, whereas `.github/workflows/workflow.yml` runs `dotnet run --file tools/dev-cli/dev.cs -- workflow` on a fresh checkout and will fail in CI.
- Suggestion: Add the `ExemptPackageContentFromAnalyzers` `BeforeTargets="CoreCompile"` target in `tools/dev-cli/Directory.Build.props` (matching `timewarp-architecture` task 470 and `timewarp-terminal/tools/dev-cli/Directory.Build.props:86-109`) so only `@(Compile)` items under `$(NuGetPackageRoot)` receive `dotnet_analyzer_diagnostic.severity = none` via an intermediate `.globalconfig`, keeping `TWA0038` and `TWA0004` active on repo-owned `tools/dev-cli` sources without raising `.timewarp/suppression-baseline.json` (do **not** suppress `TWA0038` in `<NoWarn>` or `.editorconfig`). Optionally drop any now-redundant DevCli-only `IDE*` / `RCS1194` `<NoWarn>` ids and run `ganda repo audit --fix` to lower the baseline. Verify `dotnet build tools/dev-cli/dev.cs --no-incremental` and `dotnet run --file tools/dev-cli/dev.cs -- self-install`, and add `dotnet build tools/dev-cli/dev.cs` to `task.md` `### How to validate`.
- Source: general
- Disposition notes: Fixed in the round-1 fix pass. Added the `ExemptPackageContentFromAnalyzers` target to `tools/dev-cli/Directory.Build.props`, so analyzers are off only for `@(Compile)` items under `$(NuGetPackageRoot)`. Dropped the now-redundant `RCS1194`, `CA5399`, and DevCli-only `IDE*` ids from `<NoWarn>`, keeping `IDE0211`. `ganda repo audit --fix` lowered the suppression baseline. `dotnet build tools/dev-cli/dev.cs --no-incremental` and `self-install` are clean.

## Duplicates / conflicts

- None (single reviewer).
