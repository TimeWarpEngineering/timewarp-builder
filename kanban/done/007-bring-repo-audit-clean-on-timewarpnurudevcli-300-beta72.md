# Bring repo audit-clean on TimeWarp.Nuru.DevCli 3.0.0-beta.72

## Description

Org wave (timewarp-nuru 458-010 remediation + DevCli 3.0.0-beta.72 adoption —
they are the same wave: the audit's `nuru` check went red org-wide when
beta.72 shipped, by design). Passing `ganda repo audit` now means adopting the
full release toolkit: `dev release`, promotion gates, attestation verifier,
trusted-publishing probe, derived package sets.

## Checklist

- [x] `ganda repo audit --fix` (bumps TimeWarp.Nuru/DevCli to latest, fixes kebab/structure where fixable)
- [x] Verify Directory.Packages.props pins TimeWarp.Nuru.DevCli (and TimeWarp.Nuru where referenced) at 3.0.0-beta.72
- [x] Build — NURU050 names any missing DI registration (e.g. `IPackableProjectService`); add per the DevCli readme migration notes (CS0101 local-CiMode note also applies)
- [x] `dev self-install` (AOT binary is a snapshot; new commands like `release` are absent until reinstalled)
- [x] `ganda repo audit` → PASSES ALL CHECKS (if a check is structurally unfixable here, record it explicitly with a reason instead of forcing)
- [x] Smoke: `dev --help` shows `release`; `dev check-version` derives the packable set (publishers only)
- [x] Commit everything (audit fixes, props, dev.cs, kanban) — local commits fine; ride the repo's normal merge flow

## Notes

Created 2026-08-08 from the nuru 458 program session.

## Session

- Implementation: grok 2026-08-08 — assess (20/2) → --fix → DevCli DI → kebab → self-install → green

## Results

### Outcome
Audit-clean on Nuru/DevCli 3.0.0-beta.72. `release` in help; check-version reports TimeWarp.Builder packable set (version already published — expected).

### Before
Passed 20 / Failed 2: kebab (workspace + LICENSE), nuru beta.71

### After
Passed 22 / Failed 0 (1 skip: no #:project)

### Files
- Directory.Packages.props — Nuru/DevCli 3.0.0-beta.72; Amuru 1.0.0; Amuru.Tools beta.2; Terminal
- tools/dev-cli/* — DevCli package, DI, exclude local clean/check-version/self-install, workflow injects services, drop local CiMode
- kebab: workspace files + LICENSE→license

### How to validate
```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-builder/dev
ganda repo audit
./bin/dev --help
./bin/dev check-version
dotnet build tools/dev-cli/dev.cs
```
Expect: all audit PASS; help includes `release`; check-version lists TimeWarp.Builder.
