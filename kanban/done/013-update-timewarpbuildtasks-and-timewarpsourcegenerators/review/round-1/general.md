# Round 1 — general
**Date:** 2026-11-11
**Scope reviewed:** branch `task/013-update-timewarpbuildtasks-and-timewarpsourcegenera` vs merge-base with `master` (`Directory.Packages.props`, surrounding MSBuild/CLI/test/hook call sites, and task 013 kitchen)

## Summary

The product diff bumps `TimeWarp.Build.Tasks` (`1.0.0` -> `1.0.2`) and `TimeWarp.SourceGenerators` (`1.0.0-beta.12` -> `1.0.0-beta.14`) in `Directory.Packages.props`. `ganda nuget outdated` confirms all 15 central pins are up to date, `source/timewarp-builder/timewarp-builder.csproj` builds non-incrementally with 0 warnings and 0 errors, the packed `TimeWarp.Builder.2.0.0.nupkg` content is unchanged (`<Version>2.0.0</Version>` stays valid), all 6 Jaribu test runfiles pass, all 5 `.githooks/*.cs` runfiles build clean, and repo-local `ganda repo audit` checks pass (including `global-usings-analyzer`, `assembly-metadata`, `cpm-consistency`, and `analyzer-suppressions`).

However, compiling `tools/dev-cli/dev.cs` from source is broken by `TWA0038` (`FileLengthAnalyzer`, enabled by default starting in `TimeWarp.SourceGenerators` `1.0.0-beta.13`) firing on two >500-line NuGet `contentFiles` shipped by `TimeWarp.Nuru.DevCli` `4.0.0-beta.12`. This went unnoticed because `./bin/dev` is a gitignored pre-built AOT binary from before the bump, whereas CI runs `dotnet run --file tools/dev-cli/dev.cs -- workflow` on every PR and push.

The implementer's `## Goal (implementer restatement)` matches the original task brief and requirements; there is no misread.

## Issues

### Issue 1 — Severity: bug
- File: tools/dev-cli/Directory.Build.props:50
- Description: `dotnet build tools/dev-cli/dev.cs --no-incremental` (and therefore `dotnet run --file tools/dev-cli/dev.cs -- workflow` in `.github/workflows/workflow.yml:107` and `self-install` in `tools/dev-cli/endpoints/workflow-command.cs:152`) fails under `TimeWarp.SourceGenerators` `1.0.0-beta.14` + `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` with two `error TWA0038` diagnostics on `TimeWarp.Nuru.DevCli` `4.0.0-beta.12` NuGet `contentFiles` compiled from `$(NuGetPackageRoot)`:
  - `.../timewarp.nuru.devcli/4.0.0-beta.12/content/any/endpoints/release-command.cs(72,1): error TWA0038: File 'release-command.cs' has 609 lines, over the 500-line limit; extract a responsibility into its own type`
  - `.../timewarp.nuru.devcli/4.0.0-beta.12/content/any/services/nuget-version-service.cs(73,1): error TWA0038: File 'nuget-version-service.cs' has 704 lines, over the 500-line limit; extract a responsibility into its own type`
  Running `./bin/dev build` only invokes `dotnet build` on `source/timewarp-builder/timewarp-builder.csproj` using the stale pre-bump `bin/dev` AOT binary, so `tools/dev-cli/dev.cs` was never recompiled during validation. On a fresh CI checkout (`bin/dev` is gitignored), PR and master runs will fail immediately.
- Suggestion: Add the `ExemptPackageContentFromAnalyzers` MSBuild target (`BeforeTargets="CoreCompile"`, conditioned on `'$(NuGetPackageRoot)' != ''`) to `tools/dev-cli/Directory.Build.props`—matching `timewarp-architecture` task 470 (`tools/Directory.Build.props`) and `timewarp-terminal` (`tools/dev-cli/Directory.Build.props:86-109`)—to emit an intermediate `package-content-analyzers.globalconfig` setting `dotnet_analyzer_diagnostic.severity = none` only for `@(Compile)` items whose `%(FullPath)` starts with `$(NuGetPackageRoot)`. Do **not** add `TWA0038` to `<NoWarn>` or `.editorconfig` (that would silence `TWA0038` on this repo's own `tools/dev-cli` sources and trip the `analyzer-suppressions` audit check). Optionally, if the `IDE*` / `RCS1194` `<NoWarn>` entries in `tools/dev-cli/Directory.Build.props` that existed only for `TimeWarp.Nuru.DevCli` content files are now redundant (as in `timewarp-terminal` commit `34d7f03`), remove them and run `ganda repo audit --fix` to lower `.timewarp/suppression-baseline.json`. Verify with `dotnet build tools/dev-cli/dev.cs --no-incremental` and `dotnet run --file tools/dev-cli/dev.cs -- self-install`, and include `dotnet build tools/dev-cli/dev.cs` in `task.md`'s `### How to validate` smoke command.
- Status: open
