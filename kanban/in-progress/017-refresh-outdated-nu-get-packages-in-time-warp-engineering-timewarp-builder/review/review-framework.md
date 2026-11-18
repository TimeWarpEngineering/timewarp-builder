# Review framework — task 017

**Date:** 2026-11-19
**Host task:** kanban/in-progress/017-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-builder/
**Diff scope:** branch `task/017-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `master` (`f980c0a...HEAD`). Product paths: `Directory.Packages.props`, `tools/dev-cli/endpoints/format.cs`, `tools/dev-cli/endpoints/test.cs`, `tools/dev-cli/endpoints/workflow-command.cs`, `tools/dev-cli/endpoints/workflow-command.release-publish.cs`.
**Plan / brief:** Bump the six dev-only TimeWarp pins in `Directory.Packages.props` (Amuru and Amuru.Tools 3.0.0-beta.10, Nuru and Nuru.DevCli 4.0.0-beta.16, Jaribu 1.0.0-beta.18, SourceGenerators 1.0.0-beta.17). Leave `<Version>` in `source/Directory.Build.props` unchanged. Fix what the bump breaks, without suppressions.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok 4.7, session 01a1efb9-6cc7-70c2-a606-d2d10016ec86. Implementer session not reported.

## Budget (by-diff)

- Lines changed: 185
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
