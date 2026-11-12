# Review framework

## Budget (by-diff)

- Lines changed: 81
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Framework — task 014

**Date:** 2026-11-12
**Host task:** kanban/in-progress/014-update-timewarpbuildtasks-to-200/
**Diff scope:** branch `task/014-update-timewarpbuildtasks-to-200` vs merge-base with `master` (`Directory.Packages.props`, surrounding MSBuild/CLI/test/hook call sites, and task 014 kitchen)
**Plan / brief:** bump central pin `TimeWarp.Build.Tasks` (`1.0.2` -> `2.0.0`) in `Directory.Packages.props`, adapt to any `2.0.0` breaking changes across MSBuild props/targets, `assembly-metadata` audit check expectations, and dev-cli/test/hook consumers without falling back to the old pin, keep `TimeWarp.Builder` at `2.0.0`, state in `Results` whether packed `nupkg` content changed, and pass `ganda nuget outdated`, builds, tests, pack, and `ganda repo audit`
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle reviewer-cursor (cursor, model gemini-4-carbon-high), headless `ganda task work` (round 1), session id not reported, max-turns uncapped (no --max-turns flag; bound 80)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

