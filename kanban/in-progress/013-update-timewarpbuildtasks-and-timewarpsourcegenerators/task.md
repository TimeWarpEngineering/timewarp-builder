# Update TimeWarp.Build.Tasks and TimeWarp.SourceGenerators

## Description

`ganda nuget outdated` on master (2026-11-11) reports two outdated packages, both patch-level. Bring both pins in `Directory.Packages.props` up to date.

## Requirements

- TimeWarp.Build.Tasks: 1.0.0 -> 1.0.2
- TimeWarp.SourceGenerators: 1.0.0-beta.12 -> 1.0.0-beta.14
- Update any other place that must match these pins (the `global-usings` audit check reads the TimeWarp.SourceGenerators pin).
- If a bump breaks the build, fix the call sites; do not keep the old pin.
- No version bump for TimeWarp.Builder unless the shipped package content changes: 2.0.0 is on master and not yet released.

## Checklist

- [ ] Bump both pins in `Directory.Packages.props`
- [ ] `ganda nuget outdated` reports nothing outdated
- [ ] `./bin/dev build` (0 warnings, 0 errors) and `./bin/dev test` green
- [ ] `ganda repo audit` passes

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-11)

## Notes

- The repo targets net11.0 only (task 012).
- Steven asked for Cursor Opus 5.5 to implement and Cursor Carbon to review.
