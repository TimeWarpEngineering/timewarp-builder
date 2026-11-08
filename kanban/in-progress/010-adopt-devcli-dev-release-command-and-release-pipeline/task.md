# Adopt DevCli dev release command and release pipeline

## Description

timewarp-builder has no `dev release` command. Its dev CLI is a hand-rolled Nuru app
(`tools/dev-cli/`) with build, check-version, clean, format, pack, self-install, test,
verify-samples, and workflow, but no release. `workflow.yml` publishes by rebuilding on
`release: published`, and the GitHub Release is created by hand. This is not the
TimeWarp convention (see the `tw-release` skill).

Bring the repo in line with the other TimeWarp library repos so a release is
`dev release --dry-run` then `dev release` from a clean, synced master:

- the dev CLI uses `TimeWarp.Nuru.DevCli` (4.0.0-beta.8 or newer) and exposes `release`
- the release pipeline promotes the CI-built `Packages-*` artifact (byte-identical, never
  rebuilt): tag-gate, check-version, locate-run, download-artifact, verify, push via OIDC
- break-glass `workflow_dispatch` with `mode: release` + `confirm: release`, and keep `mode: probe`
- `documentation/developer/guides/releasing.md`

Copy the pattern from an existing repo rather than inventing one. timewarp-ramani is the
closest match: a small library with `TimeWarp.Nuru.DevCli` in `Directory.Packages.props`
and `documentation/developer/guides/releasing.md`. Cross-check against timewarp-amuru or
timewarp-nuru.

Blocks the TimeWarp.Builder 1.1.0 release. The version is already 1.1.0 in
`source/Directory.Build.props`, so this task must not change the version. The release
itself waits for .NET 11 GA and is out of scope here.

## Goal (implementer restatement)

A release of this repo is `dev release --dry-run`, then `dev release`, from a clean
master that matches origin. DevCli supplies the release command. The workflow
promotes the nupkg CI already packed and tested. This task wires that path. It
does not cut 1.1.0, and it does not change `<Version>`.

Done means:

- `clean`, `check-version`, `release`, and `self-install` come from TimeWarp.Nuru.DevCli
  4.0.0-beta.11. `build`, `test`, `pack`, `format`, `verify-samples`, and `workflow`
  stay in this repo.
- `dev --help` lists `release`. `dev release --dry-run` runs the guards and creates
  no tag and no GitHub Release.
- `workflow.yml` promotes `Packages-*`: tag-gate, check-version, locate-run,
  download-artifact, verify, then push to nuget.org with the short-lived OIDC key.
  The package is not rebuilt.
- `workflow_dispatch` accepts `merge`, `release`, and `probe`. `mode=release`
  publishes only when `confirm=release`. `mode=probe` only exchanges the OIDC token.
- `documentation/developer/guides/releasing.md` is enough to cut a release and to
  resume one that stopped partway.
- `dev self-install` writes `bin/dev`. The AOT publish does not fail with MSB3030
  because `dev.xml` is missing.
- The library build, the Jaribu runfiles, and `ganda repo audit` pass.

The guide's walk-back matches DevCli. Only commits whose paths are under `kanban/`
are skipped, and the walk stops at 50 commits. Any other path, including
`documentation/`, ends it. A docs-only master HEAD has no push run, because the
push filter skips `documentation/**`. The recovery is
`gh workflow run workflow.yml --ref master -f mode=merge`, then another dry-run
after that run is green.

The pre-cut check matches release mode. Guard 8 prints one run id, the first
candidate from `OrderCandidateRuns`, which puts a `push` run first. That id is
not the gate. The operator copies the full SHA from the latest dry-run line
`Target commit:`, lists every successful `workflow.yml` run at that SHA, skips
`pull_request`, and cuts when any remaining run has a `Packages-*` artifact
with `expired` false. When every such artifact is missing or expired, the
refresh is `gh workflow run workflow.yml --ref master -f mode=merge`. That
dispatch builds `master` HEAD. After it is green, the operator dry-runs again
and lists at the new `Target commit:` SHA. That SHA is unchanged when HEAD
was already the target, and in that case the guard 8 line still names the old
push run. It moves to HEAD when HEAD was kanban-only commits past the old
target, because the live artifact is the dispatch at HEAD. The old SHA still
lists every artifact expired, and that is not another refresh. A push run
older than 7 days is not a refresh either, once the SHA on the new
`Target commit:` line has a live artifact. Download says a later merge-mode
dispatch on `master` supplies the bytes when that dispatch ran at the commit
being released. `gh run rerun` lasts 30 days. Past that, while `master` is
still the tagged commit, recovery is the merge-mode dispatch on `master`,
then break-glass with `--ref` set to the existing tag.

