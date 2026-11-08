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

## Checklist

- [ ] Study timewarp-ramani's dev CLI, `workflow.yml`, and releasing guide
- [ ] Add `TimeWarp.Nuru.DevCli` and wire its commands (keep repo-specific commands that still matter)
- [ ] `dev --help` lists `release`; `dev release --dry-run` evaluates the guards read-only
- [ ] Update `workflow.yml` to the artifact-promotion release pipeline plus break-glass and probe modes
- [ ] Add `documentation/developer/guides/releasing.md`
- [ ] `bin/dev` (`dev self-install`) builds. It failed during the worktree on-create on 2026-11-08 with MSB3030, missing `dev.xml`
- [ ] Build and tests green, `ganda repo audit` clean
- [ ] Do not create a tag or GitHub Release

## Session

- Created: 41bdc9d0-5983-4556-aa9f-6c1c2c91aa43 (2026-11-08)

## Notes

The `bin/dev` AOT on-create failure: `MSB3030: Could not copy the file ".../dev.xml" because it
was not found` (seen on the task 009 and 010 claims). Fix it as part of the move to DevCli.
