# Round 5 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta since round 4, `c8f6bf6..b4d096e`. The only product change is `documentation/developer/guides/releasing.md` (+10 / -8), the guard 8 refusal paragraph. The other changed files are this task's kanban files (`task.md`, `review/round-4/`, `review/review-framework.md`). I checked each claim against the TimeWarp.Nuru.DevCli 4.0.0-beta.11 content files: `endpoints/release-command.cs` (`LocateReleaseTargetAsync`, step 8 output, the step 9 dry-run block) and `services/release-guard.cs` (`SelectReleaseTarget`, `FormatNoSuccessfulRun`).

## Summary

The M5 fix is correct. The paragraph now says the refusal starts with the no-run reason and, when the walk reaches a commit with a path outside `kanban/`, names that commit and the path. That commit is `HEAD` or the commit under a run of kanban-only commits. The usual case names the documentation merge under later `land kanban` commits. The dispatch builds `HEAD`, so the next `Target commit:` is `HEAD`. The old clause "when HEAD is not kanban-only" is gone. No new defects.

## Issues

None.

## Verified, no finding

- M5: `SelectReleaseTarget` returns `Fail($"{noRunReason} Commit {commit.Sha} changes {shippingPath}…")` for the first commit with a path outside `kanban/`. With no such commit inside `ReleaseTargetWalkLimit`, it returns only `noRunReason`. The guide's conditional "When the walk reaches a commit with a path outside `kanban/`" covers both. `LocateReleaseTargetAsync` walks `git rev-list --first-parent` from `HEAD` and diffs each commit against `^1`, so a merge commit's paths are its first-parent diff. The documentation merge is the commit it names.
- "The dispatch builds `HEAD`, so after the run is green the new `Target commit:` is `HEAD`": the next walk asks `gh run list --commit HEAD` first. `OrderCandidateRuns` keeps the `workflow_dispatch` run, so the target is `HEAD` with `SkippedCommitCount` 0. The step 9 dry-run block prints `Target commit:` unconditionally, so the line exists in the `SkippedCommitCount` 0 case too.
- "The refusal starts with 'no successful CI run of workflow.yml'": the printed line is `Error: ` plus the reason. The quoted text is the start of the reason. Not worth a finding.
- No code, workflow, or DevCli wiring changed in this delta. The fix pass's Results record `ganda repo audit` (Passed 38, Failed 0), `./bin/dev build` (0 warnings), `./bin/dev test` (6 of 6), and `./bin/dev check-version` rerun green.
- `## Goal (implementer restatement)` gained one paragraph that restates the corrected guard 8 refusal behavior. It matches the guide and the tool, and it stays within the original task's scope (the releasing guide). No misread.
