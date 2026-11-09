# Round 4 — merged findings
**Date:** 2026-11-09
**Sources:** general
**Scope:** fix delta `083d7c4..c8f6bf6` (`documentation/developer/guides/releasing.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 5 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- **M1, M2, M3:** fixed, verified in earlier rounds.
- **M4 — bug — fixed (verified).** After the refresh, the guide has the operator dry-run again and list at the new `Target commit:` SHA. That SHA moves to `HEAD` when `HEAD` was kanban-only past the old target. This matches `LocateReleaseTargetAsync` and `OrderCandidateRuns`. Line 80 and Download step 4 are limited to the case where `HEAD` was already the target.

## Issues

### M5 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:131
- Description: The guard 8 refusal paragraph says "The refused commit is `HEAD`, so after the run is green the new `Target commit:` is that same SHA." The walk goes past kanban-only commits and refuses at the first commit with a path outside `kanban/`. On this repo's `master`, a PR merge is followed by "land kanban" commits, so for a documentation-only merge the refused commit is the merge, not `HEAD`. The dispatch builds `HEAD`, and the next `Target commit:` is `HEAD`, not the refused SHA. The action is still right, because the next step reads the latest `Target commit:`. The stated expectation is wrong in the usual case. The older clause "when HEAD is not kanban-only, names the commit and the path outside `kanban/`" has the same error.
- Suggestion: Say that the refusal names the first commit with a path outside `kanban/`, which is `HEAD` or the commit under a run of kanban-only commits. The dispatch builds `HEAD`, so the new `Target commit:` is `HEAD`. Reword or drop "when HEAD is not kanban-only".
- Source: general
- Disposition notes: Fixed in `releasing.md`. The refusal names the first commit with a path outside `kanban/`, which is `HEAD` or the commit under a run of kanban-only commits. The "when HEAD is not kanban-only" clause is gone. The usual case names the documentation merge under later `land kanban` commits. The dispatch builds `HEAD`, so after it is green the new `Target commit:` is `HEAD`. The operator dry-runs again and lists at that line.

## Duplicates / conflicts

- None. Single reviewer.
