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
- [x] Review round 1 M1: `dotnet build tools/dev-cli/dev.cs` is clean with beta.14

## Session

- Created: d24e7b7a-5ad1-45c4-bb65-067299a19cc5 (2026-11-11)
- Implement oracle: implemented by implementer-cursor (cursor, vendor anthropic, model claude-opus-5-5-medium), session not reported, max-turns uncapped (no --max-turns flag; bound 500), Done — 2026-11-11T10:34:54Z
- Review oracle round 1: reviewer-cursor (cursor, model gemini-4-carbon-high), headless, session not reported, effort 1, roster general, max-turns uncapped (no --max-turns flag; bound 80) — 2026-11-11. 1 open bug (`M1`, `review/round-1/merged.md`); fix pass requested.
- Review oracle: review by reviewer-cursor (cursor, vendor google, model gemini-4-carbon-high), session not reported, max-turns uncapped (no --max-turns flag; bound 80), artifacts general.md, merged.md — 2026-11-11T10:50:27Z

## Review

- Round 1: Blocked — open M1

## Notes

- The repo targets net11.0 only (task 012).
- Steven asked for Cursor Opus 5.5 to implement and Cursor Carbon to review.

## Results

- `Directory.Packages.props`: TimeWarp.Build.Tasks 1.0.0 -> 1.0.2, TimeWarp.SourceGenerators 1.0.0-beta.12 -> 1.0.0-beta.14. Nothing else pins either package (`Directory.Build.props` references them without a version), so no other file needed to change.
- Neither bump broke the build, so no call sites changed. The TimeWarp.Builder package content is the same, so it stays at 2.0.0.
- `./bin/dev build`: 0 warnings, 0 errors. `./bin/dev test`: all 6 test files pass.
- `ganda nuget outdated`: all packages are up to date.
- `ganda repo audit`: passes. The one advisory warning (`vscode-window-icon`, `.vscode/settings.json` missing) was fixed locally with `ganda repo audit --fix`. That file is git-ignored, so the fix adds nothing to the diff.
- Review round 1 fix (M1): the beta.14 bump turned on `TWA0038` (500-line cap), and two `TimeWarp.Nuru.DevCli` 4.0.0-beta.12 content files that `tools/dev-cli/dev.cs` compiles from the NuGet cache broke on it. `release-command.cs` has 609 lines and `nuget-version-service.cs` has 704. `./bin/dev build` missed this because it builds only the library, but CI's `dotnet run --file tools/dev-cli/dev.cs -- workflow` would fail. Fixed by adding the `ExemptPackageContentFromAnalyzers` target to `tools/dev-cli/Directory.Build.props`, the same target `timewarp-terminal` uses. It turns analyzers off only for `@(Compile)` items under `$(NuGetPackageRoot)`, so `TWA0038` stays on for this repo's own sources. With the package content exempted, the DevCli-only `<NoWarn>` ids are redundant, so I dropped `RCS1194`, `CA5399`, and the `IDE*` ids except `IDE0211`. `ganda repo audit --fix` then lowered `.timewarp/suppression-baseline.json`.

### How to validate

Smoke:

```bash
ganda nuget outdated && dotnet build tools/dev-cli/dev.cs --no-incremental && ./bin/dev build && ./bin/dev test && ganda repo audit
```

Expect: `ganda nuget outdated` prints "All packages are up to date!" and lists TimeWarp.Build.Tasks 1.0.2 and TimeWarp.SourceGenerators 1.0.0-beta.14. The `tools/dev-cli/dev.cs` build has no `TWA0038` errors and no warnings. `./bin/dev build` reports 0 warnings and 0 errors. The tests print "All 6 test file(s) passed!". The audit reports that the repository passes.

### Review

- Round 1 (effort 1, roster `general`): 1 bug (`M1` in `review/round-1/merged.md` — `tools/dev-cli/dev.cs` fails `TWA0038` on two `TimeWarp.Nuru.DevCli` NuGet content files > 500 lines). Fixed in the fix pass. See Results.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.
