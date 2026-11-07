# Multi-target net10.0 and net11.0 and clear repo audit

## Description

TimeWarp.Builder currently targets only net10.0. Ship the library for net10.0 and net11.0. The open journal-ignore task 006 is obsolete (not relevant) and shares its id with the done trusted-publishing kitchen, so archive that open kitchen as 008. Clear `ganda repo audit` errors in this same task.

## Goal (implementer restatement)

Ship TimeWarp.Builder so the library compiles and packs for both net10.0 and net11.0 until .NET 11 is GA, and so the GitHub Actions workflow can build that net11.0 target. The dev CLI and the Jaribu test runfiles target net11.0, which is the forward framework. The open journal-ignore kitchen that reused id 006 is obsolete: archive it as 008, and leave the finished trusted-publishing kitchen that already owns id 006 in done. `ganda repo audit` must report no Error on this worktree.

Acceptance:

- The library's target frameworks are net10.0 and net11.0.
- The dev CLI and the test runfiles target net11.0. The library keeps net10.0 and net11.0 until .NET 11 is GA.
- The obsolete to-do 006 journal kitchen is archived as 008.
- The done 006 trusted-publishing kitchen is unchanged.
- `ganda repo audit` has no Error.
- The library builds for net10.0 and for net11.0.
- The test runfiles pass.
- The package version is 1.1.0, which is newer than the published 1.0.0, so this multi-target build can be released.
- CI installs the .NET 10 SDK and the .NET 11 SDK. .NET 11 is still a release candidate, so its setup step requests preview quality. That quality is not applied to the .NET 10 step, because the 10.0 preview channel still resolves to the 10.0 RC rather than the GA SDK.

## Checklist

- [x] Library TargetFrameworks is net10.0;net11.0
- [x] Dev CLI and test runfiles target net11.0
- [x] Obsolete to-do 006 journal kitchen archived as 008
- [x] Done 006 trusted-publishing kitchen unchanged
- [x] ganda repo audit has no Error
- [x] Library builds for net10.0 and net11.0
- [x] Tests pass
- [x] Package version is 1.1.0, above the published 1.0.0
- [x] CI installs the .NET 10 GA SDK and the .NET 11 preview SDK in separate steps

## Session

