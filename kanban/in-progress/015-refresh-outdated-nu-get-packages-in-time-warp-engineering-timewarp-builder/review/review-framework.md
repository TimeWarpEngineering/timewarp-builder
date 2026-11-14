# Review framework — task 015

**Date:** 2026-11-14
**Host task:** kanban/in-progress/015-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-builder/
**Diff scope:** branch `task/015-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (`master...HEAD`, 6 commits). Product paths: `Directory.Packages.props`, `source/Directory.Build.props`, and Purpose regions on five `.githooks/*.cs` files from `ganda repo audit --fix`.
**Plan / brief:** Refresh the five outdated central pins (Nuru and Nuru.DevCli 4.0.0-beta.13, Jaribu 1.0.0-beta.16, SourceGenerators 1.0.0-beta.16, CSharp.CodeStyle 5.12.0) and set `<Version>` in `source/Directory.Build.props` to `2.1.0-beta.1`. The implementer also moved TimeWarp.Build.Tasks 2.0.0 to 2.0.1-beta.1 after package-pin-currency failed.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok 4.7, session 01a1d911-fb4e-7e73-ab9c-59a6c645f61b. Implementer session 41f7a72f-133e-4753-9f48-410c44d2e2fa.

## Budget (by-diff)

- Lines changed: 95
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
