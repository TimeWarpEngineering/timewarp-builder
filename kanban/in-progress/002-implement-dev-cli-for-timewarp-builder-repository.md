# Implement dev CLI for timewarp-builder repository

## Description

Create a TimeWarp.Nuru-based dev CLI tool for the timewarp-builder repository to provide consistent development commands. The dev CLI should follow the pattern used in other TimeWarp repositories and support common operations like build, test, clean, and pack.

Reference implementation: See other TimeWarp repos (timewarp-nuru, timewarp-amuru) for examples of dev CLI structure.

## Checklist

### Project Setup

- [x] **Create dev CLI project structure**
  - Create `tools/dev-cli/` directory
  - Create `tools/dev-cli/dev.cs` (entry point runfile)
  - Create `tools/dev-cli/endpoints/` directory for command implementations
  - Add dev CLI project to solution (`timewarp-builder.slnx`)

- [x] **Add required package references**
  - `TimeWarp.Nuru` (for CLI framework)
  - `TimeWarp.Amuru` (for process execution)
  - `TimeWarp.Terminal` (for console output)
  - Reference the local `timewarp-builder` project if needed for testing

### Core Commands

- [x] **Implement `build` command**
  - Build the main library: `dotnet build source/timewarp-builder/`
  - Support `--configuration` option (Debug/Release)
  - Support `--verbose` option
  - Return appropriate exit codes

- [x] **Implement `test` command**
  - Run all tests: `dotnet test` (once test project exists)
  - Support `--filter` option for selective test running
  - Support `--no-build` option
  - Show test results summary

- [x] **Implement `clean` command**
  - Clean build artifacts: `dotnet clean`
  - Remove `artifacts/` directory contents
  - Remove `bin/` and `obj/` folders from all projects

- [x] **Implement `pack` command**
  - Create NuGet package: `dotnet pack`
  - Support `--configuration Release` default
  - Support `--output` option for package location
  - Output package to `artifacts/packages/`

- [x] **Implement `self-install` command**
  - AOT compile the dev CLI itself to `./bin/dev`
  - Use `dotnet publish` with AOT flags
  - Make executable: `chmod +x ./bin/dev` (on Unix)
  - This enables fast subsequent runs without JIT overhead

### Additional Commands (Optional but Recommended)

- [ ] **Implement `restore` command**
  - Restore NuGet packages: `dotnet restore`
  - Verify Central Package Management is working

- [x] **Implement `format` command**
  - Run `dotnet format` to apply .editorconfig rules
  - Support `--verify` option for CI (check-only, no changes)

- [ ] **Implement `lint` command**
  - Run analyzers and report warnings as errors
  - Essentially `dotnet build` with strict settings

- [x] **Implement `workflow` command**
  - Run full CI/CD pipeline: clean -> build -> test (PR mode)
  - Release mode: clean -> build -> check-version -> pack -> push
  - Auto-detect mode from `GITHUB_EVENT_NAME`

### Integration & Documentation

- [x] **Wire up in solution**
  - Add `<Project Path="tools/dev-cli/dev.cs">` to `timewarp-builder.slnx`
  - Or use folder structure if dev-cli is a folder-based runfile

- [ ] **Update .gitignore** (leave unchecked — bin/ is already covered by existing .gitignore)
  - Ensure `./bin/` is ignored (for self-installed dev CLI)
  - Ensure `tools/dev-cli/bin/` and `obj/` are ignored

- [ ] **Update AGENTS.md or create docs** (leave unchecked — not done yet)
  - Document available dev CLI commands
  - Add usage examples
  - Reference: See other TimeWarp repo AGENTS.md files for patterns

## Results

**Commit:** e432432 (initial), subsequent commits for renaming  
**Date:** 2026-02-23

### What was implemented
- `tools/dev-cli/dev.cs` — Entry point runfile using TimeWarp.Nuru Endpoint DSL
- `tools/dev-cli/Directory.Build.props` — Build config with AOT support, global usings, package references
- `tools/dev-cli/endpoints/build.cs` — Build library with `--configuration` and `--verbose` options
- `tools/dev-cli/endpoints/clean.cs` — Clean project + delete all bin/obj directories (targets .csproj not .slnx)
- `tools/dev-cli/endpoints/pack.cs` — Create NuGet packages to `artifacts/packages/`
- `tools/dev-cli/endpoints/test.cs` — Run test suite (gracefully handles missing tests directory)
- `tools/dev-cli/endpoints/format.cs` — Check/fix code formatting via `dotnet format`
- `tools/dev-cli/endpoints/self-install.cs` — AOT compile dev CLI to `./bin/dev`
- `tools/dev-cli/endpoints/workflow.cs` — Full CI/CD pipeline orchestration (ci command)
- `.envrc` — `PATH_add bin` for direnv integration
- `.github/workflows/workflow.yml` — GitHub Actions workflow calling `dev ci`

