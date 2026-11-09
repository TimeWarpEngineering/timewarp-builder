# Review framework

## Budget (by-diff)

- Lines changed: 98
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 011

**Date:** 2026-11-09
**Host task:** kanban/in-progress/011-update-timewarpsourcegenerators-to-100-beta12/
**Diff scope:** branch `task/011-update-timewarpsourcegenerators-to-100-beta12` vs merge-base with `master` (product change: `Directory.Packages.props`, 1 line; rest is kanban kitchen)
**Plan / brief:** bump the central `TimeWarp.SourceGenerators` pin 1.0.0-beta.11 → 1.0.0-beta.12; keep TimeWarp.Builder 1.1.0; build with 0 warnings, tests pass, audit clean
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), ganda task work headless

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
