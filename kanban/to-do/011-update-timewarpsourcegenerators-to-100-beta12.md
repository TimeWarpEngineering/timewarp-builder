# Update TimeWarp.SourceGenerators to 1.0.0-beta.12

## Description

Raise the `TimeWarp.SourceGenerators` pin in `Directory.Packages.props` from
1.0.0-beta.11 to 1.0.0-beta.12, the latest reported by `ganda nuget outdated` on 2026-11-09.

The package is referenced for every project in `Directory.Build.props` with
`PrivateAssets="all"`, so it is build-time only and does not reach TimeWarp.Builder consumers.
This repo uses its analyzers (TW0001 file-name rule, TW0007 global-usings rule, configured in
`.editorconfig`), not the `[Implements]` interface-delegation generator. The risk is a new or
stricter diagnostic breaking the build, since warnings are errors.

## Checklist

- [ ] Re-run `ganda nuget outdated`. If a newer version exists, take it and update the title
- [ ] Bump the pin in `Directory.Packages.props`
- [ ] Read the beta.12 release notes or changes (timewarp-source-generators repo) for new or changed diagnostics
- [ ] `dotnet build` with 0 warnings. Fix code for new diagnostics; only change `.editorconfig` with a reason
- [ ] Tests pass
- [ ] tw-pr gates

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-09)

## Notes

Do not change the TimeWarp.Builder version (1.1.0 is pending release after .NET 11 GA).