## Checklist

- [x] Study timewarp-ramani's dev CLI, `workflow.yml`, and releasing guide
- [x] Add `TimeWarp.Nuru.DevCli` and wire its commands (keep repo-specific commands that still matter)
- [x] `dev --help` lists `release`; `dev release --dry-run` evaluates the guards read-only
- [x] Update `workflow.yml` to the artifact-promotion release pipeline plus break-glass and probe modes
- [x] Add `documentation/developer/guides/releasing.md`
- [x] Correct the guide: kanban-only walk-back, docs-only HEAD recovery, and expired-artifact recovery (review M1, M2)
- [x] Correct the pre-cut check: any live `Packages-*` on a non-pull-request run at the target commit, not the run guard 8 names (review M3)
- [x] After a merge-mode refresh, list artifacts at the new dry-run `Target commit:`. That SHA moves to HEAD when HEAD was kanban-only past the old target (review M4)
- [x] `bin/dev` (`dev self-install`) builds. It failed during the worktree on-create on 2026-11-08 with MSB3030, missing `dev.xml`
- [x] Build and tests green, `ganda repo audit` clean
- [x] Do not create a tag or GitHub Release

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-08)
- Implementation: 01a1bcc1-3c09-78e2-8fa0-ceeacadc6e70 (2026-11-09)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bcc1-3c09-78e2-8fa0-ceeacadc6e70, max-turns 500, Done — 2026-11-08T20:18:56Z
- Review oracle round 1: claude (claude-opus-5-5), effort 3, roster general, turn cap 200 — 2026-11-09. Result: Blocked, fix pass needed for open findings M1, M2.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 8e5d2731-2219-4431-bbb7-8e0cd5a9becc, max-turns 200, artifacts general.md, merged.md — 2026-11-08T20:22:50Z
- Fix pass (review round 1): 01a1bcd3-f740-7871-9832-821374e806d8 (2026-11-09). M1 and M2 recorded fixed on `review/round-1/merged.md`. Round 2 is the host's next node.
- Review oracle round 2: claude (claude-opus-5-5), effort 2, roster general, turn cap 120 — 2026-11-09. Scope `7059787..0070dea`. M1 and M2 verified fixed. New M3 (bug, open). Result: Blocked, fix pass needed for open finding M3.
- Fix pass (review round 2): 01a1bcdd-6707-7582-88dc-bb8d2756541f (2026-11-09). M3 recorded fixed on `review/round-2/merged.md`. Round 3 is the host's next node.
- Review oracle round 3: claude (claude-opus-5-5), effort 1, roster general, turn cap 80, 2026-11-09. Scope `0070dea..083d7c4`. M3 verified fixed. New M4 (bug, open). Result: Blocked, fix pass needed for open finding M4.
- Fix pass (review round 3): 01a1bce7-5da2-7911-b0f7-a84621d14579 (2026-11-09). M4 recorded fixed on `review/round-3/merged.md`. Round 4 is the host's next node.

## Results

`TimeWarp.Nuru.DevCli` 4.0.0-beta.11 supplies `clean`, `check-version`, `release`, and `self-install`. This repo keeps `build`, `test`, `pack`, `format`, `verify-samples`, and `workflow`. Release mode promotes `Packages-*` and pushes `TimeWarp.Builder` to nuget.org. It does not rebuild. `<Version>` is still 1.1.0. No tag and no GitHub Release were created.

`dev self-install` AOT-publishes because `tools/dev-cli/Directory.Build.props` sets `GenerateDocumentationFile` to false, so the publish no longer looks for `dev.xml`. Amuru stays at 2.0.0: 3.0.0-beta.1 is net11.0 only, and the git hooks restore that package. DevCli 4.0.0-beta.11 compiles against Amuru 2.0.0.

`ganda repo audit` first reported one warning, `vscode-window-icon`, because `.vscode/settings.json` was missing. `ganda repo audit --fix --checks vscode-window-icon` wrote that file. It is gitignored (`.gitignore` line `.vscode/*`). The re-audit reported Passed 38, Failed 0, Skipped 3.

