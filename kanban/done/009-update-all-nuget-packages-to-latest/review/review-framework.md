# Review framework — task 009

**Date:** 2026-11-09
**Host task:** kanban/in-progress/009-update-all-nuget-packages-to-latest/
**Diff scope:** branch `task/009-update-all-nuget-packages-to-latest` vs `master` (merge base). The only product change is `Directory.Packages.props`. The rest of the diff is kanban files.
**Plan / brief:** Raise all 7 outdated central NuGet pins to latest. Keep build (warnings as errors) and tests green. Fix new analyzer diagnostics instead of suppressing them.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), ganda task work review node, 2026-11-09

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Budget (by-diff)

- Lines changed: 154
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)
