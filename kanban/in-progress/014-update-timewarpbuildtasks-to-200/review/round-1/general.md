# Round 1 — general
**Date:** 2026-11-12
**Scope reviewed:** branch `task/014-update-timewarpbuildtasks-to-200` vs merge-base with `master` (`Directory.Packages.props`, surrounding MSBuild/CLI/test/hook/CI call sites, and task 014 kitchen)

## Summary

The product diff updates `TimeWarp.Build.Tasks` from `1.0.2` to `2.0.0` in `Directory.Packages.props`. Comparing the `TimeWarp.Build.Tasks` `1.0.2` and `2.0.0` NuGet packages and upstream `v1.0.2..v2.0.0` source diff confirms that `2.0.0` retargets `TimeWarp.Build.Tasks.dll` from `netstandard2.0` to `net11.0`, conditions `<UsingTask>` on `'$(MSBuildRuntimeType)' == 'Core'`, and adds error `TWBT0001` when MSBuild is non-Core or runs on a runtime older than `.NET 11.0`, while keeping `TimeWarpSetGitMetadata` and the emitted `CommitHash` / `CommitDate` `AssemblyMetadataAttribute` items unchanged. Because `timewarp-builder` targets `net11.0` exclusively and builds via the `dotnet` CLI on `.NET SDK 11.0.x` locally and in `.github/workflows/workflow.yml`, no MSBuild props/targets or consumer call-site changes are needed.

The implementer's `## Goal (implementer restatement)` matches the original task brief and requirements; there is no misread.

## Issues

None.

## Verified, no finding

- **Package diff & `assembly-metadata` contract:** Verified `~/.nuget/packages/timewarp.build.tasks/1.0.2` vs `2.0.0` (`build/TimeWarp.Build.Tasks.props`, `buildTransitive/TimeWarp.Build.Tasks.props`, `readme.md`, `.nuspec`) and `AssemblyMetadataCheck` in `timewarp-ganda` (`Directory.Build.props:79` keeps `<PackageReference Include="TimeWarp.Build.Tasks" PrivateAssets="all" />` with `"suppressParent": "All"`, so `TimeWarp.Build.Tasks` does not leak as a package dependency of `TimeWarp.Builder`).
- **Library builds & packed `nupkg`:** Non-incremental `Release` and `Debug` builds of `source/timewarp-builder/timewarp-builder.csproj` succeed with 0 warnings and 0 errors. `source/timewarp-builder/obj/project.assets.json` resolves `TimeWarp.Build.Tasks/2.0.0` (`build/net11.0/TimeWarp.Build.Tasks.dll` and `buildTransitive/net11.0/TimeWarp.Build.Tasks.dll`). `TimeWarp.Builder.2.0.0.nupkg` keeps `<version>2.0.0</version>`, contains the expected 8 deterministic entries (`_rels/.rels`, `TimeWarp.Builder.nuspec`, `lib/net11.0/TimeWarp.Builder.dll`, `lib/net11.0/TimeWarp.Builder.xml`, `logo.png`, `readme.md`, `[Content_Types].xml`, `package/services/metadata/core-properties/nuget.psmdcp`), has an empty `net11.0` `<dependencies>` group, and embeds `CommitHash` (`091152d638cda8cc698d0d8d80ae9ec02d8f61e0`) and `CommitDate` (`2026-11-12T10:16:44`) matching `HEAD`.
- **Dev CLI, tests, and git hooks:** `dotnet build tools/dev-cli/dev.cs --no-incremental` and Release NativeAOT `dotnet publish tools/dev-cli/dev.cs -c Release` succeed with 0 warnings and 0 errors, resolving `TimeWarp.Build.Tasks/2.0.0` and stamping `CommitHash`/`CommitDate` (`v1.0.0+091152d638cda8cc698d0d8d80ae9ec02d8f61e0`). All 6 Jaribu test runfiles (21 tests total) execute and pass, and all 5 `.githooks/*.cs` runfiles build with 0 warnings and 0 errors.
- **Currency & repo audit:** `ganda nuget outdated` reports all 15 central packages up to date (`All packages are up to date!`), and `ganda repo audit` passes all 43 repo-local checks (`assembly-metadata`, `cpm-consistency`, `global-usings-analyzer`, `analyzer-suppressions`, etc.; only `self-hosted-runner` requires `gh repo view` network access outside the review sandbox).