On the clean task branch, `./bin/dev release --dry-run` passed the gh and clean-tree guards and stopped with "release must be cut from master". `git tag -l 'v1.1.0'` was empty. `./bin/dev workflow --mode pr` then ran self-install, clean, build, test, and pack and printed `Pipeline SUCCEEDED`. The packed file is `artifacts/packages/TimeWarp.Builder.1.1.0.nupkg`.

Review round 1's open findings were both in `documentation/developer/guides/releasing.md`. The guide says `dev release` walks back only over `kanban/` (at most 50 commits). A docs-only HEAD is recovered with `gh workflow run workflow.yml --ref master -f mode=merge`, then another dry-run after that run is green. Download and Break-glass state that `gh run rerun` lasts 30 days. Past that, while `master` is still the tagged commit, the recovery is that merge-mode dispatch (the upload step requires `refs/heads/master`) and then break-glass on the tag. Round 1 `merged.md` marks M1 and M2 fixed. Round 2 verified those two and opened M3.

This fix pass reran `ganda repo audit` (Passed 38, Failed 0, Skipped 3), `./bin/dev build` (0 warnings, both target DLLs), `./bin/dev test` (6 of 6 Jaribu files), and `./bin/dev check-version` (source 1.1.0, NuGet 1.0.0). `./bin/dev release --dry-run` stopped at the clean-tree guard while these edits were uncommitted. After that commit, the same command passed the clean-tree guard and stopped with "release must be cut from master". It did not run `gh release create`. `git tag -l 'v1.1.0'` was empty. `source/Directory.Build.props` was unchanged.

Review round 2's bug was the pre-cut gate. Step 2 no longer reads the run id from guard 8. Guard 8 prints `OrderCandidateRuns(...)[0]`, which is the push run when one exists, so a refresh leaves that line on the expired version-bump run. The guide now tells the operator to take the full SHA from the dry-run `Target commit:` line, list `gh run list --workflow workflow.yml --commit SHA --status success`, skip `pull_request`, and continue when any other run has a `Packages-*` artifact with `expired` false. Refresh is the same merge-mode dispatch, and only when every such artifact is missing or expired. The push run staying older than 7 days is not another refresh. Download says an expired push run is skipped so the later dispatch's artifact is the one promoted. Round 2 `merged.md` marks M3 fixed. Disposition waits for review round 3.

Review round 3's bug was the refresh loop that the M3 wording added. The merge-mode dispatch builds `master` HEAD. When HEAD is kanban-only commits past the dry-run target, the live `Packages-*` artifact is at HEAD, and the next dry-run moves `Target commit:` there. The guide no longer says to repeat the list at the SHA from before the refresh. After the run is green, the operator dry-runs again and lists at the new `Target commit:` SHA. That SHA stays when HEAD was already the target, and only then does guard 8 still name the old push run. It moves to HEAD when HEAD was kanban-only past the old target. Download no longer says the dispatch is "at the same commit". It says a later merge-mode dispatch on `master` supplies the bytes when that dispatch ran at the commit being released. Round 3 `merged.md` marks M4 fixed. Disposition waits for review round 4. No tag and no GitHub Release were created. `<Version>` is still 1.1.0.

This fix pass reran `ganda repo audit` (Passed 38, Failed 0, Skipped 3), `./bin/dev build` (0 warnings, both target DLLs, package `TimeWarp.Builder.1.1.0.nupkg`), `./bin/dev test` (6 of 6 Jaribu files), and `./bin/dev check-version` (source 1.1.0, NuGet 1.0.0). `./bin/dev release --dry-run` stopped at the clean-tree guard while these edits were uncommitted. `git tag -l 'v1.1.0'` was empty. `source/Directory.Build.props` was unchanged.

### How to validate

**Smoke:** from this worktree, on a clean tree:

```bash
./bin/dev --help
./bin/dev release --dry-run
./bin/dev check-version
./bin/dev build
./bin/dev test
git tag -l 'v1.1.0'
git diff -- source/Directory.Build.props
grep -n -E 'kanban-only|mode=merge|30 days|Target commit:|pull_request|OrderCandidateRuns' documentation/developer/guides/releasing.md
```

