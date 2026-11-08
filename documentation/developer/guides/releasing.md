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
  upload. A push that changes only `kanban/` does not run CI. `dev release`
  walks back over those kanban-only commits, at most 50, to the newest green
  run and tags that commit. A commit that changes any path outside `kanban/`,
  including `documentation/`, ends the walk. The push filter also skips
  `documentation/**`, so a docs-only HEAD has no run. Step 2 is the recovery.
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

From a clean `master` worktree that matches `origin/master`, start with the
read-only pass. The cut itself is the `dev release` below, after the artifact
check.

```bash
dev release --dry-run
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
   earlier commit whose changes are kanban-only. The walk stops at the first
   commit with a path outside `kanban/`. The tag is placed on the commit that
   produced the `Packages-*` artifact.

Guard 8 does not check that the artifact is still downloadable. `Packages-*`
is kept for 7 days, and each green master push deletes older `Packages-*`
artifacts until two remain. The 1.1.0 cut waits for .NET 11 GA, so the
artifact from the version-bump run will be gone by then. `dev release` can
still pass and create the tag. The promotion run then fails at
download-artifact.

The run id on the guard 8 line is not the artifact to trust. That line prints
the first candidate from `OrderCandidateRuns`, and that order puts a `push`
run ahead of every other event at the commit. After a refresh the line still
names the old push run. Its artifact stays expired, and the run stays older
than 7 days. Release mode does not stop on that run. It lists every
successful `workflow.yml` run at the target commit, drops `pull_request`
runs, and downloads the first candidate whose `Packages-*` artifact is not
expired.

Before `dev release`, copy the full SHA from the dry-run line
`Target commit:` and list the successful runs at that commit:

```bash
gh run list --workflow workflow.yml --commit SHA --status success --json databaseId,event
```

Skip each run whose `event` is `pull_request`. For every other run:

```bash
gh api "repos/TimeWarpEngineering/timewarp-builder/actions/runs/RUN_ID/artifacts" --jq '.artifacts[] | select(.name | startswith("Packages-")) | {name, expired}'
```

Go on when any of those runs has a `Packages-*` row with `expired` false.
Refresh when every list is empty or every `Packages-*` row is expired:

```bash
gh workflow run workflow.yml --ref master -f mode=merge
```

Wait until that run is green, dry-run again, and repeat the list at the same
SHA. The push run that guard 8 names stays older than 7 days. That age is
not a reason to dispatch again once another run at the SHA has a live
artifact.

Use the same command when guard 8 refuses. The refusal starts with "no
successful CI run of workflow.yml" and, when HEAD is not kanban-only, names
the commit and the path outside `kanban/`. A documentation-only merge is the
usual case: the push filter skips `documentation/**`, and there is no run to
rerun. `workflow_dispatch` with `mode=merge` on `master` ignores the push
paths filter. It builds, packs, and uploads `Packages-*` because `github.ref`
is `refs/heads/master`. A pull-request run is not a candidate. Wait for
green, then `dev release --dry-run`, and apply the artifact check above.

With the guards passed, and with a live `Packages-*` artifact on any
successful run at the target commit whose event is not `pull_request`:

```bash
dev release
```

`dev release` creates the annotated tag `v{Version}`, pushes it, and runs
`gh release create`. Publishing that GitHub Release starts the promotion
pipeline.

`bin/dev` is an AOT snapshot. If `release` is missing from `dev --help`, run
`dev self-install`, or use `dotnet run --file tools/dev-cli/dev.cs -- release`.

### 3. `release: published` promotes the artifact

`workflow.yml` runs `dev workflow --mode release --api-key …`. The six steps:

1. **Release gate.** On a `release` event, `GITHUB_REF_NAME` must be `v{Version}`.
   If the tag exists locally, `HEAD` must be that tag's commit. `HEAD` must be
   an ancestor of `origin/master`. The checkout is `fetch-depth: 0`.
2. **Check version.** `None` and `Partial` continue. `All` stops.
3. **Locate CI run.** `gh run list` for `workflow.yml` at the commit, status
   success. Pull-request runs are ignored. A push run is ordered first.
4. **Download artifact.** The first candidate with a live `Packages-*` artifact
   is downloaded into `artifacts/packages/`. An expired artifact is skipped,
   including the preferred push run, so a later merge-mode dispatch at the
   same commit supplies the bytes. When every candidate is expired, or none
   uploaded `Packages-*`, the run stops. `gh run rerun <run-id>` rebuilds
   that same commit. GitHub accepts the rerun for 30 days after the run.
   Past that, use the break-glass recovery below. Leave the package unbuilt
   at release time.
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

Replace the ref with the tag that already exists. Dispatch this against the
commit that was tagged for that version.

When the promotion run stopped because `Packages-*` was expired or missing,
and the CI run is still inside the 30-day window, `gh run rerun <run-id>` on
that CI run rebuilds the same commit. After the new artifact exists, re-run
the failed promotion run, or dispatch break-glass again.

Past 30 days GitHub refuses the rerun. Refresh the artifact when `master`
still points at the tagged commit:

```bash
gh workflow run workflow.yml --ref master -f mode=merge
```

Wait for green, then dispatch break-glass with `--ref` set to the existing
tag. The merge-mode dispatch uses `--ref master`. The upload step runs only
for `refs/heads/master`, so a dispatch whose ref is the tag does not upload.
Locate-run on the break-glass checkout reads runs at the tag's commit, so
the green merge-mode run has to be that same commit.

When `master` has moved past the tag, that dispatch builds a different
commit and leaves the tag's artifact missing. After 30 days there is no
same-commit rebuild. Bump `<Version>` and cut a new release from a fresh
green master run. Leave the old tag in place.

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
