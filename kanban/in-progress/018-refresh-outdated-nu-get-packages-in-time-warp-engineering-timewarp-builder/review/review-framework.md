# Review framework — task 018

**Date:** 2026-11-26
**Host task:** kanban/in-progress/018-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-builder/
**Diff scope:** branch `task/018-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `master` (`d73024cc978cc9d70d5c72a93659eac302cf1aae`). Product paths: `Directory.Packages.props`, `tools/dev-cli/dev.cs`, `.timewarp/line-cap-baseline.json`.
**Plan / brief:** Bump the two dev-only pins, TimeWarp.Nuru and TimeWarp.Nuru.DevCli, from 4.0.0-beta.16 to 4.0.0-beta.17 in `Directory.Packages.props`. Leave `<Version>` in `source/Directory.Build.props` unchanged. The implementer also turned on dev CLI telemetry and recorded the existing 747-line workspace guide in the line-cap baseline so `ganda repo audit` can pass.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok 4.7, session 01a215bb-8516-7c52-9cb1-d192c408fb62. Implementer: claude session 02e8af14-8087-4509-be54-6a1599a1fb48.

## Budget (by-diff)

- Lines changed: 56
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
