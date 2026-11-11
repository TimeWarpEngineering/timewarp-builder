# Round 2 — merged findings
**Date:** 2026-11-11
**Sources:** general
**Scope:** fix delta `3cefd8c..044cfce` (`tools/dev-cli/Directory.Build.props`, `.timewarp/suppression-baseline.json`, and task 013 kitchen)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- **M1 — Severity: bug — Status: fixed (verified).** `ExemptPackageContentFromAnalyzers` in `tools/dev-cli/Directory.Build.props` scopes `dotnet_analyzer_diagnostic.severity = none` exclusively to `@(Compile)` items under `$(NuGetPackageRoot)` (the 26 `TimeWarp.Nuru.DevCli` `4.0.0-beta.12` content files), keeping `TWA0038` and all other analyzers active on repo-owned `tools/dev-cli/` sources. Redundant DevCli-only `<NoWarn>` IDs were removed, lowering `.timewarp/suppression-baseline.json` from 81 to 68 total suppressions. Verified via `dotnet build tools/dev-cli/dev.cs --no-incremental` (0 warnings, 0 errors), NativeAOT `dotnet publish tools/dev-cli/dev.cs -c Release` (0 warnings, 0 errors), library Release build, all 6 Jaribu test runfiles, and `ganda repo audit`.

## Issues

None new.

## Duplicates / conflicts

- None (single reviewer).
