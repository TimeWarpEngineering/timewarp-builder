# Update all NuGet packages to latest

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

- [ ] Re-run `ganda nuget outdated` to pick up anything newer
- [ ] Bump every outdated package to latest (central package versions)
- [ ] `dotnet build`: fix any new analyzer diagnostics (warnings are errors)
- [ ] Run tests
- [ ] `ganda nuget outdated` reports everything up to date
- [ ] tw-pr gates, then open the PR

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-08)

## Notes

The analyzer major bumps (Roslynator 5, BannedApiAnalyzers 5) may bring new or
renamed rules. Fix the code. Only suppress a rule in `.editorconfig` with a reason.
