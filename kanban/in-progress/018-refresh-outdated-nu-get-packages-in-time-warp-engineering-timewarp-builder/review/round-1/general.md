# Round 1 — general
**Date:** 2026-11-26
**Scope reviewed:** branch `task/018-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `master` (`Directory.Packages.props`, `tools/dev-cli/dev.cs`, `.timewarp/line-cap-baseline.json`), plus kitchen `## Goal (implementer restatement)`, `## Results`, and `### How to validate`

## Summary

The diff bumps TimeWarp.Nuru and TimeWarp.Nuru.DevCli from 4.0.0-beta.16 to 4.0.0-beta.17 in the root `Directory.Packages.props`. `source/Directory.Build.props` is absent from the diff, so `<Version>2.1.0-beta.1</Version>` stays. `tools/dev-cli/Directory.Build.props` references both packages with no version, so the dev CLI takes the central pins. The goal restatement matches the task: the same two pins, the same versions, and no `<Version>` change. The two extra edits are the prescribed responses to the pre-existing audit errors named in `## Results`. `dev.cs` adds `.UseTelemetry(options => options.ServiceName = "timewarp-builder-dev")`, the call `DevCliTelemetryCheck` tells a repo to add. `.timewarp/line-cap-baseline.json` records `.agent/workspace/2026-02-06-t00-00-00-ubuntu-custom-tools-discovery-guide.md` at 747, which matches the file. That file is tracked, and `master` has no line-cap baseline, so this is the check's first adoption rather than a raised ratchet. `## Results` records `./bin/dev build` (0 warnings, 0 errors) after the beta.17 self-install, and `ganda repo audit` passing after those two edits, with the non-blocking `vscode-window-icon` advisory. No product test files changed; the record says CI runs the suite. Gates were not re-run.

## Issues

<!-- none -->
