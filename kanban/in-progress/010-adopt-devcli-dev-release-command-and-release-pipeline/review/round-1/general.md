# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** same as framework (branch vs merge-base with master)

## Summary

The change swaps the hand-rolled clean, check-version, and self-install endpoints for
TimeWarp.Nuru.DevCli 4.0.0-beta.11, adds `release`, and replaces the rebuild-on-release
workflow with the six-step Packages-* promotion (tag-gate, check-version, locate-run,
download-artifact, verify, push to nuget.org). The `workflow-command*.cs` files are a
close copy of timewarp-ramani with the GitHub Packages push removed, as the task asks.
The `workflow.yml` artifact retention and keep-last-two prune match timewarp-amuru, and
the `NoWarn` list matches the amuru and ramani dev CLIs. `workflow_dispatch` with no
`--mode` maps to Merge in DevCli's `CiModeDetector`, so a default dispatch cannot
publish. Break-glass is gated on `confirm=release` both before NuGet login and in the
run step. `<Version>` is unchanged at 1.1.0.

Verified in this worktree: `dotnet run --file tools/dev-cli/dev.cs -- --help` lists
`release`, `clean`, `check-version`, and `self-install` next to the repo commands.
`dev build` reports 0 warnings and 0 errors. `dev test` passes all 6 Jaribu files.

The code and workflow are sound. Both findings are in the operator guide, which the
goal says must be enough to cut a release and to resume one that stopped partway.

## Issues

### Issue 1 — Severity: bug
- File: documentation/developer/guides/releasing.md:22
- Description: The guide says "A push that changes only kanban or documentation does not run CI. `dev release` walks back over those commits to the newest green run." Only the first half is true. The `push` paths filter in `workflow.yml` leaves out `documentation/**` (and `.timewarp/**`, `AGENTS.md`, and other paths), so those pushes get no CI run. But DevCli 4.0.0-beta.11 only walks back over **kanban-only** commits: `ReleaseGuard.NonShippingPathPrefix = "kanban/"`, and `SelectReleaseTarget` stops at the first commit with any path outside `kanban/` (`release-guard.cs:125`, `:238-264`). If master HEAD is a docs-only merge, which is likely because this task itself lands a guide, `dev release` refuses at guard 8 with "no successful CI run". The guide does not mention that case or how to recover. Guard 8 at lines 62-64 says "kanban-only" correctly, so the guide also contradicts itself.
- Suggestion: Change line 22-23 to say that only kanban-only commits are walked back over. Add the recovery for a HEAD with no green run (for example a docs-only merge): run `gh workflow run workflow.yml --ref master -f mode=merge`. That run is a `workflow_dispatch` on `refs/heads/master`, so it uploads `Packages-*`, and `CiRunPromotion.OrderCandidateRuns` accepts it as a candidate (only `pull_request` runs are excluded). Then rerun `dev release --dry-run`. Widening the paths filter is the other option, but the sibling repos use the same filter, so the doc fix is the smaller change.
- Status: open

### Issue 2 — Severity: suggestion
- File: documentation/developer/guides/releasing.md:85
- Description: `Packages-*` has `retention-days: 7` and keep-last-two pruning, and the 1.1.0 release is deferred until .NET 11 GA. DevCli guard 8 checks only that a successful run exists. It does not check that the run's artifact is still live. So `dev release` can tag and publish the GitHub Release, and then the promotion run fails at download-artifact because the artifact expired. The guide's only remedy is `gh run rerun <run-id>`, and GitHub refuses to rerun a run more than 30 days old. A deferred release is that old. The workable recovery is not documented: a merge-mode dispatch on master while HEAD is still the tag commit, then the break-glass dispatch with `--ref v{Version}`. Neither is the simpler prevention of refreshing the artifact before cutting.
- Suggestion: In "Normal release flow" step 2, add a pre-check. If the last green master run is older than 7 days, or newer master pushes have pruned it, run `gh workflow run workflow.yml --ref master -f mode=merge` and wait for it to go green before `dev release`. In step 4 and under Break-glass, add a note that `gh run rerun` works only within 30 days. Past that, refresh the artifact with a merge-mode dispatch at the tagged commit, then use break-glass.
- Status: open
