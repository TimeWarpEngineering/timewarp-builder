# Fix repo audit failures

## Description

`ganda repo audit` on master reported 6 failures (4 errors, 2 warnings). Make the audit pass.

## Checklist

- [x] `ganda repo audit --fix`: suppression baseline, Nuru 4.0.0-beta.12, `/.timewarp/signals/` gitignore, refreshed git hooks
- [x] Migrate dev CLI endpoints to the Nuru beta.12 handler API (`IRequestHandler`, `ValueTask<Unit>`)
- [x] `workflow-pwsh-shell`: default shell pwsh; convert `Run CI Pipeline` and `Keep last two Packages artifacts` from bash
- [x] `ci-code-coverage`: `dev test --coverage` (dotnet-coverage, Cobertura) and a `coverage` upload-artifact step
- [x] `ganda repo audit` passes; build and tests green
- [ ] CI green on the PR (the converted workflow steps cannot be run locally)

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-10)
- Implementation: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-10), via a sub-agent

## Notes

- Tests are Jaribu runfiles run with `dotnet run`, not an MTP project, so the audit's `global.json` / MTP flag guidance does not apply. Coverage is collected by wrapping each runfile in `dotnet-coverage collect` (local tool pinned in `.config/dotnet-tools.json`).
- The first `--fix` run also moved TimeWarp.Amuru / Amuru.Tools to 3.0.0-beta.2 and TimeWarp.Terminal to 2.0.0-beta.1. Those are net11.0-only and broke the `.githooks/*.cs` runfiles, which build for net10.0 (NU1202). They are back at 2.0.0 / 2.0.0 / 1.0.2, which is what Nuru 4.0.0-beta.12 itself depends on. The audit passes with these pins and a second `--fix` run leaves them alone.
- No version bump: 1.1.0 is already released, but this change touches only CI, dev tooling and repo scaffold, not the shipped package. `check-version` runs only in release mode.

## Results

- `ganda repo audit`: 43 passed, 0 failed, 3 skipped.
- `dev build`: 0 errors. `dev test --coverage`: all 6 test files pass, 6 Cobertura files in `artifacts/coverage`.
- The "Keep last two Packages artifacts" pwsh step was exercised locally against a fake `gh`; the workflow as a whole is unverified until CI runs.
