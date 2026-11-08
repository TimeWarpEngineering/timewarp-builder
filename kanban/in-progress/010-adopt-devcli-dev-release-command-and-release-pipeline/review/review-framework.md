# Review framework

## Budget (by-diff)

- Lines changed: 167
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Framework — task 010

**Date:** 2026-11-09
**Host task:** kanban/in-progress/010-adopt-devcli-dev-release-command-and-release-pipeline/
**Diff scope:** branch `task/010-adopt-devcli-dev-release-command-and-release-pipel` vs merge-base with `master` (commits 46b5e49..7059787)
**Plan / brief:** adopt TimeWarp.Nuru.DevCli (clean, check-version, release, self-install), move `workflow.yml` to Packages-* artifact promotion with break-glass and probe modes, add `documentation/developer/guides/releasing.md`, fix the `dev.xml` MSB3030 self-install failure. No version change, no tag, no GitHub Release.
**Effort:** 3 (axes: general)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), headless `ganda task work` review node

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Cross-check against timewarp-ramani and timewarp-amuru (the patterns the task names) and against the TimeWarp.Nuru.DevCli 4.0.0-beta.11 content files
- Prior rounds are immutable; new work goes in `round-(N+1)/`
