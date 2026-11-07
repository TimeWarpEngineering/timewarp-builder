# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** branch diff against the merge base with master (see `../review-framework.md`)

## Summary

The change moves the root `Directory.Build.props` from `TargetFramework` net10.0 to `TargetFrameworks` net10.0;net11.0. It pins the dev CLI and tests back to net10.0, archives the stale 006 journal kitchen as 008, applies the audit baseline, and bumps the version to 1.1.0. The MSBuild layering is correct. I ran the `.githooks/pre-commit.cs` runfile in a clean copy of HEAD, and it still runs (exit 0). The done 006 kitchen has no diff. The dominant risk is CI. The workflow installs only the .NET 10 SDK, and that SDK cannot build the new net11.0 target. Every PR or release run of this branch would fail.

Goal restatement check: the restatement matches the task description and checklist. The 1.1.0 bump is extra to the description, but it is a checklist item and is needed to release. No misread.

## Issues

### Issue 1 — Severity: bug
- File: .github/workflows/workflow.yml:53
- Description: `actions/setup-dotnet` installs only `dotnet-version: '10.0.x'`. Root `Directory.Build.props:18` now adds net11.0, and the .NET 10 SDK cannot target it. I reproduced the failure on a clean `git archive HEAD` copy, pinned to SDK 10.0.403 through a temp `global.json`. `dotnet build source/timewarp-builder/timewarp-builder.csproj -c Release` fails with `error NETSDK1045: The current .NET SDK does not support targeting .NET 11.0` for `TargetFramework=net11.0`. This PR touches `Directory.Build.props`, which is on the workflow `paths` filter, so CI runs on it. `dotnet run tools/dev-cli/dev.cs -- workflow` will fail at build and pack. A release run could not publish the 1.1.0 package. The local validation passed only because this machine also has SDK 11.0.100-rc.3, and with no `global.json` the newest SDK wins.
- Suggestion: install both SDKs in the Setup .NET step: `dotnet-version: |` with `10.0.x` and `11.0.x`. While .NET 11 is still RC, add `dotnet-quality: preview` (or pin the exact 11.0.100-rc SDK). Then confirm CI goes green on the PR. Optionally record in Results that the net11.0 asset is built with an RC SDK until .NET 11 reaches GA.
- Status: open
