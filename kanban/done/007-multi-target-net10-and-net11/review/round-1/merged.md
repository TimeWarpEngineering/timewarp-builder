# Round 1 — merged findings
**Date:** 2026-11-07
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: .github/workflows/workflow.yml:53
- Description: CI installs only the .NET 10 SDK (`dotnet-version: '10.0.x'`). The library now also targets net11.0 (`Directory.Build.props:18`). With SDK 10.0.403, `dotnet build` of the library fails with NETSDK1045 for `TargetFramework=net11.0`. This was reproduced on a clean copy of HEAD. The CI build, pack, and release publish of 1.1.0 would fail.
- Suggestion: have setup-dotnet install `10.0.x` and `11.0.x`. Add `dotnet-quality: preview` while .NET 11 is RC. Confirm CI is green on the PR.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. There was a single reviewer.
