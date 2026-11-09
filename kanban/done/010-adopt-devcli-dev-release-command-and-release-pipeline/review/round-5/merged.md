# Round 5 — merged findings
**Date:** 2026-11-09
**Sources:** general
**Scope:** fix delta `c8f6bf6..b4d096e` (`documentation/developer/guides/releasing.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 5 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- **M1, M2, M3, M4:** fixed, verified in earlier rounds.
- **M5 — bug — fixed (verified).** The guard 8 refusal paragraph names the first commit with a path outside `kanban/`, which is `HEAD` or the commit under a run of kanban-only commits. The dispatch builds `HEAD`, so the new `Target commit:` is `HEAD`. This matches `SelectReleaseTarget`, `LocateReleaseTargetAsync`, and the unconditional `Target commit:` line in the dry-run block.

## Issues

None new.

## Duplicates / conflicts

- None. Single reviewer.
