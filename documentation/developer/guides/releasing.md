# Releasing TimeWarp.Builder

How this repo cuts and publishes a release. The version is typed once. The tag,
the GitHub Release, and the NuGet push all derive from that value. The release
pipeline promotes the package CI already built. It does not compile again.

## Overview

- **Version SSOT:** `<Version>` in `source/Directory.Build.props`. There is no
  MinVer and no per-project version. The release tag is `v{Version}`.
- **One package.** `TimeWarp.Builder` is the only `IsPackable` project under
  `source/`. It ships `net10.0` and `net11.0` in one nupkg. A project that must
  not ship sets `IsPackable=false`. Do not add a `<Version>` to a csproj.
- **Feed:** nuget.org only. `.timewarp/dev.jsonc` leaves `checkVersionConfig.feeds`
  unset, so `dev check-version` and `dev release` read nuget.org and do not
  call GitHub Packages.
- **CI (push and pull request):** `dev workflow` runs
  `self-install → clean → build → test → pack`. `dev pack` writes
  `artifacts/packages/TimeWarp.Builder.{Version}.nupkg`. A green push to
  `master` uploads that file as `Packages-{run_number}` (`retention-days: 7`)
  and then keeps the two newest `Packages-*` artifacts. Pull requests do not
  upload. A push that changes only kanban or documentation does not run CI.
  `dev release` walks back over those commits to the newest green run.
- **Release:** promote that artifact. The pipeline is
  `tag-gate → check-version → locate-run → download-artifact → verify → push`.
  Push uses the short-lived key from `nuget/login@v1` and `--skip-duplicate`.

## Normal release flow

### 1. Bump the version and merge to master

Open a pull request that changes `<Version>` in `source/Directory.Build.props`.
After it merges, the `push` event runs `workflow.yml` in merge mode and uploads
`Packages-*`. Nothing is published yet.

The props version is 1.1.0. Cutting that release is a separate step, after
.NET 11 is GA. This guide does not perform it.

### 2. Cut the release with `dev release`

From a clean `master` worktree that matches `origin/master`:

```bash
dev release --dry-run
dev release
```

`dev release` reads `<Version>` and runs these guards before it creates anything.
The first failure stops the command. `--dry-run` runs the same guards and prints
the tag and `gh release create` commands without running them.

1. **Props version readable** from `source/Directory.Build.props`.
2. **`gh` available and authenticated** (`gh auth status`).
3. **Working tree clean.**
4. **On master.**
5. **In sync with `origin/master`** after `git fetch origin master`.
6. **Tag `v{Version}` does not exist** locally or on origin. If it does, resume
   from that commit or bump the version. Do not create a second tag.
7. **Publish-state gate** for `TimeWarp.Builder` at the props version. None
   published passes. All published means bump. Partial means an earlier push
   did not finish: resume from the original commit, or bump.
8. **A successful `workflow.yml` run** exists at `HEAD`, or at the nearest
   earlier commit whose changes are kanban-only. The tag is placed on the
   commit that produced the `Packages-*` artifact.

With the guards passed, `dev release` creates the annotated tag `v{Version}`,
pushes it, and runs `gh release create`. Publishing that GitHub Release starts
the promotion pipeline.

`bin/dev` is an AOT snapshot. If `release` is missing from `dev --help`, run
`dev self-install`, or use `dotnet run --file tools/dev-cli/dev.cs -- release`.

### 3. `release: published` promotes the artifact

`workflow.yml` runs `dev workflow --mode release --api-key …`. The six steps:

1. **Release gate.** On a `release` event, `GITHUB_REF_NAME` must be `v{Version}`.
   If the tag exists locally, `HEAD` must be that tag's commit. `HEAD` must be
   an ancestor of `origin/master`. The checkout is `fetch-depth: 0`.
2. **Check version.** `None` and `Partial` continue. `All` stops.
3. **Locate CI run.** `gh run list` for `workflow.yml` at the commit, status
   success. Pull-request runs are ignored. A push run is preferred.
4. **Download artifact.** The first candidate with a live `Packages-*` artifact
   is downloaded into `artifacts/packages/`. An expired artifact is skipped.
   `gh run rerun <run-id>` rebuilds the same commit when every artifact is gone.
5. **Verify.** The downloaded file must be `TimeWarp.Builder.{Version}.nupkg`
   and nothing else.
6. **Push.** `dotnet nuget push` to `https://api.nuget.org/v3/index.json` with
   `--skip-duplicate`. A missing `--api-key` stops the run before the push.

Success prints `Pipeline SUCCEEDED - Packages published to NuGet.org`.

Confirm the package at <https://www.nuget.org/packages/TimeWarp.Builder>. The
flat-container index updates before search does.

## Why promotion, not rebuild

The bytes on nuget.org are the bytes the merge run built, tested, packed, and
uploaded. A second compile at release time could differ when the SDK has moved.

## Break-glass

A `workflow_dispatch` with the default inputs runs merge mode and does not
publish. Publishing requires `mode` set to `release` and `confirm` set to the
word `release`. Any other confirm value fails the run before NuGet login.

Use it to resume a release whose tag already exists: the webhook never fired,
or the promotion run stopped after the tag was pushed. Check out the tagged
commit first. The tag-pin check rejects any other commit. If the source has
changed, bump the version instead.

```bash
gh workflow run workflow.yml --repo TimeWarpEngineering/timewarp-builder -f mode=release -f confirm=release --ref v1.1.0
```

Replace the ref with the tag that already exists. Do not dispatch this against
a commit that was never tagged for that version.

## Trusted publishing

NuGet credentials are not stored as a repository secret. The job has
`id-token: write`. On a `release` event, a confirmed break-glass dispatch, or
a probe dispatch, `nuget/login@v1` runs with `user: TimeWarp.Enterprises`. The
OIDC token becomes `steps.nuget-login.outputs.NUGET_API_KEY`.

The trusted-publishing policy lives on the NuGet.org account, not in this repo:

- Repository owner: `TimeWarpEngineering`
- Repository: `timewarp-builder`
- Workflow file: `workflow.yml`
- Environment: empty

### Probe

```bash
gh workflow run workflow.yml --repo TimeWarpEngineering/timewarp-builder -f mode=probe
```

The workflow file has to be on the default branch before dispatch can see
`probe`. The run exchanges the OIDC token and stops. It does not build or
push. A failure of the NuGet login step means the policy is missing or
misconfigured.
