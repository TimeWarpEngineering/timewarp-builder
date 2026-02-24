# Address code review blockers for v1.0.0 release

## Description

Resolve all blockers and high-priority items identified in the comprehensive code review to prepare TimeWarp.Builder for an official v1.0.0 release.

Code review report: `.agent/workspace/2026-02-23T00-00-00_code-review-release-readiness.md`

## Checklist

### 🔴 Blockers (must fix before v1.0.0)

- [x] **Fix `msbuild/repository.props`** (commit: c5ab2b6)
  - Change `RepositoryName` from `timewarp-nuru` to `timewarp-builder`
  - Change `SolutionFile` from `timewarp-nuru.slnx` to `timewarp-builder.slnx`
  - Add `ToolsDirectory` for future tooling support
  - Note: TestsDirectory, SamplesDirectory, BenchmarksDirectory left in place for future use

- [ ] **Add automated tests**
  - Create test project (suggested: `tests/TimeWarp.Builder.Tests/`)
  - Test all 4 scope extension methods:
    - `Also` - executes action, returns original object
    - `Apply` - executes action, returns original object
    - `Let` - transforms to new type/value
    - `Run` - executes action (terminal, void)
  - Test null guard behavior - all methods throw `ArgumentNullException` when callback is null
  - Test interface implementations work correctly (integration-style with concrete builders)

- [ ] **Add required NuGet metadata to `timewarp-builder.csproj`**
  - `<Authors>TimeWarp Engineering</Authors>` (or appropriate value)
  - `<PackageProjectUrl>https://github.com/TimeWarpEngineering/timewarp-builder</PackageProjectUrl>`
  - `<RepositoryUrl>https://github.com/TimeWarpEngineering/timewarp-builder.git</RepositoryUrl>`
  - `<PackageLicenseExpression>MIT</PackageLicenseExpression>` (verify LICENSE file matches)

- [ ] **Bump version for release**
  - Change `<Version>1.0.0-beta.1</Version>` to `<Version>1.0.0</Version>` in `timewarp-builder.csproj`

### ⚠️ High Priority (strongly recommended)

- [ ] **Improve README.md**
  - Add installation section with `dotnet add package TimeWarp.Builder`
  - Add requirements section (target framework: .NET 10.0)
  - Add license badge and link to LICENSE file
  - Add NuGet version badge
  - Add design rationale explaining `Also` vs `Apply` distinction
  - Link to GitHub repository

- [ ] **Wire up package icon**
  - Add `<PackageIcon>timewarp-builder-avatar.png</PackageIcon>` (convert SVG to PNG if needed, NuGet prefers PNG)
  - Add icon file reference to `.csproj`:
    ```xml
    <ItemGroup>
      <None Include="../../assets/timewarp-builder-avatar.png" Pack="true" PackagePath="" />
    </ItemGroup>
    ```
  - Note: `assets/timewarp-builder-avatar.svg` exists but SVG icons in NuGet packages have limited client support

- [ ] **Prune `Directory.Packages.props`**
  - Remove unused package groups:
    - Serilog (Logging - Serilog section)
    - OpenTelemetry
    - Aspire
    - Benchmark - CLI Frameworks
    - Benchmark - Performance
    - MCP Server
    - Mediator
  - Keep only packages actually referenced by this library (likely just analyzers and Microsoft.Extensions if needed)
  - Note: `TimeWarp.Builder` self-reference can also be removed

- [ ] **Review AOT warning suppressions**
  - Current global suppressions in `Directory.Build.props`: `IL2026;IL2067;IL2070;IL2075;IL3050;IL2104;IL3053`
  - Evaluate if these are truly needed for this library (it has no reflection/dynamic code)
  - If they are false positives, consider removing the global suppression and testing AOT build

### Nice to Have (low priority)

- [ ] Add `#region Purpose` / `#region Design` context blocks to source files per csharp skill conventions
- [ ] Add `CHANGELOG.md` for v1.0.0 release notes
- [ ] Consider `IBuildAsync<T>` interface for async build scenarios (future v1.x)

## Notes

### Code Review Summary

**Reviewer:** claude-sonnet-4-6 (automated analysis)  
**Date:** 2026-02-23  
**Branch:** Cramer-2025-12-22-dev  
**Version:** 1.0.0-beta.1

**Overall Assessment:** The C# source code is excellent - clean, well-documented, AOT-compatible, with aggressive analyzer enforcement. All 4 files are correct and fully documented. The blockers are all in tooling, packaging, and testing infrastructure - not in the core logic.

**Positive Highlights:**
- Aggressive analyzer stack (Roslynator + NetAnalyzers + TreatWarningsAsErrors)
- Full XML documentation with code examples
- Correct use of covariance on interfaces
- Modern null guards via `ArgumentNullException.ThrowIfNull`
- AOT compatibility flags correctly set
- Central Package Management enabled
- Clean file naming conventions

**Key Issues Found:**
1. `msbuild/repository.props` has stale references to `timewarp-nuru` (copied from another repo)
2. Zero automated tests - no test project exists
3. Missing required NuGet metadata (Authors, URLs, License)
4. README lacks installation instructions and badges
5. Package icon exists but isn't referenced in `.csproj`
6. `Directory.Packages.props` contains many irrelevant packages from nuru monorepo

With the blockers resolved, this library is ready for a clean v1.0.0 release.

### Files Involved
- `msbuild/repository.props` - fix stale references
- `source/timewarp-builder/timewarp-builder.csproj` - add metadata, bump version
- `Directory.Packages.props` - remove unused packages
- `Directory.Build.props` - review AOT suppressions
- `README.md` - add installation, badges, requirements
- New: `tests/TimeWarp.Builder.Tests/` - create test project
- `assets/timewarp-builder-avatar.svg` - convert to PNG and wire up
