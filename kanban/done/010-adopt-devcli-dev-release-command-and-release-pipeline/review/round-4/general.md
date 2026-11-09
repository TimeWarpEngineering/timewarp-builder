# Round 4 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta since round 3, `083d7c4..c8f6bf6`. The only product change is `documentation/developer/guides/releasing.md` (+37 / -19). The other changed files are this task's kanban files. I checked each claim against the TimeWarp.Nuru.DevCli 4.0.0-beta.11 content files (`endpoints/release-command.cs` `LocateReleaseTargetAsync` and the step 8 output, `services/release-guard.cs` `SelectReleaseTarget` and `FormatNoSuccessfulRun`, `services/ci-run-promotion.cs` `OrderCandidateRuns`). I also checked the first-parent shape of `origin/master`.

## Summary

The M4 fix is correct. After a merge-mode refresh, the guide now has the operator dry-run again and list at the new `Target commit:` SHA. That SHA moves to `HEAD` when `HEAD` was kanban-only past the old target. The tool agrees: `LocateReleaseTargetAsync` asks `gh run list` at `HEAD` first, `OrderCandidateRuns` keeps the `workflow_dispatch` run, and the walk stops at `HEAD`. The step 8 line is then `✓ successful CI run found (<dispatch id>)`. Line 80 and Download step 4 are now limited to the case where `HEAD` was already the target. One new sentence in the guard 8 refusal paragraph makes the same same-SHA assumption that M4 removed elsewhere.

## Issues

### Issue 1 — Severity: bug
- File: documentation/developer/guides/releasing.md:131
- Description: The new sentence reads: "The refused commit is `HEAD`, so after the run is green the new `Target commit:` is that same SHA." The refusal does not always name `HEAD`. `LocateReleaseTargetAsync` walks first-parent history from `HEAD`. It goes past kanban-only commits that have no run and stops at the first commit with a path outside `kanban/`. `SelectReleaseTarget` then fails with `Commit {commit.Sha} changes {path} ...`, and that commit can sit below kanban-only commits. This repo's `master` is in that shape after every merge. `origin/master` first-parent is `5bfbc83 Merge pull request #9`, then seven "land kanban" commits that change only `kanban/`. A documentation-only PR merge, which the paragraph calls "the usual case", is followed the same way. The refusal then names the merge commit D. The dispatch builds `HEAD`, and the next dry-run's `Target commit:` is `HEAD`, not D. The procedure still works, because the paragraph ends with "dry-run again and apply the artifact check above", and that check reads the latest `Target commit:`. But the guide says to expect D, and the operator sees `HEAD`, which looks like a fault in the usual case. This is the same class as M4. The older sentence just before it, "when HEAD is not kanban-only, names the commit and the path outside `kanban/`", is also wrong for this case. The walk names the shipping commit whether or not `HEAD` itself is kanban-only. That sentence predates this delta.
- Suggestion: Say that the refusal names the first commit with a path outside `kanban/`, which may be `HEAD` or the commit under a run of kanban-only commits. The dispatch builds `HEAD`, so after it is green the new `Target commit:` is `HEAD`. Drop "when HEAD is not kanban-only" from the earlier sentence, or reword it to match the walk.
- Status: open

## Verified, no finding

- M4: lines 107-123 say to dry-run again after the refresh is green and list at the new `Target commit:`, not the pre-refresh SHA. They say the SHA moves to `HEAD` when `HEAD` was kanban-only past the old target. This matches `LocateReleaseTargetAsync`, which asks for runs at `HEAD` first and breaks on the first candidate, and `OrderCandidateRuns`, which drops only `pull_request`.
- Lines 80-88: when `HEAD` was already the target, guard 8 still prints the push run, because `OrderCandidateRuns` sorts `push` first. When `HEAD` moved, the target is `HEAD` with `SkippedCommitCount` 0, and the line is `✓ successful CI run found (<dispatch id>)`. That is the new dispatch, as the guide says.
- Download step 4 limits "a later merge-mode dispatch on `master` supplies the bytes" to a dispatch at the commit being released. The release-mode loop downloads at the tag commit.
- Final gate (line 136) names "the latest dry-run `Target commit:`". That is consistent.
- No code, workflow, or DevCli wiring changed in this delta. The build and test evidence from earlier rounds still holds. The fix pass's Results record `ganda repo audit`, `./bin/dev build`, `./bin/dev test`, and `./bin/dev check-version` reran green.
- `## Goal (implementer restatement)` is unchanged in this delta and still matches the original task. No misread.
