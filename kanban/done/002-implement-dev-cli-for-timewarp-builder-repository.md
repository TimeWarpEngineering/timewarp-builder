# Implement dev CLI for timewarp-builder repository

## Description

Create a TimeWarp.Nuru-based dev CLI tool for the timewarp-builder repository to provide consistent development commands. The dev CLI should follow the pattern used in other TimeWarp repositories and support common operations like build, test, clean, and pack.

Reference implementation: See other TimeWarp repos (timewarp-nuru, timewarp-amuru) for examples of dev CLI structure.

## Checklist

### Project Setup

- [x] **Create dev CLI project structure**
  - Create `tools/dev-cli/` directory
  - Create `tools/dev-cli/dev.cs` (entry point runfile)
  - Create `tools/dev-cli/commands/` directory for command implementations
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
  - Support `--verbosity` option
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

**Commit:** e432432  
**Date:** 2026-02-23

### What was implemented
- `tools/dev-cli/dev.cs` — Entry point runfile using TimeWarp.Nuru Endpoint DSL
- `tools/dev-cli/Directory.Build.props` — Build config with AOT support, global usings, package references
- `tools/dev-cli/commands/build-command.cs` — Build library with `--configuration` and `--verbose` options
- `tools/dev-cli/commands/clean-command.cs` — Clean solution + delete all bin/obj directories
- `tools/dev-cli/commands/pack-command.cs` — Create NuGet packages to `artifacts/packages/`
- `tools/dev-cli/commands/test-command.cs` — Run test suite (gracefully handles missing tests directory)
- `tools/dev-cli/commands/format-command.cs` — Check/fix code formatting via `dotnet format`
- `tools/dev-cli/commands/self-install-command.cs` — AOT compile dev CLI to `./bin/dev`
- `.envrc` — `PATH_add bin` for direnv integration

### Files modified
- `Directory.Packages.props` — Added TimeWarp.Nuru 3.0.0-beta.54; updated TimeWarp.Amuru to 1.0.0-beta.20
- `timewarp-builder.slnx` — Added `/tools/` solution folder with `tools/dev-cli/dev.cs`

### Key decisions
- Used `Shell.Builder` fallback for `dotnet test` — `DotNet.Test()` does not exist in Amuru API
- Renamed `Configuration` → `Config` in build/pack commands to avoid source-generator naming conflicts
- `test` command gracefully handles the case where `tests/` directory doesn't exist yet (returns exit code 1 with helpful message)
- `.envrc` placed at repo root; `direnv allow` needed once after cloning

### Test results
CLI compiles and `--help` shows all 6 commands (build, clean, format, pack, self-install, test):
```
Commands:
  build         Build the TimeWarp.Builder library
  clean         Clean solution and build artifacts
  format        Check or fix code formatting
  pack          Create NuGet packages
  self-install  AOT compile and install dev CLI to ./bin
  test          Run the test suite
```

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

### Reference Implementation Pattern

Typical dev CLI structure in TimeWarp repos:

```
tools/
  dev-cli/
    dev.cs              # Entry point (runfile)
    commands/
      build.cs          # Build command handler
      test.cs           # Test command handler
      clean.cs          # Clean command handler
      pack.cs           # Pack command handler
      self-install.cs   # Self-install command handler
```

### Commands to Expose

Based on this repository's needs:

| Command | Description | Example Usage |
|---------|-------------|---------------|
| `build` | Build the library | `./bin/dev build --configuration Release` |
| `test` | Run tests | `./bin/dev test --filter "AlsoTests"` |
| `clean` | Clean artifacts | `./bin/dev clean` |
| `pack` | Create NuGet package | `./bin/dev pack` |
| `restore` | Restore packages | `./bin/dev restore` |
| `format` | Format code | `./bin/dev format` |
| `self-install` | Install dev CLI | `dotnet run tools/dev-cli/dev.cs -- self-install` |

### AOT Considerations

The dev CLI should be AOT-compatible:
- Use `TimeWarp.Nuru` source generators (already AOT-ready)
- Avoid reflection in command implementations
- Use `TimeWarp.Amuru` for process execution instead of `System.Diagnostics.Process`
- Test with `dotnet publish -p:PublishAot=true`

### Integration with Existing Build

The dev CLI should work with existing infrastructure:
- Respect `Directory.Build.props` settings
- Use `msbuild/repository.props` paths where appropriate
- Output to existing `artifacts/` directory structure

### First-Time Setup

Document how developers first use the dev CLI:

```bash
# Initial setup (run once)
dotnet run tools/dev-cli/dev.cs -- self-install

# Subsequent usage (fast AOT version)
./bin/dev build
./bin/dev test
./bin/dev pack
```

### Related Tasks

- Task 001: Address code review blockers for v1.0.0 release
  - When implementing dev CLI `test` command, coordinate with test project creation
  - The dev CLI will need to know how to run the test project once it exists

### Files to Create/Modify

**New files:**
- `tools/dev-cli/dev.cs`
- `tools/dev-cli/commands/build.cs`
- `tools/dev-cli/commands/test.cs`
- `tools/dev-cli/commands/clean.cs`
- `tools/dev-cli/commands/pack.cs`
- `tools/dev-cli/commands/self-install.cs`
- `tools/dev-cli/global-usings.cs` (if needed)

**Modified files:**
- `timewarp-builder.slnx` - add dev CLI project reference
- `.gitignore` - ensure dev CLI build artifacts are ignored
- `AGENTS.md` - document dev CLI usage (or create if doesn't exist)