### Files modified
- `Directory.Packages.props` — Added TimeWarp.Nuru 3.0.0-beta.54; updated TimeWarp.Amuru to 1.0.0-beta.20
- `timewarp-builder.slnx` — Added `/tools/` solution folder with `tools/dev-cli/dev.cs`

### Key decisions
- Used `Shell.Builder` fallback for `dotnet test` — `DotNet.Test()` does not exist in Amuru API
- Directory renamed from `commands/` to `endpoints/` to follow Nuru convention
- Files renamed from `*-command.cs` to `*.cs` (e.g., `build-command.cs` → `build.cs`)
- Renamed `Configuration` → `Config` in build/pack commands to avoid source-generator naming conflicts
- `clean.cs` targets `.csproj` directly instead of `.slnx` (the slnx parser fails on runfile entries)
- `test` command gracefully handles the case where `tests/` directory doesn't exist yet
- `.envrc` placed at repo root; `direnv allow` needed once after cloning

### Test results
CLI compiles and `--help` shows all 7 commands:
```
Commands:
  build         Build the TimeWarp.Builder library
  ci            Run full CI/CD pipeline
  clean         Clean solution and build artifacts
  format        Check or fix code formatting
  pack          Create NuGet packages
  self-install  AOT compile and install dev CLI to ./bin
  test          Run the test suite
```

`dev ci` runs successfully:
- Step 1/3 Clean ✅
- Step 2/3 Build ✅ (0 warnings, 0 errors, package produced)
- Step 3/3 Test ✅ (gracefully reports no test project yet)

## Notes

### What is a Dev CLI?

A dev CLI is a repository-specific command-line tool that provides standardized development tasks. Unlike generic `dotnet` commands, it:
- Encapsulates repository-specific knowledge (paths, configurations)
- Provides a consistent interface across all TimeWarp projects
- Can be AOT-compiled for fast execution via `self-install`
- Exposes `--capabilities` JSON for AI agent integration

### Why TimeWarp.Nuru?

TimeWarp.Nuru provides:
- Route-based command dispatch (like ASP.NET routing)
- Source generator for AOT compatibility
- Built-in help generation
- Consistent patterns across TimeWarp repos

### Actual Structure in This Repo

```
tools/
  dev-cli/
    dev.cs              # Entry point (runfile)
    Directory.Build.props
    endpoints/          # Named endpoints/ not commands/
      build.cs
      clean.cs
      format.cs
      pack.cs
      self-install.cs
      test.cs
      workflow.cs
```

### Commands Available

| Command | Description | Example Usage |
|---------|-------------|---------------|
| `build` | Build the library | `./bin/dev build --configuration Release` |
| `test` | Run tests | `./bin/dev test --filter "AlsoTests"` |
| `clean` | Clean artifacts | `./bin/dev clean` |
| `pack` | Create NuGet package | `./bin/dev pack` |
| `format` | Format code | `./bin/dev format` |
| `self-install` | Install dev CLI | `dotnet run tools/dev-cli/dev.cs -- self-install` |
| `ci` / `workflow` | Run CI/CD pipeline | `dotnet run tools/dev-cli/dev.cs -- ci` |

### AOT Considerations

The dev CLI should be AOT-compatible:
- Use `TimeWarp.Nuru` source generators (already AOT-ready)
- Avoid reflection in command implementations
- Use `TimeWarp.Amuru` for process execution instead of `System.Diagnostics.Process`
- Test with `dotnet publish -p:PublishAot=true`

### First-Time Setup

```bash
# Initial setup (run once)
dotnet run tools/dev-cli/dev.cs -- self-install

# Subsequent usage (fast AOT version)
./bin/dev build
./bin/dev test
./bin/dev ci
```

### Related Tasks

- Task 001: Address code review blockers for v1.0.0 release
  - Includes adding tests, which will run via `dev test` in CI
  - The `dev ci` command already handles the case gracefully when no tests exist