# Review framework

## Budget (by-diff)

- Lines changed: 56
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 013

**Date:** 2026-11-11
**Host task:** kanban/in-progress/013-update-timewarpbuildtasks-and-timewarpsourcegenerators/
**Diff scope:** branch `task/013-update-timewarpbuildtasks-and-timewarpsourcegenera` vs merge-base with `master` (`Directory.Packages.props` + task 013 kitchen)
**Plan / brief:** bump central pins `TimeWarp.Build.Tasks` (`1.0.0` -> `1.0.2`) and `TimeWarp.SourceGenerators` (`1.0.0-beta.12` -> `1.0.0-beta.14`) in `Directory.Packages.props`, fix any broken build call sites without holding old pins back, keep `TimeWarp.Builder` at `2.0.0` unless shipped package content changes, and pass `ganda nuget outdated`, build, tests, and `ganda repo audit`
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle reviewer-cursor (cursor, model gemini-4-carbon-high), headless `ganda task work`, session id not reported, max-turns uncapped (bound 80)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

