# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-builder

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-builder.
Filter: (all)
Outdated:
  TimeWarp.Amuru 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Amuru.Tools 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Jaribu 1.0.0-beta.16 -> 1.0.0-beta.18 (dev-only: tests/Directory.Build.props)
  TimeWarp.SourceGenerators 1.0.0-beta.16 -> 1.0.0-beta.17 (dev-only: PrivateAssets=all on Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the six dev-only TimeWarp package pins in `Directory.Packages.props` to their latest
prereleases (Amuru/Amuru.Tools 3.0.0-beta.10, Nuru/Nuru.DevCli 4.0.0-beta.16, Jaribu
1.0.0-beta.18, SourceGenerators 1.0.0-beta.17).

Acceptance criteria:
- The version bump touches only `Directory.Packages.props`; `<Version>` in `source/Directory.Build.props` is untouched.
- Anything the bumped packages break in this repo (e.g. new analyzers in the dev CLI) is fixed on this task, without suppressions.
- `./bin/dev build` passes, the dev CLI still compiles and self-installs, the Jaribu test runfiles run green, and `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
- Amuru 3.0.0-beta.10 ships analyzers AMURU0001/AMURU0002 that reject `Shell.Builder("dotnet"|"gh")`;
  the dev CLI failed to compile with 9 errors. Migrated each call site to the typed builder
  (no suppressions): `DotNet.Format`, `DotNet.Tool().Restore()`, `DotNet.Tool().Run("dotnet-coverage")`,
  `DotNet.Run().WithFile`, `DotNet.NuGet().Push`, `Gh.Run().List()`, `Gh.Api`, `Gh.Run().Download`.
- Verified each new builder's `Build().ToCommandString()` with a throwaway runfile: argument lists
  match the old ones. Coverage now runs `dotnet tool run dotnet-coverage …` (was `dotnet dotnet-coverage …`, same local tool).
- `ganda repo audit` first run: pass with 1 advisory warning (`vscode-window-icon`); `ganda repo audit --fix`
  wrote the gitignored `.vscode/settings.json` (no tracked change); rerun is clean.

## Results

Bumped in `Directory.Packages.props`: TimeWarp.Amuru and TimeWarp.Amuru.Tools 3.0.0-beta.5 → 3.0.0-beta.10,
TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.15 → 4.0.0-beta.16, TimeWarp.Jaribu 1.0.0-beta.16 → 1.0.0-beta.18,
TimeWarp.SourceGenerators 1.0.0-beta.16 → 1.0.0-beta.17. `source/Directory.Build.props` `<Version>` unchanged.

Dev CLI migrated to typed Amuru builders: `tools/dev-cli/endpoints/format.cs`, `test.cs`,
`workflow-command.cs`, `workflow-command.release-publish.cs`.

### Gates

- `./bin/dev build` — pass (0 warnings, 0 errors), before and after dev CLI reinstall
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` — pass (was 9 AMURU errors before the migration)
- `dotnet run --file tests/i-builder.build.cs` — pass (3/3)
- `dotnet run --file tests/i-nested-builder.done.cs` — pass (4/4)
- `dotnet run --file tests/scope-extensions.also.cs` — pass (4/4)
- `dotnet run --file tests/scope-extensions.apply.cs` — pass (3/3)
- `dotnet run --file tests/scope-extensions.let.cs` — pass (4/4)
- `dotnet run --file tests/scope-extensions.run.cs` — pass (3/3)
- `./bin/dev format` (smoke of migrated format endpoint) — pass
- `ganda repo audit` — pass (after `--fix` of advisory `vscode-window-icon`; untracked file only)

### How to validate

Smoke:
```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev build
./bin/dev format
dotnet run --file tests/scope-extensions.let.cs
```

Expect: self-install succeeds with no AMURU0001/AMURU0002 errors; build reports 0 warnings / 0 errors;
format prints "✅ Code style check passed!"; the test runfile reports all tests passed.
