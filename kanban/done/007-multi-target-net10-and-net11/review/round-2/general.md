# Round 2 — general
**Date:** 2026-11-07
**Scope reviewed:** the delta since round 1 base `3b983eb` (`3b983eb..HEAD`). That delta is `edc9d63` (ci: install the .NET 11 RC SDK) plus the kanban records. The only product file it changes is `.github/workflows/workflow.yml`.

## Summary

The fix splits SDK setup into two `actions/setup-dotnet@v4` steps. Setup .NET 10 installs `10.0.x` with no quality input. Setup .NET 11 installs `11.0.x` with `dotnet-quality: 'preview'`. This resolves round 1 M1, where CI had no SDK that could build the net11.0 target.

I re-checked the facts the fix relies on with the setup-dotnet v4.3.1 `install-dotnet.sh` dry-run:

- `--channel 10.0` resolves to SDK 10.0.403 (GA).
- `--channel 11.0 --quality preview` resolves to SDK 11.0.100-rc.3.26522.148.
- `--channel 10.0 --quality preview` resolves to 10.0.100-rc.2.25502.107. This confirms the code comment: one combined step with preview quality would pull the 10.0 RC instead of the GA SDK.

There is no `global.json`, so `dotnet` picks the newest SDK, which is the 11 RC. The 11 RC SDK can build net10.0, and the 10.0 step installs the net10.0 runtime that the dev CLI and runfiles run on. Locally the same two SDKs are installed, and `dotnet build source/timewarp-builder/timewarp-builder.csproj -c Release` succeeded with 0 warnings and 0 errors for both frameworks.

The risk is low. Only a CI run on the PR proves the hosted runner, and that run is a later host step.

## Issues

None.

## Prior findings

- M1 (bug): **fixed**. Verified against `.github/workflows/workflow.yml:50-60`.

## Goal check

The `## Goal (implementer restatement)` matches the task. It adds the version bump and the CI SDK as acceptance items. Both follow from shipping the multi-target package, and they are not a misread.
