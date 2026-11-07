# Review framework

## Budget (by-diff)

- Lines changed: 466
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 007

**Date:** 2026-11-07
**Host task:** kanban/in-progress/007-multi-target-net10-and-net11/
**Diff scope:** branch `task/007-ignore-routine-journals-so-worktree-gc-is-not-dirt` against the merge base with `master` (`git merge-base master HEAD`). Master is 8 commits ahead, and those commits are kanban landings only.
**Plan / brief:** multi-target the library on net10.0 and net11.0. Keep the dev CLI and the Jaribu runfiles on one target framework. Archive the obsolete 006 journal kitchen as 008. Clear the `ganda repo audit` errors. Bump the version to 1.1.0.
**Effort:** 2 (by-diff budget). The roster is general only.
**Reviewer roster:** general
**Session IDs:** review oracle Claude Opus 5.5 (claude-opus-5-5). This headless run did not expose a session id.

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Address the diff and the call sites around it. Re-verify falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
- Audit `--fix` output is policy baseline. It is not a finding unless it breaks a build or a test.
