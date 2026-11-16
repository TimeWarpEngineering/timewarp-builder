# Round 1 — general
**Date:** 2026-11-16
**Scope reviewed:** branch `task/016-adopt-the-shared-pull-request-build-classifier-and` vs `master`

## Summary

The change pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at 4.0.0-beta.15, calls `PullRequestBuildClassifier.DecideAsync(RepoRoot, Ct)` at the start of the non-release pipeline, and lists `readme.md` and `assets/logo.png` under `pullRequestBuild.buildPaths`. That matches the task and the implementer restatement. There is no misread. The recorded build, test, audit, and smoke cover the delta. No issues.

## Goal check

The restatement tracks the original requirements. `workflow.yml` already had no `pull_request` paths filter and already kept the `push` filter with the kanban-only why comment, so leaving that file untouched is correct. Packed `readme.md` and `assets/logo.png` are the paths the defaults would skip; they are data in `.timewarp/dev.jsonc`, not a local copy of the rule. `TimeWarp.Amuru` 3.0.0-beta.5 and `TimeWarp.Terminal` 2.0.0-beta.2 are the NU1605 companions of the Nuru pin, stated in Results.

## Contract check

`DecideAsync(string, CancellationToken)` in DevCli 4.0.0-beta.15 reads `GITHUB_EVENT_NAME`, `GITHUB_EVENT_PATH`, and `.timewarp/dev.jsonc` `pullRequestBuild` itself. `RepoConfigService.Parse` skips comments and matches property names case-insensitively, so `buildPaths` binds. A non-pull-request event, including `push` and a local run, returns `RequiresBuild` true. Release still goes through `RunReleaseAsync` and does not classify. On skip, `RunPrAsync` has already set `Environment.ExitCode` to 0, prints `decision.Message`, and returns before self-install. The audit token `PullRequestBuildClassifier.DecideAsync(` is present. No second copy of the rule is in `tools/dev-cli/`.

`assets/timewarp-builder-avatar.svg` stays a default non-build path. It is not packed. The `push` filter already lists `readme.md` and `assets/logo.png` and does not list the avatar.

## Recorded results

`## Results` and `### How to validate` name `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` (6 files, exit 0), and `ganda repo audit` (44 passed, 0 failed, 3 skipped), plus the kanban-only skip smoke, the readme-only build smoke (scratch commit 956c1d0), and the local full-build smoke. That record covers this change. Gates were not re-run.

## Issues

None.
