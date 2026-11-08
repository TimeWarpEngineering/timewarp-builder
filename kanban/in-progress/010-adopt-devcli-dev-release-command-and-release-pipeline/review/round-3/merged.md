# Round 3 — merged findings
**Date:** 2026-11-09
**Sources:** general
**Scope:** fix delta `0070dea..083d7c4` (`documentation/developer/guides/releasing.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 4 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- **M1 — bug — fixed** (verified in round 2).
- **M2 — suggestion — fixed** (verified in round 2).
- **M3 — bug — fixed (verified).** The pre-cut check takes the full SHA from the dry-run `Target commit:` line and lists every successful `workflow.yml` run at that SHA. It skips `pull_request` and accepts any live `Packages-*`, which matches `OrderCandidateRuns` and the release-mode download loop. The run id that guard 8 prints is no longer the gate. The new M4 is about the refresh loop this fix added, when HEAD is not the target.

## Issues

### M4 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:107
- Description: After the refresh, the guide says to "repeat the list at the same SHA". The merge-mode dispatch on `master` builds HEAD. When HEAD is kanban-only commits past the target, the live artifact lands at HEAD and the next dry-run moves `Target commit:` to HEAD. Listing the old SHA shows every artifact still expired, so the operator dispatches again in a loop. `origin/master` has six kanban-only commits at its tip right now, so this is the expected 1.1.0 case. Lines 80 ("still names the old push run") and 147 ("dispatch at the same commit") make the same same-SHA assumption.
- Suggestion: After the refresh is green, re-read `Target commit:` from the new dry-run and list at that SHA. Say that it moves to HEAD when HEAD was kanban-only past the old target. Limit lines 80 and 147 to the HEAD-is-target case, or reword them.
- Source: general
- Disposition notes: Fixed in `releasing.md`. After the merge-mode refresh is green, the operator dry-runs again and lists at the new `Target commit:` SHA. That SHA moves to HEAD when HEAD was kanban-only past the old target. The old-push-run sentence is limited to the case where HEAD was already the target. Download says a later merge-mode dispatch on `master` supplies the bytes when that dispatch ran at the commit being released.

## Duplicates / conflicts

- None. Single reviewer.
