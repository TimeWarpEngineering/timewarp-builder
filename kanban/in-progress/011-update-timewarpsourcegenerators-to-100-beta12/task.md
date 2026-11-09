# Update TimeWarp.SourceGenerators to 1.0.0-beta.12

## Goal (implementer restatement)

Move the central `TimeWarp.SourceGenerators` package pin from 1.0.0-beta.11 to the newest published version (1.0.0-beta.12 unless `ganda nuget outdated` reports something later). Keep TimeWarp.Builder's own version unchanged. Confirm the beta.12 analyzer changes, then build and test with warnings as errors. Fix any new diagnostic in source. Change `.editorconfig` only when a diagnostic does not apply, and record why. Audit, commit, and push on this task branch.

Acceptance:

- `Directory.Packages.props` pins the latest `TimeWarp.SourceGenerators`.
- The solution builds with zero warnings and tests pass.
- `ganda repo audit` passes.
- The kitchen records how to smoke-check the pin.

## Description

Raise the `TimeWarp.SourceGenerators` pin in `Directory.Packages.props` from
1.0.0-beta.11 to 1.0.0-beta.12, the latest reported by `ganda nuget outdated` on 2026-11-09.

The package is referenced for every project in `Directory.Build.props` with
`PrivateAssets="all"`, so it is build-time only and does not reach TimeWarp.Builder consumers.
This repo uses its analyzers (TW0001 file-name rule, TW0007 global-usings rule, configured in
`.editorconfig`), not the `[Implements]` interface-delegation generator. The risk is a new or
stricter diagnostic breaking the build, since warnings are errors.

## Checklist

- [x] Re-run `ganda nuget outdated`. If a newer version exists, take it and update the title
- [x] Bump the pin in `Directory.Packages.props`
- [x] Read the beta.12 release notes or changes (timewarp-source-generators repo) for new or changed diagnostics
- [x] `dotnet build` with 0 warnings. Fix code for new diagnostics; only change `.editorconfig` with a reason
- [x] Tests pass
- [x] tw-pr gates

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-09)
- Implementation: 01a1bf65-5563-7ce1-937d-624f8f138ec5 (2026-11-09)

## Notes

Do not change the TimeWarp.Builder version (1.1.0 is pending release after .NET 11 GA).

## Results

`ganda nuget outdated` on 2026-11-09 still reports one update: TimeWarp.SourceGenerators 1.0.0-beta.11 → 1.0.0-beta.12 on nuget.org. Nothing newer, so the title stays. After the pin change, the same command reports all 15 central packages up to date, including TimeWarp.SourceGenerators 1.0.0-beta.12.

`Directory.Packages.props` now pins `1.0.0-beta.12`. TimeWarp.Builder stays 1.1.0. `source/timewarp-builder/obj/project.assets.json` restores that version for net10.0 and net11.0.

[v1.0.0-beta.12](https://github.com/TimeWarpEngineering/timewarp-source-generators/releases/tag/v1.0.0-beta.12) and `documentation/releases.md` say the package ships analyzers only. It removes the interface-delegation generator, `[Implements]`, and diagnostics TW0004, TW0005, and TW0006. It removes TW0002 (`XmlDocsToMarkdownAnalyzer`) and TW0003 (`MarkdownDocsGenerator`). Those ids stay retired. The live diagnostics are TW0001 and TW0007, the same two this repo already uses. This tree does not use `[Implements]` or the markdown sidecar, so no source file and no `.editorconfig` entry changed.

`./bin/dev build` (Release) succeeded with 0 warnings and 0 errors and wrote net10.0 and net11.0 outputs. `dotnet build timewarp-builder.slnx` succeeded with 0 warnings and 0 errors. `dotnet build tools/dev-cli/dev.cs` exited 0. `./bin/dev test` passed all 6 Jaribu runfiles. `./bin/dev check-version` reports source 1.1.0 against published 1.0.0 and does not ask for a bump. `ganda repo audit` reports Failed: 0. `ganda repo audit --fix` wrote the gitignored `.vscode/settings.json` window title for `vscode-window-icon`. That file stays untracked because `.vscode/*` is ignored.

### How to validate

**Smoke:**

```bash
ganda nuget outdated
./bin/dev build
./bin/dev test
ganda repo audit
```

**Expect:** `ganda nuget outdated` prints `All packages are up to date!` and lists TimeWarp.SourceGenerators 1.0.0-beta.12. `./bin/dev build` succeeds with 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. `ganda repo audit` reports Failed: 0. TimeWarp.Builder's `<Version>` is still 1.1.0.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root.
