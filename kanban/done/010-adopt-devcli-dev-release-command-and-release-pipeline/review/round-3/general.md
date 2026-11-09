# Round 3 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta since round 2, `0070dea..083d7c4`. The only product change is `documentation/developer/guides/releasing.md` (+36 / -14). The other changed files are this task's kanban files. I checked each claim against `.github/workflows/workflow.yml` and the TimeWarp.Nuru.DevCli 4.0.0-beta.11 content files (`endpoints/release-command.cs`, `services/ci-run-promotion.cs`). I also checked `origin/master` history for kanban-only commits.

## Summary

The M3 fix is correct when `master` HEAD is the target commit. Guard 8 prints `OrderCandidateRuns(...)[0]` (`release-command.cs:573-576`), and `OrderCandidateRuns` drops `pull_request` and sorts `push` first (`ci-run-promotion.cs:155-156`). The dry-run prints `Target commit: {short} ({full})` (`release-command.cs:463`). The new check lists every successful run at that SHA and accepts any live `Packages-*`, which is what release mode does. One new defect is in the refresh loop. When `master` HEAD is kanban-only commits past the target, the guide sends the operator back to the old SHA. That is the usual state of `master` in this repo.

## Issues

### Issue 1 — Severity: bug
- File: documentation/developer/guides/releasing.md:107
- Description: After the refresh, the guide says "dry-run again, and repeat the list at the same SHA." `gh workflow run workflow.yml --ref master -f mode=merge` builds `master` HEAD, not the dry-run target. The two differ whenever HEAD is kanban-only commits past the target. Guard 8 then names the target as "HEAD is N kanban-only commit(s) past X". This repo's `master` is in that state now: the six tip commits on `origin/master` (`1e96401` … `9ec3187`, "land kanban 010") change only `kanban/`. For 1.1.0, every task landed after the version-bump merge adds more of these commits. In that state the dispatch run and its live `Packages-*` sit at HEAD. The old target SHA still has only the expired push run. An operator who repeats the list "at the same SHA" sees every artifact expired and dispatches again, in a loop. The release itself would work: the new dry-run walks from HEAD, finds the green dispatch run there, and prints HEAD as `Target commit:`. `dev release` then tags HEAD, and release mode finds the live artifact at that SHA. The guide's check is wrong, not the tooling, which is the same class of defect as M3. Two nearby sentences have the same assumption. Line 80 says that after a refresh the guard 8 line "still names the old push run", but when HEAD moved it names the new dispatch run at HEAD. Line 147 says "a later merge-mode dispatch at the same commit supplies the bytes."
- Suggestion: After the refresh run is green, dry-run again and copy the SHA from that new `Target commit:` line. Say that it moves to HEAD when HEAD was kanban-only past the old target, then run the list at that SHA. Limit the "still names the old push run" sentence to the case where HEAD is the target. In Download, word the dispatch as "a later merge-mode dispatch on `master`" or state the same-commit condition.
- Status: open

## Verified, no finding

- M3: the pre-cut check no longer trusts the guard 8 run id. It takes the full SHA from `Target commit:`, which the dry-run prints as `ShortSha (fullSha)`. It runs `gh run list --workflow workflow.yml --commit SHA --status success`, which is the same query `LocateReleaseTargetAsync` uses, skips `pull_request`, and accepts any `Packages-*` row with `expired` false. The final gate (line 121) matches it. The 7-day age is no longer a trigger.
- Pipeline step 3 now says "ordered first" for push, which matches the `OrderBy` in `OrderCandidateRuns`.
- No code, workflow, or DevCli wiring changed in this delta, so the build and test evidence from earlier rounds still holds. The implementer's Results record that `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` reran green after the fix.
- `## Goal (implementer restatement)` matches the original task: DevCli `release`, promotion of `Packages-*` without a rebuild, break-glass and probe modes, the guide, and no version change. No misread.
