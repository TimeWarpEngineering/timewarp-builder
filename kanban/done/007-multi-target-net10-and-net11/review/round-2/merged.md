# Round 2 — merged findings
**Date:** 2026-11-07
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: .github/workflows/workflow.yml:50
- Description: CI installed only the .NET 10 SDK, so the net11.0 target failed with NETSDK1045 (round 1).
- Suggestion: install `10.0.x` and `11.0.x`, with preview quality for 11.
- Source: general (round 1)
- Disposition notes: fixed in `edc9d63`. There are separate Setup .NET 10 (`10.0.x`, no quality) and Setup .NET 11 (`11.0.x`, `dotnet-quality: preview`) steps. A dry-run of setup-dotnet v4.3.1 `install-dotnet.sh` resolves them to 10.0.403 and 11.0.100-rc.3.26522.148. A local Release build of the library succeeded for net10.0 and net11.0.

No new findings on the fix delta.

## Duplicates / conflicts

- None. There was a single reviewer.
