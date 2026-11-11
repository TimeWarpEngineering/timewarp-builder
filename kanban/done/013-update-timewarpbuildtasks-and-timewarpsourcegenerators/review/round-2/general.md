# Round 2 — general
**Date:** 2026-11-11
**Scope reviewed:** fix delta since round 1, `3cefd8c..044cfce` (`tools/dev-cli/Directory.Build.props`, `.timewarp/suppression-baseline.json`, and task 013 kitchen)

## Summary

The round-1 fix (`044cfce`) resolves `M1` cleanly and introduces no new issues. `tools/dev-cli/Directory.Build.props` adds the `ExemptPackageContentFromAnalyzers` `BeforeTargets="CoreCompile"` target (matching `timewarp-terminal/tools/dev-cli/Directory.Build.props`), emitting an intermediate `package-content-analyzers.globalconfig` that sets `dotnet_analyzer_diagnostic.severity = none` solely for `@(Compile)` items under `$(NuGetPackageRoot)` while keeping `TWA0038`, `TWA0004`, and Roslyn/Roslynator analyzers active on this repo's own `tools/dev-cli/` sources. Thirteen DevCli-only `<NoWarn>` IDs (`CA5399`, `RCS1194`, `IDE0005`, `IDE0022`, `IDE0046`, `IDE0052`, `IDE0055`, `IDE0058`, `IDE0065`, `IDE0066`, `IDE0078`, `IDE0160`, `IDE0290`) were removed and `.timewarp/suppression-baseline.json` was lowered accordingly.

The implementer's `## Goal (implementer restatement)` matches the original task brief and requirements; there is no misread.

## Issues

None.

## Verified, no finding

- **M1 (`tools/dev-cli/Directory.Build.props`):** `dotnet build tools/dev-cli/dev.cs --no-incremental` and `dotnet publish tools/dev-cli/dev.cs -c Release` (NativeAOT publish, matching `self-install` and CI `workflow.yml`) both succeed with 0 warnings and 0 errors, and the published native `dev` binary executes `--help` with all 10 commands registered (`v1.0.0+044cfceedcf9a8c554fa52ac72a32f741bfed421`).
- **Scoping of `ExemptPackageContentFromAnalyzers`:** Inspected the generated `package-content-analyzers.globalconfig`; it contains only the 26 `TimeWarp.Nuru.DevCli` `4.0.0-beta.12` C# files under `$(NuGetPackageRoot)` and zero repo-owned `tools/dev-cli/` files. Path separators are normalized to `/` on both `$(NuGetPackageRoot)` and `%(FullPath)`, and `<FileWrites>` + `WriteOnlyWhenDifferent="true"` preserve clean and incremental build semantics.
- **Suppression baseline (`.timewarp/suppression-baseline.json`):** Total repo suppressions dropped from 81 to 68; `ganda repo audit` reports `analyzer-suppressions` PASS (`68 analyzer suppressions, none above baseline`), along with PASS on all 43 local checks.
- **Library and tests:** `dotnet build source/timewarp-builder/timewarp-builder.csproj -c Release --no-incremental` succeeds with 0 warnings and 0 errors (`TimeWarp.Builder.2.0.0.nupkg` unchanged), and all 6 Jaribu test runfiles (21 tests total) build and pass.
