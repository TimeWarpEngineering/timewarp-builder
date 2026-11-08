# Update all NuGet packages to latest

## Goal (implementer restatement)

Every centrally managed NuGet pin in this repo is at the latest version
`ganda nuget outdated` reports. The solution still builds with warnings as
errors, and the existing tests pass. New analyzer diagnostics from the major
bumps (Roslynator 5, BannedApiAnalyzers 5, and any other rule changes) are
fixed in code. A rule is suppressed in `.editorconfig` only with a written
reason. The walk stops after the pre-PR gates and a push. Opening the pull
request is a later host step.

Acceptance:

- `ganda nuget outdated` is re-run first and again at the end, and the end run
  reports nothing outdated.
- Each outdated `PackageVersion` in `Directory.Packages.props` is raised to
  that latest version.
- `dotnet build` (or the repo `dev build`) succeeds with warnings as errors,
  after any new diagnostics are fixed.
- Tests pass.
- Pre-PR gates (`ganda repo audit`, build, tests) pass and the branch is pushed.
  `gh pr create` is not run from this walk.

## Description

Bring every NuGet package reference up to its latest version, as reported by
`ganda nuget outdated`. Build and tests must stay green, with warnings as errors.

Outdated as of 2026-11-08 (7 packages: 4 major, 1 minor, 2 patch):

| Package | Current | Latest |
|---------|---------|--------|
| TimeWarp.Jaribu | 1.0.0-beta.13 | 1.0.0-beta.15 |
| Roslynator.Analyzers | 4.15.0 | 5.0.1 (major) |
| Roslynator.CodeAnalysis.Analyzers | 4.15.0 | 5.0.1 (major) |
| Roslynator.Formatting.Analyzers | 4.15.0 | 5.0.1 (major) |
| Microsoft.CodeAnalysis.NetAnalyzers | 10.0.301 | 10.0.403 |
| Microsoft.CodeAnalysis.CSharp.CodeStyle | 5.3.0 | 5.9.0 |
| Microsoft.CodeAnalysis.BannedApiAnalyzers | 4.14.0 | 5.6.0 (major) |

## Checklist

- [x] Re-run `ganda nuget outdated` to pick up anything newer
- [x] Bump every outdated package to latest (central package versions)
- [x] `dotnet build`: fix any new analyzer diagnostics (warnings are errors)
- [x] Run tests
- [x] `ganda nuget outdated` reports everything up to date
- [x] tw-pr gates passed (audit, check-version, build, test, push). The host open-pr node opens the pull request.

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-08)
- Implementation: 01a1bc21-a39b-7c23-8ec7-2c0fe3197441 (2026-11-09)
- `ganda repo audit --fix --checks vscode-window-icon` wrote the gitignored `.vscode/settings.json` window title so the advisory window-icon check passes on this machine. That file stays untracked. No analyzer rule was suppressed.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bc21-a39b-7c23-8ec7-2c0fe3197441, max-turns 500, Done — 2026-11-08T17:17:51Z

## Notes

The analyzer major bumps (Roslynator 5, BannedApiAnalyzers 5) may bring new or
renamed rules. Fix the code. Only suppress a rule in `.editorconfig` with a reason.

No new diagnostics fired. The library, the dev CLI, and the six Jaribu runfiles
built with warnings as errors and needed no source edits.

## Results

`ganda nuget outdated` on 2026-11-09 reported the same seven packages as the
description, and nothing newer. All seven central pins were raised. A second
`ganda nuget outdated` reports all 13 packages up to date.

| Package | From | To |
|---------|------|----|
| TimeWarp.Jaribu | 1.0.0-beta.13 | 1.0.0-beta.15 |
| Roslynator.Analyzers | 4.15.0 | 5.0.1 |
| Roslynator.CodeAnalysis.Analyzers | 4.15.0 | 5.0.1 |
| Roslynator.Formatting.Analyzers | 4.15.0 | 5.0.1 |
| Microsoft.CodeAnalysis.NetAnalyzers | 10.0.301 | 10.0.403 |
| Microsoft.CodeAnalysis.CSharp.CodeStyle | 5.3.0 | 5.9.0 |
| Microsoft.CodeAnalysis.BannedApiAnalyzers | 4.14.0 | 5.6.0 |

`ganda nuget outdated --update` wrote a UTF-8 BOM onto `Directory.Packages.props`.
That BOM was removed so the file stays UTF-8 without a BOM.

`dotnet build timewarp-builder.slnx` and `dotnet build tools/dev-cli/dev.cs`
succeeded with 0 warnings and 0 errors. `./bin/dev build` (Release) succeeded
the same way for net10.0 and net11.0. `./bin/dev test` passed all 6 Jaribu
runfiles. `./bin/dev check-version` reports TimeWarp.Builder 1.1.0 is not yet
published. `ganda repo audit` reports Failed: 0 after the local window-icon fix.

### How to validate

**Smoke:**

```bash
ganda nuget outdated
./bin/dev build
./bin/dev test
ganda repo audit
```

**Expect:** `ganda nuget outdated` prints `All packages are up to date!` for the 13 central pins, including TimeWarp.Jaribu 1.0.0-beta.15, Roslynator.Analyzers 5.0.1, Roslynator.CodeAnalysis.Analyzers 5.0.1, Roslynator.Formatting.Analyzers 5.0.1, Microsoft.CodeAnalysis.NetAnalyzers 10.0.403, Microsoft.CodeAnalysis.CSharp.CodeStyle 5.9.0, and Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0. `./bin/dev build` succeeds with 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. `ganda repo audit` reports Failed: 0.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root.
