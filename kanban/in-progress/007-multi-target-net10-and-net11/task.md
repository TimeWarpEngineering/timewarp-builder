# Multi-target net10.0 and net11.0 and clear repo audit

## Description

TimeWarp.Builder currently targets only net10.0. Ship the library for net10.0 and net11.0. The open journal-ignore task 006 is obsolete (not relevant) and shares its id with the done trusted-publishing kitchen, so archive that open kitchen as 008. Clear `ganda repo audit` errors in this same task.

## Goal (implementer restatement)

Ship TimeWarp.Builder so the library compiles and packs for both net10.0 and net11.0. Keep the dev CLI and the Jaribu test runfiles on one framework so those executables still run. The open journal-ignore kitchen that reused id 006 is obsolete: archive it as 008, and leave the finished trusted-publishing kitchen that already owns id 006 in done. `ganda repo audit` must report no Error on this worktree.

Acceptance:

- The library's target frameworks are net10.0 and net11.0.
- The dev CLI and the test runfiles stay on a single target framework.
- The obsolete to-do 006 journal kitchen is archived as 008.
- The done 006 trusted-publishing kitchen is unchanged.
- `ganda repo audit` has no Error.
- The library builds for net10.0 and for net11.0.
- The test runfiles pass.
- The package version is 1.1.0, which is newer than the published 1.0.0, so this multi-target build can be released.

## Checklist

- [x] Library TargetFrameworks is net10.0;net11.0
- [x] Dev CLI and test runfiles stay single-TFM so they still run
- [x] Obsolete to-do 006 journal kitchen archived as 008
- [x] Done 006 trusted-publishing kitchen unchanged
- [x] ganda repo audit has no Error
- [x] Library builds for net10.0 and net11.0
- [x] Tests pass
- [x] Package version is 1.1.0, above the published 1.0.0

## Session

- Created: 01a1b27c-6184-7133-a255-0dd9b04df0a0 (2026-11-06)
- 2026-11-07: user asked to archive 006 as not relevant, fix the audit, and multi-target net10 and net11 in this task. Session id 01a1b27c-6184-7133-a255-0dd9b04df0a0. The blank 007 kitchen was created only because id 006 was ambiguous.
- `ganda repo audit` passes on this worktree. `ganda kanban reconcile` landed the 007 kitchen. Audit `--fix` pinned Amuru 3.0.0-beta.1, which is net11.0 only and cannot restore the net10.0 hooks or dev CLI. Nuru 4.0.0-beta.11 depends on Amuru 2.0.0, so the central pin is 2.0.0. Handler methods return `Task<Unit>` so they match Mediator contracts 14.
- Library build produced net10.0 and net11.0. Dev CLI and the six Jaribu runfiles stayed on net10.0 and passed.
- 2026-11-07: user asked to launch this task, run `ganda repo audit`, and roll the result into 007. Re-ran the audit on this worktree before the walk. It passes. There is no new audit diff to commit. Do not open a sibling task for audit.
- Implementer: grok session 01a1b3c6-a57d-73a1-858e-d6a8d75a9fc9 (2026-11-07). Verified the multi-target build, tests, and audit, then set the package version to 1.1.0 because 1.0.0 is already tagged and on NuGet.org.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1b3c6-a57d-73a1-858e-d6a8d75a9fc9, max-turns 500, Done — 2026-11-07T02:20:31Z

## Results

- The library target frameworks are `net10.0;net11.0` in the root `Directory.Build.props`. `./bin/dev build` (Release) produced `TimeWarp.Builder.dll` for both frameworks and packed `artifacts/packages/TimeWarp.Builder.1.1.0.nupkg` with `lib/net10.0` and `lib/net11.0`. 0 warnings, 0 errors.
- The dev CLI and the Jaribu runfiles stay on `net10.0`. `tools/dev-cli/Directory.Build.props` and `tests/Directory.Build.props` set `TargetFramework` to net10.0 and clear `TargetFrameworks`. `./bin/dev test` ran all 6 runfiles and all passed.
- The obsolete journal-ignore kitchen is `kanban/archived/008-ignore-routine-journals-so-worktree-gc-is-not-dirty/`. The done trusted-publishing kitchen `kanban/done/006-add-trusted-publishing-probe-mode-to-workflowyml.md` has no diff against master.
- `ganda repo audit` on this worktree: Passed 37, Failed 0, Skipped 3.
- Package version is 1.1.0 in `source/Directory.Build.props`. GitHub release `v1.0.0` exists, and `dotnet package search` lists `TimeWarp.Builder` 1.0.0 on nuget.org. `./bin/dev check-version` still prints ready for 1.0.0 because it looks for `| 1.0.0 |` and the search table pads the version column (`| 1.0.0        |`).
- TimeWarp.Amuru stays at 2.0.0. Audit `--fix` had pinned 3.0.0-beta.1, which is net11.0 only, and Nuru 4.0.0-beta.11 depends on Amuru 2.0.0. Dev CLI handlers return `Task<Unit>` so they match Mediator contracts 14.

### How to validate

**Smoke:**

```bash
./bin/dev build
unzip -l artifacts/packages/TimeWarp.Builder.1.1.0.nupkg | grep 'lib/net'
./bin/dev test
ganda repo audit
git diff origin/master -- kanban/done/006-add-trusted-publishing-probe-mode-to-workflowyml.md
```

**Expect:** Build succeeds with 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. The package lists `lib/net10.0/TimeWarp.Builder.dll` and `lib/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. Audit reports Failed: 0. The done 006 diff is empty. `kanban/archived/008-ignore-routine-journals-so-worktree-gc-is-not-dirty/task.md` is present. `tools/dev-cli/Directory.Build.props` and `tests/Directory.Build.props` contain `<TargetFramework>net10.0</TargetFramework>` and an empty `<TargetFrameworks>`.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root.

**Not in scope:** pushing the package to NuGet.org. That is the release cut after this branch merges. Confirm the published version with `dotnet package search TimeWarp.Builder --exact-match --prerelease --source https://api.nuget.org/v3/index.json`, not with `./bin/dev check-version`, until that column match is fixed.
