# Round 1 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:22
- Description: The guide claims `dev release` walks back over documentation-only commits. DevCli 4.0.0-beta.11 walks back over kanban-only commits only (`NonShippingPathPrefix = "kanban/"`). A docs-only master HEAD has no CI run, because the push paths filter skips `documentation/**`, so `dev release` refuses at guard 8. The guide has no recovery for that case, and line 22 contradicts guard 8 at line 62.
- Suggestion: Say "kanban-only" at line 22. Document the recovery: `gh workflow run workflow.yml --ref master -f mode=merge`, wait for green, then `dev release --dry-run`.
- Source: general
- Disposition notes: Fixed in `releasing.md`. The overview and guard 8 say the walk covers only `kanban/`. A docs-only HEAD is recovered with `gh workflow run workflow.yml --ref master -f mode=merge`, then `dev release --dry-run` after that run is green.

### M2 — Severity: suggestion — Status: fixed
- File: documentation/developer/guides/releasing.md:85
- Description: The 7-day retention and keep-last-two pruning, with a release deferred to .NET 11 GA, mean the artifact will probably have expired. `dev release` does not check that the artifact is live, so it can tag and release, and then the promotion run fails. The guide's remedy, `gh run rerun`, does not work once the run is more than 30 days old. The working recovery and the prevention are not documented.
- Suggestion: Add a pre-release artifact freshness step (merge-mode dispatch on master when the last green run is more than 7 days old or was pruned). Note the 30-day rerun limit, and describe the dispatch-then-break-glass recovery at the tagged commit.
- Source: general
- Disposition notes: Fixed in `releasing.md`. Step 2 checks the guard 8 run's `Packages-*` artifact and refreshes with a merge-mode dispatch on `master` when it is missing, expired, or older than 7 days. Download and Break-glass state the 30-day `gh run rerun` limit. Past that, while `master` is still the tagged commit, the recovery is the same merge-mode dispatch, then break-glass on the tag. The upload step requires `refs/heads/master`, so the refresh is not a tag ref.

## Duplicates / conflicts

- None. Single reviewer.