**Expect:** `release` is in `./bin/dev --help`. `./bin/dev release --dry-run` exits non-zero because this branch is not `master`, prints no `gh release create` execution, and `git tag -l 'v1.1.0'` stays empty. `./bin/dev check-version` prints `Packages checked: TimeWarp.Builder`, `Version in source: 1.1.0`, and `Latest NuGet version: 1.0.0`. `./bin/dev build` is 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. `source/Directory.Build.props` still has `<Version>1.1.0</Version>`. The guide's walk-back sentence names kanban-only commits and says a path outside `kanban/`, including `documentation/`, ends the walk. It contains `gh workflow run workflow.yml --ref master -f mode=merge` as the guard 8 recovery and as the pre-cut refresh. The pre-cut check copies the full SHA from `Target commit:`, runs `gh run list --workflow workflow.yml --commit SHA --status success`, skips `pull_request`, and continues when any other run has a `Packages-*` artifact with `expired` false. After the refresh is green it says to dry-run again and list at the new `Target commit:` SHA, and not to repeat the list at the SHA from before the refresh. It says that SHA moves to `HEAD` when `HEAD` was kanban-only commits past the old target, and that the old SHA still showing every artifact expired is not a reason to dispatch again. It limits "the push run that guard 8 names stays older than 7 days" to the case where `HEAD` was already the target. Download says a later merge-mode dispatch on `master` supplies the bytes when that dispatch ran at the commit being released. It says GitHub accepts `gh run rerun` for 30 days, and the break-glass section then dispatches merge mode on `master` while `master` is still the tagged commit.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root. `ganda repo audit` exits 0.

The release cut itself is not part of this validation. Run `dev release` only with `--dry-run` on this branch.

### Review

Round 1 (2026-11-09): effort 3, one `general` reviewer. After this fix pass the live ledger is bug 0 open / 1 fixed, suggestion 0 open / 1 fixed, nit 0. Disposition is not written yet. Review round 2 re-checks the guide.

- M1 (bug, fixed): the overview claimed `dev release` walks back over documentation commits. The guide now limits the walk to `kanban/` and documents the merge-mode dispatch for a docs-only HEAD.
- M2 (suggestion, fixed): the guide now has the 7-day / keep-last-two freshness check, the 30-day rerun limit, and the dispatch-then-break-glass recovery while `master` is still the tagged commit.

The code, `workflow.yml`, and DevCli wiring had no findings. The reviewer reran `dev --help` (`release` is listed), `dev build` (0 warnings), and `dev test` (6 of 6 pass).

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

Round 2 (2026-11-09): effort 2, one `general` reviewer, scope `7059787..0070dea` (guide only). After this fix pass the live ledger is bug 0 open / 2 fixed, suggestion 0 open / 1 fixed, nit 0. Disposition is written on `review/round-2/merged.md`. Review round 3 re-checks the guide.

- M3 (bug, fixed): the pre-cut check used the run guard 8 names. That id is the push run, so a refresh left the operator looping on an expired artifact. The guide now checks every successful non-pull-request run at the dry-run target SHA and accepts any live `Packages-*` artifact.

Paths: `review/round-2/general.md`, `review/round-2/merged.md`.

Round 3 (2026-11-09): effort 1, one `general` reviewer, scope `0070dea..083d7c4` (guide only). After this fix pass the live ledger is bug 0 open / 4 fixed, suggestion 0 open / 1 fixed, nit 0. Disposition is not written. Review round 4 re-checks the guide.

- M3 (bug, fixed): verified against `OrderCandidateRuns` and the dry-run `Target commit:` output.
- M4 (bug, fixed): the refresh no longer repeats the artifact list at the pre-refresh SHA. The next dry-run's `Target commit:` is the SHA to list. It moves to HEAD when HEAD was kanban-only past the old target. The old-push-run sentence applies only when HEAD was already the target. Download names a later merge-mode dispatch on `master` that ran at the commit being released.

Paths: `review/round-3/general.md`, `review/round-3/merged.md`.

## Review

- Round 1: M1 and M2 fixed in `releasing.md`.
- Round 2: M3 fixed in `releasing.md`.
- Round 3: M4 fixed in `releasing.md`. Round 4 has not run.
## Notes

The `bin/dev` AOT on-create failure: `MSB3030: Could not copy the file ".../dev.xml" because it
was not found` (seen on the task 009 and 010 claims). Fix it as part of the move to DevCli.
