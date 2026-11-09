# Round 2 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta since round 1, `7059787..0070dea`. The only product change is `documentation/developer/guides/releasing.md` (+80 / -12). The other changed files are kanban files for this task. Every claim was checked against `.github/workflows/workflow.yml`, `tools/dev-cli/endpoints/workflow-command.release-publish.cs`, and the TimeWarp.Nuru.DevCli 4.0.0-beta.11 content files (`endpoints/release-command.cs`, `services/release-guard.cs`, `services/ci-run-promotion.cs`).

## Summary

The fix pass changed only the release guide. The M1 fix is correct. The walk-back is now described as kanban-only, with at most 50 commits (`NonShippingPathPrefix = "kanban/"`, `ReleaseTargetWalkLimit = 50`, and `rev-list --max-count=50`). A docs-only HEAD fails guard 8 with the `FormatNoSuccessfulRun` text, which names the commit and the path outside `kanban/`. The merge-mode dispatch on `master` uploads, because the upload condition is `github.ref == 'refs/heads/master'` with no event check that excludes a merge-mode dispatch. The M2 fix is correct about retention, keep-last-two, and the 30-day rerun window. It also states the break-glass constraint correctly: the merge-mode dispatch must be at the tag's commit. One defect is new: the pre-cut artifact check cannot pass after a refresh when the target commit already has a push run. That is the expected 1.1.0 case.

## Issues

### Issue 1 — Severity: bug
- File: documentation/developer/guides/releasing.md:78
- Description: Step 2 tells the operator to take the run id from the guard 8 line of `dev release --dry-run`. It then says to check that this run has a live `Packages-*` artifact, to refresh with a merge-mode dispatch when the artifact is expired or the run is older than 7 days, and to run `dev release` only when "a live `Packages-*` artifact on the run guard 8 named" exists (line 101). Guard 8 does not name the newest run. `LocateReleaseTargetAsync` reports `CiRunPromotion.OrderCandidateRuns(...)[0]`, and that method sorts `push`-event runs before every other event (`OrderBy(event == "push" ? 0 : 1)`). For 1.1.0, the target commit is the version-bump merge, which has a push run. After the refresh, a `workflow_dispatch` run exists at the same SHA, but guard 8 still names the old push run. That run's artifact is still expired and the run is still older than 7 days, so the guide sends the operator back to refresh again. The gate on line 101 can never pass. The release itself would work: `workflow-command.release-publish.cs` loops over every candidate and skips expired ones (`continue`), so it would download the dispatch run's artifact. Only the guide's check is wrong.
- Suggestion: Check every non-PR successful run at the target commit, not only the run guard 8 names. For example, take the SHA from the dry-run (HEAD, or the short SHA in the kanban-only line), then list `gh run list --workflow workflow.yml --commit SHA --status success --json databaseId,event`. Proceed when any run that is not `pull_request` has a non-expired `Packages-*` artifact. Alternatively, say that guard 8 prefers the push run, and after a refresh check the new dispatch run's id instead. Drop the "older than 7 days" condition for the run guard 8 names, or limit it to the push run before any refresh.
- Status: open

## Verified, no finding

- Overview, lines 22–26: the walk covers only `kanban/`, at most 50 commits. A commit outside `kanban/`, including `documentation/`, ends it. The push `paths` allowlist does not contain `documentation/**`, so a docs-only push has no run. All of this matches the source.
- Guard 8 refusal, lines 92–94: the text starts with "no successful CI run of workflow.yml" (printed after `Error: `). When HEAD is not kanban-only, `SelectReleaseTarget` adds "Commit {sha} changes {path} … outside kanban/". This matches.
- Download, lines 127–130: release mode loops over the candidates, skips expired or non-matching artifacts, and stops when none is live. The `gh run rerun` 30-day limit matches GitHub's re-run window.
- Break-glass, lines 164–185: release mode's locate-run reads runs at `headSha`, the tag checkout. Upload needs `refs/heads/master`, so a dispatch on the tag ref does not upload. The refresh therefore works only while `master` is still the tagged commit. When `master` has moved, the guide's answer is to bump. That is correct.
- No product code, workflow, or DevCli wiring changed in this delta, so the round-1 build and test evidence still holds.
