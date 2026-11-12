# Update TimeWarp.Build.Tasks to 2.0.0

## Description

`ganda nuget outdated` on master (2026-11-12) reports one outdated package: TimeWarp.Build.Tasks 1.0.2 -> 2.0.0. This is a major version, so expect breaking changes.

## Requirements

- Bump TimeWarp.Build.Tasks to 2.0.0 in `Directory.Packages.props`.
- Read what changed in 2.0.0 (release notes, package contents) and adapt this repo: MSBuild props/targets, the `assembly-metadata` audit check's expectations, and anything else that consumes the package.
- If the bump breaks the build, fix the call sites; do not keep the old pin.
- The shipped TimeWarp.Builder package content should be unchanged apart from what the new Build.Tasks itself emits. Say in Results if the nupkg content changed. No version bump for TimeWarp.Builder: 2.0.0 is on master and not yet released.

## Goal (implementer restatement)

Move the central pin for TimeWarp.Build.Tasks from 1.0.2 to 2.0.0 and make the repo build cleanly on it without falling back to the old version. Done means:

- `Directory.Packages.props` pins 2.0.0 and `ganda nuget outdated` reports nothing.
- Any breaking change in 2.0.0 that touches this repo (props/targets, the git metadata the `assembly-metadata` audit check expects, the dev CLI build) is handled.
- `./bin/dev build` has 0 warnings and 0 errors; `./bin/dev test`, `./bin/dev pack`, `dotnet build tools/dev-cli/dev.cs --no-incremental` and `ganda repo audit` all pass.
- TimeWarp.Builder's version stays at 2.0.0, and Results says whether the nupkg content changed.

## Checklist

- [x] Bump the pin in `Directory.Packages.props`
- [x] Adapt to any breaking changes in Build.Tasks 2.0.0
- [x] `ganda nuget outdated` reports nothing outdated
- [x] `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` and `./bin/dev pack` green
- [x] `dotnet build tools/dev-cli/dev.cs --no-incremental` green (task 013's review caught a failure only this build showed)
- [x] `ganda repo audit` passes

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-12)
- Implement oracle: implemented by implementer-cursor (cursor, vendor anthropic, model claude-opus-5-5-medium), session not reported, max-turns uncapped (no --max-turns flag; bound 500), Done — 2026-11-12T10:16:44Z
- Review oracle round 1: reviewer-cursor (cursor, model gemini-4-carbon-high), headless, session not reported, effort 1, roster general, max-turns uncapped (no --max-turns flag; bound 80) — 2026-11-12. 0 findings; disposition `clean` (`review/disposition.md`). Result: Done.
- Review oracle: review by reviewer-cursor (cursor, vendor google, model gemini-4-carbon-high), session not reported, max-turns uncapped (no --max-turns flag; bound 80), artifacts general.md, merged.md, disposition.md — 2026-11-12T10:30:25Z

## Results

The pin in `Directory.Packages.props` is now TimeWarp.Build.Tasks 2.0.0. Nothing else in the repo needed to change.

What changed in 2.0.0, from comparing the 1.0.2 and 2.0.0 package contents (`build/` and `buildTransitive/` props, readme):

- The task assembly moved from `netstandard2.0` to `net11.0`. The `UsingTask` only loads when `MSBuildRuntimeType` is `Core`.
- A new error, `TWBT0001`, stops the build when MSBuild is not on .NET 11+ (Visual Studio, `MSBuild.exe`, older SDKs). `TimeWarpEnableGitMetadata=false` turns it off.
- The git metadata targets and the attributes they emit (`CommitHash`, `CommitDate`) did not change.

This repo already builds with the dotnet CLI on SDK 11.0.100 and targets net11.0 only, so the new requirement is already met. The `assembly-metadata` audit check still passes ("TimeWarp.Build.Tasks is referenced").

nupkg content: unchanged. I built and packed TimeWarp.Builder 2.0.0 on 1.0.2, then again on 2.0.0 from a clean `bin/`/`obj/`. All 8 files in the two packages are byte-identical, including `TimeWarp.Builder.dll` and the nuspec. One pack made over a stale incremental build had a different dll; a clean rebuild matched the baseline exactly. The TimeWarp.Builder version stays at 2.0.0.

Gates on 2.0.0: `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` (6 test files passed), `./bin/dev pack`, `dotnet build tools/dev-cli/dev.cs --no-incremental`, `ganda nuget outdated` ("All packages are up to date!") and `ganda repo audit` (all checks pass).

Boy-scout audit fix: `vscode-window-icon` gave an advisory because `.vscode/settings.json` was missing. `ganda repo audit --fix` created it. The file is gitignored, so no committed change came from this.

### How to validate

Smoke:

```bash
rg -n 'TimeWarp.Build.Tasks' Directory.Packages.props
rm -rf source/timewarp-builder/bin source/timewarp-builder/obj artifacts/packages
./bin/dev build && ./bin/dev test && ./bin/dev pack
dotnet build tools/dev-cli/dev.cs --no-incremental
rg -n '"TimeWarp.Build.Tasks/' source/timewarp-builder/obj/project.assets.json
ganda nuget outdated
ganda repo audit
```

Expect:

- `Directory.Packages.props` pins `Version="2.0.0"`, and `project.assets.json` resolves `TimeWarp.Build.Tasks/2.0.0` with `build/net11.0/TimeWarp.Build.Tasks.dll`.
- Build reports 0 Warning(s) and 0 Error(s). Tests pass, pack produces `artifacts/packages/TimeWarp.Builder.2.0.0.nupkg`, and the dev-cli build succeeds. No `TWBT0001` error appears.
- `strings` on the packed `lib/net11.0/TimeWarp.Builder.dll` shows the `CommitHash` and `CommitDate` metadata.
- `ganda nuget outdated` prints "All packages are up to date!" and `ganda repo audit` prints "Repository passes all audit checks."

### Review

- Round 1 (effort 1, roster `general`): verified `TimeWarp.Build.Tasks` `1.0.2` -> `2.0.0` package/source diff, non-incremental `Release`/`Debug` library builds, `tools/dev-cli/dev.cs` non-incremental build and Release NativeAOT publish, all 6 Jaribu test runfiles, all 5 `.githooks/*.cs` runfiles, packed `TimeWarp.Builder.2.0.0.nupkg` layout/nuspec/`CommitHash`/`CommitDate` metadata, `ganda nuget outdated`, and `ganda repo audit`; 0 findings raised.
- **Review disposition: clean.** Rounds: 1, effort: 1, roster: `general`. Final counts: `bug` 0 open / 0 fixed / 0 wontfix; `suggestion` 0 open / 0 fixed / 0 wontfix; `nit` 0 open / 0 fixed / 0 wontfix. No exceptions (`wontfix`) and no escalations. Goal restatement matches the original task; no misread.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Review

- Round 1: Done

## Notes

- The repo targets net11.0 only (task 012). Task 013 took Build.Tasks from 1.0.0 to 1.0.2.
- Steven asked for Cursor Opus 5.5 to implement and Cursor Carbon to review.
