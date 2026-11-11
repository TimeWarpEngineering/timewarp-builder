# Update TimeWarp.Build.Tasks and TimeWarp.SourceGenerators

## Description

`ganda nuget outdated` on master (2026-11-11) reports two outdated packages, both patch-level. Bring both pins in `Directory.Packages.props` up to date.

## Requirements

- TimeWarp.Build.Tasks: 1.0.0 -> 1.0.2
- TimeWarp.SourceGenerators: 1.0.0-beta.12 -> 1.0.0-beta.14
- Update any other place that must match these pins (the `global-usings` audit check reads the TimeWarp.SourceGenerators pin).
- If a bump breaks the build, fix the call sites; do not keep the old pin.
- No version bump for TimeWarp.Builder unless the shipped package content changes: 2.0.0 is on master and not yet released.

## Goal (implementer restatement)

Move the central pins for TimeWarp.Build.Tasks (to 1.0.2) and TimeWarp.SourceGenerators (to 1.0.0-beta.14) forward in `Directory.Packages.props`, plus any other file that has to agree with them. Done means: `ganda nuget outdated` is clean, `./bin/dev build` has 0 warnings and 0 errors, `./bin/dev test` is green, and `ganda repo audit` passes. Build breaks get fixed at the call sites, never by keeping the old pin. TimeWarp.Builder stays at 2.0.0 unless the shipped package content changes.

## Checklist

- [x] Bump both pins in `Directory.Packages.props`
- [x] `ganda nuget outdated` reports nothing outdated
- [x] `./bin/dev build` (0 warnings, 0 errors) and `./bin/dev test` green
- [x] `ganda repo audit` passes

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-11)

## Notes

- The repo targets net11.0 only (task 012).
- Steven asked for Cursor Opus 5.5 to implement and Cursor Carbon to review.

## Results

- `Directory.Packages.props`: TimeWarp.Build.Tasks 1.0.0 -> 1.0.2, TimeWarp.SourceGenerators 1.0.0-beta.12 -> 1.0.0-beta.14. Nothing else pins either package (`Directory.Build.props` references them without a version), so no other file needed to change.
- Neither bump broke the build, so no call sites changed. The TimeWarp.Builder package content is the same, so it stays at 2.0.0.
- `./bin/dev build`: 0 warnings, 0 errors. `./bin/dev test`: all 6 test files pass.
- `ganda nuget outdated`: all packages are up to date.
- `ganda repo audit`: passes. The one advisory warning (`vscode-window-icon`, `.vscode/settings.json` missing) was fixed locally with `ganda repo audit --fix`. That file is git-ignored, so the fix adds nothing to the diff.

### How to validate

Smoke:

```bash
ganda nuget outdated && ./bin/dev build && ./bin/dev test && ganda repo audit
```

Expect: `ganda nuget outdated` prints "All packages are up to date!" and lists TimeWarp.Build.Tasks 1.0.2 and TimeWarp.SourceGenerators 1.0.0-beta.14. The build reports 0 warnings and 0 errors. The tests print "All 6 test file(s) passed!". The audit reports that the repository passes.
