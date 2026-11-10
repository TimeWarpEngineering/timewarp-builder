# Fix repo audit failures

## Description

`ganda repo audit` on master reported 6 failures (4 errors, 2 warnings). Make the audit pass.

Scope grew on 2026-11-10: .NET 11 went GA and Steven asked for everything in the repo to move to .NET 11, so the library drops net10.0 in this same task.

## Checklist

- [x] `ganda repo audit --fix`: suppression baseline, Nuru 4.0.0-beta.12, `/.timewarp/signals/` gitignore, refreshed git hooks
- [x] Migrate dev CLI endpoints to the Nuru beta.12 handler API (`IRequestHandler`, `ValueTask<Unit>`)
- [x] `workflow-pwsh-shell`: default shell pwsh; convert `Run CI Pipeline` and `Keep last two Packages artifacts` from bash
- [x] `ci-code-coverage`: `dev test --coverage` (dotnet-coverage, Cobertura) and a `coverage` upload-artifact step
- [x] Target net11.0 only (library, tests, dev CLI, git hooks); version 2.0.0
- [x] Amuru / Amuru.Tools 3.0.0-beta.2, Terminal 2.0.0-beta.1, NetAnalyzers 11.0.100
- [x] Workflow installs only the .NET 11 SDK from the GA channel
- [x] Docs say net11.0 only (`AGENTS.md`, `readme.md`, releasing guide)
- [x] `ganda repo audit` passes; build, tests, pack and check-version green locally
- [ ] CI green on PR #12 with the GA SDK channel

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-10)
- Implementation: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-10), via sub-agents

## Notes

- Tests are Jaribu runfiles run with `dotnet run`, not an MTP project, so the audit's `global.json` / MTP flag guidance does not apply. Coverage is collected by wrapping each runfile in `dotnet-coverage collect` (local tool pinned in `.config/dotnet-tools.json`).
- Amuru, Amuru.Tools and Terminal were first held at 2.0.0 / 2.0.0 / 1.0.2 because the net11-only previews broke the net10.0 git hook runfiles. With the repo on net11.0 that hold-back is gone. The betas are the newest published net11 versions; no stable net11 release exists yet.
- Version 2.0.0: dropping net10.0 from the shipped package is a breaking change.
- The local SDK is still 11.0.100-rc.3. When this was written the .NET release metadata did not list a GA 11.0 SDK yet, so `actions/setup-dotnet` with `11.0.x` and no preview quality may fail until it does.

## Results

- `ganda repo audit`: 43 passed, 0 failed, 3 skipped.
- `dev build`: 0 warnings, 0 errors. `dev test --coverage`: all 6 test files pass. `dev pack`: `TimeWarp.Builder.2.0.0.nupkg` holds only `lib/net11.0`. `dev check-version`: 2.0.0 is safe to release.
- PR #12 CI was green before the .NET 11 move (pull_request run 42844436248): the pwsh `Run CI Pipeline` step and the `coverage` upload ran. The artifact-pruning step only runs on a master push, so its first real run is after merge; it was exercised locally against a fake `gh`.
