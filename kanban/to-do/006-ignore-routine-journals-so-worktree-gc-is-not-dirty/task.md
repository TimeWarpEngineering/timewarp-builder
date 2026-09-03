# Ignore routine journals so worktree gc is not dirty

## Description

`ganda task work` writes `task-work.journal.json` beside the kitchen. Unless
root `.gitignore` lists that basename, `git status --porcelain` shows `??`
and `ganda pr merge` / `worktree gc` **refuses** a dirty worktree.

This is a **consumer sweep**. Ganda **262** added audit check
`routine-journals-gitignore` and `--fix`, then left “sweep every org repo”
out of scope. That was wrong: we have hit this on merge at least six times
(Taratibu 252/253/254, mediator 004-001/004-002, architecture 207/208,
timewarp-software **033**). Each origin that never ran `--fix` is another
dirty-gc.

This origin (`timewarp-builder`) is missing the ignore. Org SSOT: `ganda repo audit`
check `routine-journals-gitignore`. `--fix` appends the missing basename
lines. Tracked journals are **Failed / not fixable** — `git rm --cached`
is required (gitignore does not hide tracked files).

Do **not** commit journal contents.

## Requirements

Root `.gitignore` must contain these exact basename lines (comments/blanks ok):

```
task-work.journal.json
stacked-task-set.journal.json
planning.journal.json
rfc.journal.json
debate.journal.json
advisor.journal.json
```

Prefer `ganda repo audit --fix --checks routine-journals-gitignore` (this
CLI requires `--fix` when `--checks` is set) so the commented block matches
other origins.

Then `git rm --cached` any tracked `*.journal.json`. `git ls-files '*.journal.json'`
must be empty. Audit check PASSes. Do not implement on `master`.

## Checklist

- [ ] Root `.gitignore` has the six routine-journal basenames
- [ ] `git ls-files '*.journal.json'` is empty
- [ ] Audit `routine-journals-gitignore` PASSes
- [ ] `git check-ignore -v` confirms ignore; porcelain does not list journals
- [ ] Do not implement on `master`

## Notes

- Predecessor: ganda 262; consumer precedent architecture 208, software 034
- 262 out-of-scope (“do not sweep every org repo”) is why this kitchen exists.

## Session

- Created: grok `01a06304-cbf6-7d83-b5a2-4a99e9d09d40` (2026-09-03) cockpit timewarp-flow
- Trigger: `/tw-merge` software 033 dirty-gc; 262 left consumer sweep out of scope
