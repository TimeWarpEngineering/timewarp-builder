# Add BannedApiAnalyzers to timewarp-builder

## Description

Add BannedApiAnalyzers to enforce banned API patterns. Reference: timewarp-ganda project configuration.

## Checklist

- [x] Add `Microsoft.CodeAnalysis.BannedApiAnalyzers` package reference to `Directory.Build.props`
- [x] Add `AdditionalFiles` ItemGroup for `BannedSymbols.txt` in `Directory.Build.props`
- [x] Create `BannedSymbols.txt` file at repository root with banned API definitions
- [x] Build project to verify analyzer works correctly

## Notes

Reference from timewarp-ganda:
- Package: `Microsoft.CodeAnalysis.BannedApiAnalyzers`
- BannedSymbols.txt location: `$(MSBuildThisFileDirectory)BannedSymbols.txt`
- Initially ban `System.Console` - use `TimeWarp.Terminal.Terminal` or inject `ITerminal` instead

## Results

- Added `Microsoft.CodeAnalysis.BannedApiAnalyzers` v3.3.4 to `Directory.Packages.props`
- Added `PackageReference` with `PrivateAssets="all"` to `Directory.Build.props` Code Analyzers ItemGroup
- Added `ItemGroup Label="Banned API Files"` with `AdditionalFiles` pointing to `BannedSymbols.txt` in `Directory.Build.props`
- Created `BannedSymbols.txt` at repo root banning `T:System.Console` with message directing to `TimeWarp.Terminal.Terminal` or `ITerminal`
- Build succeeded with 0 warnings, 0 errors
