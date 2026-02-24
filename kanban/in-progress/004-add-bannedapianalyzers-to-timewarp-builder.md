# Add BannedApiAnalyzers to timewarp-builder

## Description

Add BannedApiAnalyzers to enforce banned API patterns. Reference: timewarp-ganda project configuration.

## Checklist

- [ ] Add `Microsoft.CodeAnalysis.BannedApiAnalyzers` package reference to `Directory.Build.props`
- [ ] Add `AdditionalFiles` ItemGroup for `BannedSymbols.txt` in `Directory.Build.props`
- [ ] Create `BannedSymbols.txt` file at repository root with banned API definitions
- [ ] Build project to verify analyzer works correctly

## Notes

Reference from timewarp-ganda:
- Package: `Microsoft.CodeAnalysis.BannedApiAnalyzers`
- BannedSymbols.txt location: `$(MSBuildThisFileDirectory)BannedSymbols.txt`
- Initially ban `System.Console` - use `TimeWarp.Terminal.Terminal` or inject `ITerminal` instead