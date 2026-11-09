# Round 2 — merged findings
**Date:** 2026-11-09
**Sources:** general
**Scope:** fix delta `7059787..0070dea` (`documentation/developer/guides/releasing.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- **M1 — bug — fixed (verified).** The guide now limits the walk-back to `kanban/`, at most 50 commits, which matches `ReleaseGuard.NonShippingPathPrefix` and `ReleaseTargetWalkLimit`. A docs-only HEAD is recovered with `gh workflow run workflow.yml --ref master -f mode=merge`. That dispatch uploads `Packages-*` because the upload condition keys on `refs/heads/master`.
- **M2 — suggestion — fixed (verified).** The guide now covers the 7-day retention and keep-last-two pruning, states the 30-day `gh run rerun` limit, and gives the dispatch-on-master-then-break-glass recovery while `master` is still the tagged commit. The pre-cut check this fix added has the defect recorded as M3.

## Issues

### M3 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:78
- Description: The pre-cut artifact check reads the run id from guard 8. Guard 8 always prefers a `push`-event run at the target commit (`CiRunPromotion.OrderCandidateRuns`, push first). After the refresh dispatch, it still names the old push run, and that run's artifact is expired. The run is also more than 7 days old. Following the guide therefore loops on refresh, and the "live artifact on the run guard 8 named" gate at line 101 can never pass. The 1.1.0 cut is exactly this case: the version-bump merge's push run will be stale by .NET 11 GA. Release mode itself would succeed, because it loops over every candidate and skips expired artifacts.
- Suggestion: Check every successful run at the target commit that is not a pull-request run (`gh run list --workflow workflow.yml --commit SHA --status success`) and proceed when any has a live `Packages-*` artifact. Alternatively, say that after a refresh the run to check is the new dispatch run, not the run guard 8 prints.
- Source: general
- Disposition notes: Fixed in `releasing.md`. The pre-cut check takes the full SHA from the dry-run `Target commit:` line, lists every successful `workflow.yml` run at that commit, skips `pull_request`, and proceeds when any remaining run has a `Packages-*` artifact with `expired` false. Run age is not the gate. After a merge-mode refresh the operator repeats that list. Download says an expired push run is skipped, so the later dispatch supplies the artifact.

## Duplicates / conflicts

- None. Single reviewer.
