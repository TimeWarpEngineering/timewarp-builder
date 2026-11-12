# Update TimeWarp.Build.Tasks to 2.0.0

## Description

`ganda nuget outdated` on master (2026-11-12) reports one outdated package: TimeWarp.Build.Tasks 1.0.2 -> 2.0.0. This is a major version, so expect breaking changes.

## Requirements

- Bump TimeWarp.Build.Tasks to 2.0.0 in `Directory.Packages.props`.
- Read what changed in 2.0.0 (release notes, package contents) and adapt this repo: MSBuild props/targets, the `assembly-metadata` audit check's expectations, and anything else that consumes the package.
- If the bump breaks the build, fix the call sites; do not keep the old pin.
- The shipped TimeWarp.Builder package content should be unchanged apart from what the new Build.Tasks itself emits. Say in Results if the nupkg content changed. No version bump for TimeWarp.Builder: 2.0.0 is on master and not yet released.

## Checklist

- [ ] Bump the pin in `Directory.Packages.props`
- [ ] Adapt to any breaking changes in Build.Tasks 2.0.0
- [ ] `ganda nuget outdated` reports nothing outdated
- [ ] `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` and `./bin/dev pack` green
- [ ] `dotnet build tools/dev-cli/dev.cs --no-incremental` green (task 013's review caught a failure only this build showed)
- [ ] `ganda repo audit` passes

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-12)

## Notes

- The repo targets net11.0 only (task 012). Task 013 took Build.Tasks from 1.0.0 to 1.0.2.
- Steven asked for Cursor Opus 5.5 to implement and Cursor Carbon to review.
