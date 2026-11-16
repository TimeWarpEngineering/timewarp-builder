# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-builder to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: no `pull_request` paths filter; `dev workflow` does not classify. Work: add the classifier call.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Checklist

- [x] `ganda repo audit --fix` run and committed
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` filter kept with a why comment
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Notes


After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Goal (implementer restatement)

Every pull request in timewarp-builder must report a `CI` check, and the dev CLI (not a YAML `paths`
filter) decides whether that run builds. Concretely:

- `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` pinned at `4.0.0-beta.15`.
- `dev workflow`'s pull-request/merge path calls `PullRequestBuildClassifier.DecideAsync` first, prints its
  message, and returns exit 0 without building when it says skip. Release path unchanged.
- `workflow.yml`: no `paths`/`paths-ignore` on `pull_request` (already true); the `push` filter stays with a
  why comment (already present).
- Packed non-code files the defaults would skip (`readme.md`, `assets/logo.png`) are declared under
  `pullRequestBuild.buildPaths` in `.timewarp/dev.jsonc`, not by editing the rule.
- No local copy of the rule in `tools/dev-cli/`.
- `ganda repo audit --fix` applied and committed; `ganda repo audit` passes, including the ganda 655
  CI-standard checks (`dev-workflow-classifier` was the failing one).

## Results

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.13 → 4.0.0-beta.15. Nuru beta.15
  needs `TimeWarp.Amuru` ≥ 3.0.0-beta.5 and `TimeWarp.Terminal` ≥ 2.0.0-beta.2 (NU1605 downgrade errors), so
  `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` → 3.0.0-beta.5 and `TimeWarp.Terminal` → 2.0.0-beta.2.
- `tools/dev-cli/endpoints/workflow-command.cs`: the pull-request/merge path finds the repo root, calls
  `PullRequestBuildClassifier.DecideAsync(RepoRoot, Ct)`, prints `decision.Message`, and returns (exit 0) when
  `RequiresBuild` is false. Release path unchanged. No local copy of the rule exists in `tools/dev-cli/`.
- `.timewarp/dev.jsonc`: `pullRequestBuild.buildPaths` = `readme.md`, `assets/logo.png`. Both are packed into the
  nupkg, and the defaults would skip them (root markdown, `assets/**`). Other packed and build inputs
  (`Directory.*.props`, `nuget.config`, `.slnx`, `BannedSymbols.txt`, `.editorconfig`) already build under the
  default rule.
- `.github/workflows/workflow.yml`: no change needed. `pull_request` already had no `paths` filter, and the `push`
  filter already carries the why comment (a kanban-only merge must not rebuild).
- `ganda repo audit --fix` added `.githooks/prepare-commit-msg` and `.githooks/prepare-commit-msg.cs`. They are
  committed per policy. Before this change, the audit failed `dev-workflow-classifier`.
- Merged `origin/master` (the `land kanban 016` commit) into the branch.
- CI run URL: the host opens the PR later (open-pr node). The URL goes here once it shows.

### How to validate

Smoke (kanban-only pull request skips):

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
printf '{"pull_request":{"base":{"sha":"bf04120"},"head":{"sha":"28221f5"}}}' > /tmp/ev.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=/tmp/ev.json ./bin/dev workflow; echo "exit=$?"
```

Expect: `Pull request changes only declared non-build paths. Skipping build and test. ... (2 changed paths)` and
`exit=0`, with no self-install, clean, or build steps after it.

Smoke (readme-only pull request builds): make an event whose head is a scratch commit that changes only
`readme.md`. Expect: `Build path changed: readme.md. Full build.`, then `Pipeline: self-install -> clean -> build
-> test -> pack`. This was verified with a `git commit-tree` scratch commit (956c1d0) on 2026-11-16.

Local run (no `GITHUB_EVENT_NAME`): expect `No GITHUB_EVENT_NAME (local run). Full build.` and then the full pipeline.

### Gates (2026-11-16)

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | PASS: 0 warnings, 0 errors |
| Test | `./bin/dev test` | PASS: all 6 test files passed (exit 0) |
| Audit | `ganda repo audit` | PASS: 44 passed, 0 failed, 3 skipped. "Repository passes all audit checks." |

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle (claude-opus-5-5) 2026-11-16: classifier wired, pins bumped, buildPaths declared, gates green.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session e21139dc-2e8c-4193-93a0-ba56c0ad56b0, max-turns 500, Done — 2026-11-16T15:15:16Z
