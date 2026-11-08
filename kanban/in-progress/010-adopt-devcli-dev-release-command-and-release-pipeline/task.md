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

## Checklist

- [x] Study timewarp-ramani's dev CLI, `workflow.yml`, and releasing guide
- [x] Add `TimeWarp.Nuru.DevCli` and wire its commands (keep repo-specific commands that still matter)
- [x] `dev --help` lists `release`; `dev release --dry-run` evaluates the guards read-only
- [x] Update `workflow.yml` to the artifact-promotion release pipeline plus break-glass and probe modes
- [x] Add `documentation/developer/guides/releasing.md`
- [x] `bin/dev` (`dev self-install`) builds. It failed during the worktree on-create on 2026-11-08 with MSB3030, missing `dev.xml`
- [x] Build and tests green, `ganda repo audit` clean
- [x] Do not create a tag or GitHub Release

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-08)
- Implementation: 01a1bcc1-3c09-78e2-8fa0-ceeacadc6e70 (2026-11-09)

## Results

`TimeWarp.Nuru.DevCli` 4.0.0-beta.11 supplies `clean`, `check-version`, `release`, and `self-install`. This repo keeps `build`, `test`, `pack`, `format`, `verify-samples`, and `workflow`. Release mode promotes `Packages-*` and pushes `TimeWarp.Builder` to nuget.org. It does not rebuild. `<Version>` is still 1.1.0. No tag and no GitHub Release were created.

`dev self-install` AOT-publishes because `tools/dev-cli/Directory.Build.props` sets `GenerateDocumentationFile` to false, so the publish no longer looks for `dev.xml`. Amuru stays at 2.0.0: 3.0.0-beta.1 is net11.0 only, and the git hooks restore that package. DevCli 4.0.0-beta.11 compiles against Amuru 2.0.0.

`ganda repo audit` first reported one warning, `vscode-window-icon`, because `.vscode/settings.json` was missing. `ganda repo audit --fix --checks vscode-window-icon` wrote that file. It is gitignored (`.gitignore` line `.vscode/*`). The re-audit reported Passed 38, Failed 0, Skipped 3.

On the clean task branch, `./bin/dev release --dry-run` passed the gh and clean-tree guards and stopped with "release must be cut from master". `git tag -l 'v1.1.0'` was empty. `./bin/dev workflow --mode pr` then ran self-install, clean, build, test, and pack and printed `Pipeline SUCCEEDED`. The packed file is `artifacts/packages/TimeWarp.Builder.1.1.0.nupkg`.

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
```

**Expect:** `release` is in `./bin/dev --help`. `./bin/dev release --dry-run` exits non-zero because this branch is not `master`, prints no `gh release create` execution, and `git tag -l 'v1.1.0'` stays empty. `./bin/dev check-version` prints `Packages checked: TimeWarp.Builder`, `Version in source: 1.1.0`, and `Latest NuGet version: 1.0.0`. `./bin/dev build` is 0 warnings and writes `source/timewarp-builder/bin/Release/net10.0/TimeWarp.Builder.dll` and `source/timewarp-builder/bin/Release/net11.0/TimeWarp.Builder.dll`. All 6 Jaribu runfiles pass. `source/Directory.Build.props` still has `<Version>1.1.0</Version>`.

**Automated gate:** `./bin/dev build` and `./bin/dev test` from the repo root. `ganda repo audit` exits 0.

The release cut itself is not part of this validation. Do not run `dev release` without `--dry-run`.

## Notes

The `bin/dev` AOT on-create failure: `MSB3030: Could not copy the file ".../dev.xml" because it
was not found` (seen on the task 009 and 010 claims). Fix it as part of the move to DevCli.
