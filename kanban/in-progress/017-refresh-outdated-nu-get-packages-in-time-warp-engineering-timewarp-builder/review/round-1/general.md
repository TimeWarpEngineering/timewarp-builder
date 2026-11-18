# Round 1 — general
**Date:** 2026-11-19
**Scope reviewed:** branch `task/017-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `master` (`Directory.Packages.props` and the four dev-cli endpoints), plus kitchen `## Goal (implementer restatement)`, `## Results`, and `### How to validate`

## Summary

The diff bumps the six pins the task names and leaves `source/Directory.Build.props` `<Version>2.1.0-beta.1</Version>` untouched. Amuru 3.0.0-beta.10 rejects `Shell.Builder("dotnet")` and `Shell.Builder("gh")`; the four endpoint edits move those call sites onto typed builders and add no suppression. Decompiled `TimeWarp.Amuru.Tools` 3.0.0-beta.10 builds the same argument lists as the old shell calls: `dotnet format` with `--severity warn` and `--verify-no-changes`, `dotnet run --file … -- self-install`, `dotnet tool restore`, `dotnet tool run dotnet-coverage` with the previous collect arguments, `gh run list --json databaseId,event,headSha,createdAt`, `gh api` with the `{owner}/{repo}` path, `gh run download` with `--name` and `--dir`, and `dotnet nuget push` with `--source`, `--api-key`, and `--skip-duplicate`. Remaining `Shell.Builder` calls are `git` and the installed `dev` binary. `## Results` records build, dev-cli self-install, the six Jaribu runfiles, `./bin/dev format`, and audit. The goal restatement matches the task. No issues.

## Issues

<!-- none -->
