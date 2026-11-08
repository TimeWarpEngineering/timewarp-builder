# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** branch vs `master` merge base. The product change is `Directory.Packages.props` (7 lines). Also reviewed: the task.md Results and the `.editorconfig` / `Directory.Build.props` analyzer settings.

## Summary

The change raises all seven outdated central `PackageVersion` pins to the versions listed in the task:
TimeWarp.Jaribu 1.0.0-beta.15, the three Roslynator packages 5.0.1, NetAnalyzers 10.0.403, CSharp.CodeStyle 5.9.0, and BannedApiAnalyzers 5.6.0.
No source, `.editorconfig`, or `BannedSymbols` file changed, so no rule was suppressed or carved out.
The risk is low.

Checks I ran in the claim worktree:

- `ganda nuget outdated`: all 13 packages are up to date.
- `./bin/dev build` (Release, net10.0 and net11.0): 0 warnings, 0 errors.
- `dotnet build timewarp-builder.slnx --no-incremental`: 0 warnings, 0 errors. `Directory.Build.props` sets `TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`, `EnforceCodeStyleInBuild`, and `AnalysisMode=All`.
- `obj/project.assets.json` resolves Roslynator 5.0.1, BannedApiAnalyzers 5.6.0, CodeStyle 5.9.0, and NetAnalyzers 10.0.403, so the build ran the new analyzers.
- `./bin/dev test`: all 6 Jaribu runfiles pass.
- `Directory.Packages.props` has no UTF-8 BOM, which matches the Results note.
- The `.githooks/*.cs` `#:package` directives have no inline versions, so the central pins cover them.
- `master` is 1 commit ahead (`land kanban 009`). That commit changes kanban files only. `git diff HEAD master` shows only `Directory.Packages.props`.

The goal restatement matches the task. It adds the walk's pre-PR gate and push steps, which is consistent with the host walk and is not a misread.

## Issues

None.