- Created: 01a1b27c-6184-7133-a255-0dd9b04df0a0 (2026-11-06)
- 2026-11-07: user asked to archive 006 as not relevant, fix the audit, and multi-target net10 and net11 in this task. Session id 01a1b27c-6184-7133-a255-0dd9b04df0a0. The blank 007 kitchen was created only because id 006 was ambiguous.
- `ganda repo audit` passes on this worktree. `ganda kanban reconcile` landed the 007 kitchen. Audit `--fix` pinned Amuru 3.0.0-beta.1, which is net11.0 only and cannot restore the net10.0 hooks or dev CLI. Nuru 4.0.0-beta.11 depends on Amuru 2.0.0, so the central pin is 2.0.0. Handler methods return `Task<Unit>` so they match Mediator contracts 14.
- Library build produced net10.0 and net11.0. Dev CLI and the six Jaribu runfiles stayed on net10.0 and passed.
- 2026-11-07: user asked to launch this task, run `ganda repo audit`, and roll the result into 007. Re-ran the audit on this worktree before the walk. It passes. There is no new audit diff to commit. Do not open a sibling task for audit.
- Implementer: grok session 01a1b3c6-a57d-73a1-858e-d6a8d75a9fc9 (2026-11-07). Verified the multi-target build, tests, and audit, then set the package version to 1.1.0 because 1.0.0 is already tagged and on NuGet.org.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1b3c6-a57d-73a1-858e-d6a8d75a9fc9, max-turns 500, Done — 2026-11-07T02:20:31Z
- Review oracle round 1: reviewer-claude (Claude Opus 5.5, claude-opus-5-5) ran at effort 2 with a general-only roster, on 2026-11-07. It found 1 open bug (M1: CI installs only the .NET 10 SDK, so net11.0 fails with NETSDK1045) and requested a fix pass. Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 51443732-830e-4de6-af80-9da31cb89bb4, max-turns 120, artifacts general.md, merged.md — 2026-11-07T02:22:53Z
- Fix pass (implement-fix-2): grok session 01a1b3d0-e290-7a40-9524-f14af99b01a4 (2026-11-07). M1: the workflow now installs .NET 10.0.x and .NET 11.0.x. Preview quality is only on the 11 step, because one step applies that input to every version and the 10.0 preview channel is still 10.0.100-rc.2. The v4 install script dry-run resolves channel 10.0 to 10.0.403 and channel 11.0 with preview quality to 11.0.100-rc.3.26522.148. `./bin/dev build` and `./bin/dev test` passed on that RC SDK. `ganda kanban reconcile` landed the kitchen. `ganda repo audit` then passed: Passed 37, Failed 0, Skipped 3.
- Review oracle round 2: reviewer-claude (Claude Opus 5.5, claude-opus-5-5) ran at effort 1 with a general-only roster, turn cap 80, on 2026-11-07. It re-verified M1 as fixed and found no new issues. Disposition is clean. Artifacts: `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.
- Installed ganda changed during this walk: was 50128416 bytes 0ae910dc29e1318d800d9595d8d3462a946a1af23b0e841a6f5d3c20b91354a8; now 50128416 bytes 6ee9d7151768bb3ee32bb86afcd1f647f65d076cab7eb76aa698982f2e8657b2
- 2026-11-07: the net10.0 pin on the dev CLI and tests was a local choice, not a Nuru requirement. Nuru's repo targets net11.0. The published Nuru 4.0.0-beta.11 package still ships only `lib/net10.0`, and a net11.0 app can reference that asset. The user said the library ships both frameworks until .NET 11 is official, and net11.0 is the forward target for the dev CLI and tests.

## Results

- Review disposition: **clean**. There were 2 rounds, each with a general-only roster: round 1 at effort 2, round 2 at effort 1. The final counts are bug 0 open, 1 fixed, 0 wontfix; suggestion 0; nit 0. M1, where CI lacked the .NET 11 SDK, is fixed in `edc9d63`. Round 2 confirmed the fix with setup-dotnet v4.3.1 install-script dry-runs: 10.0 resolves to 10.0.403, 11.0 with preview quality resolves to 11.0.100-rc.3.26522.148, and 10.0 with preview quality would resolve to 10.0.100-rc.2. A local Release build of the library also succeeded for net10.0 and net11.0. Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

- Round 1 M1 is fixed in `.github/workflows/workflow.yml`. Setup .NET 10 installs `10.0.x` with no quality. Setup .NET 11 installs `11.0.x` with `dotnet-quality: preview`. The same step cannot carry preview quality for both, because that input applies to every version and the 10.0 preview channel still resolves to SDK 10.0.100-rc.2.25502.107. A dry-run of the `actions/setup-dotnet` v4.3.1 `install-dotnet.sh` resolves `--channel 10.0` to SDK 10.0.403 and `--channel 11.0 --quality preview` to SDK 11.0.100-rc.3.26522.148. The net11.0 asset is built with that RC SDK until .NET 11 reaches GA. Round 2 verified the fix. See `review/round-1/merged.md`.
- The library target frameworks are `net10.0;net11.0` in the root `Directory.Build.props`. `./bin/dev build` (Release) on SDK 11.0.100-rc.3.26522.148 produced `TimeWarp.Builder.dll` for both frameworks and packed `artifacts/packages/TimeWarp.Builder.1.1.0.nupkg` with `lib/net10.0` and `lib/net11.0`. 0 warnings, 0 errors. Re-run on 2026-11-07 during the fix pass: same result.
- The dev CLI and the Jaribu runfiles target `net11.0`. The library keeps both frameworks until .NET 11 is GA. `tools/dev-cli/Directory.Build.props` and `tests/Directory.Build.props` set `TargetFramework` to net11.0 and clear `TargetFrameworks`.
- The obsolete journal-ignore kitchen is `kanban/archived/008-ignore-routine-journals-so-worktree-gc-is-not-dirty/`. The done trusted-publishing kitchen `kanban/done/006-add-trusted-publishing-probe-mode-to-workflowyml.md` has no diff against master.
- `ganda repo audit` on this worktree: Passed 37, Failed 0, Skipped 3. Re-run after the CI fix and `ganda kanban reconcile`: same counts, including `kanban-landed`.
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
curl -fsSL -o /tmp/install-dotnet-v4.sh https://raw.githubusercontent.com/actions/setup-dotnet/v4.3.1/externals/install-dotnet.sh
bash /tmp/install-dotnet-v4.sh --channel 10.0 --dry-run
bash /tmp/install-dotnet-v4.sh --channel 11.0 --quality preview --dry-run
```

**Expect:** Build succeeds with 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. The package lists `lib/net10.0/TimeWarp.Builder.dll` and `lib/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. Audit reports Failed: 0. The done 006 diff is empty. `kanban/archived/008-ignore-routine-journals-so-worktree-gc-is-not-dirty/task.md` is present. `tools/dev-cli/Directory.Build.props` and `tests/Directory.Build.props` contain `<TargetFramework>net11.0</TargetFramework>` and an empty `<TargetFrameworks>`. The workflow has a Setup .NET 10 step with `dotnet-version: '10.0.x'` and no `dotnet-quality`, and a Setup .NET 11 step with `dotnet-version: '11.0.x'` and `dotnet-quality: 'preview'`. The 10.0 dry-run names SDK 10.0.403. The 11.0 preview dry-run names SDK 11.0.100-rc.3.26522.148. GitHub Actions on the pull request is the run that proves the hosted runner; this walk does not open the pull request.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root.

**Not in scope:** pushing the package to NuGet.org. That is the release cut after this branch merges. Confirm the published version with `dotnet package search TimeWarp.Builder --exact-match --prerelease --source https://api.nuget.org/v3/index.json`, not with `./bin/dev check-version`, until that column match is fixed.

## Review

- Round 1: M1 fixed in `.github/workflows/workflow.yml`.
- Round 2: Done
