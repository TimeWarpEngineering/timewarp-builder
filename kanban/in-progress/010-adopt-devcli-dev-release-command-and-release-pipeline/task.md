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
master that matches origin. This task wires that path. It does not cut the 1.1.0
release, and it does not change `<Version>`.

Done means:

- `clean`, `check-version`, `release`, and `self-install` come from TimeWarp.Nuru.DevCli
  4.0.0-beta.11. `build`, `test`, `pack`, `format`, `verify-samples`, and `workflow`
  stay in this repo.
- `dev --help` lists `release`. `dev release --dry-run` runs the DevCli guards and
  creates no tag and no GitHub Release.
- `workflow.yml` promotes the CI `Packages-*` artifact. Release mode is tag-gate,
  check-version, locate-run, download-artifact, verify, then push to nuget.org with
  the short-lived OIDC key. The package is not rebuilt at release time.
- `workflow_dispatch` accepts `merge`, `release`, and `probe`. `mode=release` publishes
  only when `confirm=release`. `mode=probe` only exchanges the OIDC token.
- `documentation/developer/guides/releasing.md` is enough for an operator to cut a
  release and to resume one that stopped partway.
- `dev self-install` writes `bin/dev`. The AOT publish no longer fails with MSB3030
  because `dev.xml` is missing.
- The library build, the Jaribu runfiles, and `ganda repo audit` pass.

The shape comes from timewarp-ramani (DevCli registration, the six promotion steps,
the guide). The push is nuget.org only, as in timewarp-amuru, because TimeWarp.Builder
has no GitHub Packages feed. The workflow still installs the .NET 10 SDK and the
.NET 11 preview SDK: the library multi-targets, and the dev CLI targets net11.0.

Review round 1 left the DevCli wiring and `workflow.yml` alone. This pass makes
`documentation/developer/guides/releasing.md` match DevCli 4.0.0-beta.11 and the
workflow's artifact rules:

- The walk-back is kanban-only. A commit that changes any other path, including
  `documentation/`, ends the walk. A docs-only master HEAD has no CI run, because
  the push filter skips `documentation/**`.
- That refusal is recovered with `gh workflow run workflow.yml --ref master -f mode=merge`.
  After that run is green, `dev release --dry-run` is run again.
- Before a cut, the operator checks that the run named by guard 8 still has a live
  `Packages-*` artifact. A run older than 7 days, or one removed by keep-last-two,
  is refreshed with the same merge-mode dispatch.
- `gh run rerun` is a 30-day window. Past that, while `master` still points at the
  tagged commit, the recovery is the merge-mode dispatch on `master`, then
  break-glass with `--ref` set to the existing tag.

## Checklist

- [x] Study timewarp-ramani's dev CLI, `workflow.yml`, and releasing guide
- [x] Add `TimeWarp.Nuru.DevCli` and wire its commands (keep repo-specific commands that still matter)
- [x] `dev --help` lists `release`; `dev release --dry-run` evaluates the guards read-only
- [x] Update `workflow.yml` to the artifact-promotion release pipeline plus break-glass and probe modes
- [x] Add `documentation/developer/guides/releasing.md`
- [x] Correct the guide: kanban-only walk-back, docs-only HEAD recovery, and expired-artifact recovery (review M1, M2)
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

## Results

`TimeWarp.Nuru.DevCli` 4.0.0-beta.11 supplies `clean`, `check-version`, `release`, and `self-install`. This repo keeps `build`, `test`, `pack`, `format`, `verify-samples`, and `workflow`. Release mode promotes `Packages-*` and pushes `TimeWarp.Builder` to nuget.org. It does not rebuild. `<Version>` is still 1.1.0. No tag and no GitHub Release were created.

`dev self-install` AOT-publishes because `tools/dev-cli/Directory.Build.props` sets `GenerateDocumentationFile` to false, so the publish no longer looks for `dev.xml`. Amuru stays at 2.0.0: 3.0.0-beta.1 is net11.0 only, and the git hooks restore that package. DevCli 4.0.0-beta.11 compiles against Amuru 2.0.0.

