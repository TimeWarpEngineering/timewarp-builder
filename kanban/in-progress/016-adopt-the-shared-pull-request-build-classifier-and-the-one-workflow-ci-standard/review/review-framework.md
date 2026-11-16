# Review framework — task 016

**Date:** 2026-11-16
**Host task:** kanban/in-progress/016-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/016-adopt-the-shared-pull-request-build-classifier-and` vs `master` (4 commits, 215 lines). Product delta: `.githooks/prepare-commit-msg`, `.githooks/prepare-commit-msg.cs`, `.timewarp/dev.jsonc`, `Directory.Packages.props`, `tools/dev-cli/endpoints/workflow-command.cs`, plus this task's `task.md`.
**Plan / brief:** Task 016 requirements and `## Goal (implementer restatement)`. Pin `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at 4.0.0-beta.15, call `PullRequestBuildClassifier.DecideAsync` on the pull-request path, keep `workflow.yml` with no `pull_request` path filter, and declare packed paths the defaults would skip through `pullRequestBuild` in `.timewarp/dev.jsonc`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** grok 01a1e4ed-6464-7903-9cc8-f5e94913188a (review); implementer claude e21139dc-2e8c-4193-93a0-ba56c0ad56b0

## Budget (by-diff)

- Lines changed: 215
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