`ganda repo audit` first reported one warning, `vscode-window-icon`, because `.vscode/settings.json` was missing. `ganda repo audit --fix --checks vscode-window-icon` wrote that file. It is gitignored (`.gitignore` line `.vscode/*`). The re-audit reported Passed 38, Failed 0, Skipped 3.

On the clean task branch, `./bin/dev release --dry-run` passed the gh and clean-tree guards and stopped with "release must be cut from master". `git tag -l 'v1.1.0'` was empty. `./bin/dev workflow --mode pr` then ran self-install, clean, build, test, and pack and printed `Pipeline SUCCEEDED`. The packed file is `artifacts/packages/TimeWarp.Builder.1.1.0.nupkg`.

Review round 1's open findings were both in `documentation/developer/guides/releasing.md`. The guide now says `dev release` walks back only over `kanban/` (at most 50 commits). A docs-only HEAD is recovered with `gh workflow run workflow.yml --ref master -f mode=merge`, then another dry-run after that run is green. Step 2 checks the guard 8 run's `Packages-*` artifact before the cut and refreshes it the same way when it is missing, expired, or older than 7 days. Download and Break-glass state that `gh run rerun` lasts 30 days. Past that, while `master` is still the tagged commit, the recovery is that merge-mode dispatch (the upload step requires `refs/heads/master`) and then break-glass on the tag. Round 1 `merged.md` marks M1 and M2 fixed. Disposition waits for review round 2.

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
grep -n -E 'kanban-only|mode=merge|30 days|Packages-' documentation/developer/guides/releasing.md
```

**Expect:** `release` is in `./bin/dev --help`. `./bin/dev release --dry-run` exits non-zero because this branch is not `master`, prints no `gh release create` execution, and `git tag -l 'v1.1.0'` stays empty. `./bin/dev check-version` prints `Packages checked: TimeWarp.Builder`, `Version in source: 1.1.0`, and `Latest NuGet version: 1.0.0`. `./bin/dev build` is 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. `source/Directory.Build.props` still has `<Version>1.1.0</Version>`. The guide's walk-back sentence names kanban-only commits and says a path outside `kanban/`, including `documentation/`, ends the walk. It contains `gh workflow run workflow.yml --ref master -f mode=merge` both as the guard 8 recovery and as the pre-cut refresh when the artifact is missing, expired, or older than 7 days. It says GitHub accepts `gh run rerun` for 30 days, and the break-glass section then dispatches merge mode on `master` while `master` is still the tagged commit.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root. `ganda repo audit` exits 0.

The release cut itself is not part of this validation. Run `dev release` only with `--dry-run` on this branch.

### Review

Round 1 (2026-11-09): effort 3, one `general` reviewer. After this fix pass the live ledger is bug 0 open / 1 fixed, suggestion 0 open / 1 fixed, nit 0. Disposition is not written yet. Review round 2 re-checks the guide.

- M1 (bug, fixed): the overview claimed `dev release` walks back over documentation commits. The guide now limits the walk to `kanban/` and documents the merge-mode dispatch for a docs-only HEAD.
- M2 (suggestion, fixed): the guide now has the 7-day / keep-last-two freshness check, the 30-day rerun limit, and the dispatch-then-break-glass recovery while `master` is still the tagged commit.

The code, `workflow.yml`, and DevCli wiring had no findings. The reviewer reran `dev --help` (`release` is listed), `dev build` (0 warnings), and `dev test` (6 of 6 pass).

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Review

- Round 1: M1 and M2 fixed in `releasing.md`. Round 2 has not run.

## Notes

The `bin/dev` AOT on-create failure: `MSB3030: Could not copy the file ".../dev.xml" because it
was not found` (seen on the task 009 and 010 claims). Fix it as part of the move to DevCli.
