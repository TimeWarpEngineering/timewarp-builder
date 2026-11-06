# Multi-target net10.0 and net11.0 and clear repo audit

## Description

TimeWarp.Builder currently targets only net10.0. Ship the library for net10.0 and net11.0. The open journal-ignore task 006 is obsolete (not relevant) and shares its id with the done trusted-publishing kitchen, so archive that open kitchen as 008. Clear `ganda repo audit` errors in this same task.

## Checklist

- [ ] Library TargetFrameworks is net10.0;net11.0
- [ ] Dev CLI and test runfiles stay single-TFM so they still run
- [x] Obsolete to-do 006 journal kitchen archived as 008
- [x] Done 006 trusted-publishing kitchen unchanged
- [ ] ganda repo audit has no Error
- [ ] Library builds for net10.0 and net11.0
- [ ] Tests pass

## Session

- Created: 01a1b27c-6184-7133-a255-0dd9b04df0a0 (2026-11-06)
- 2026-11-07: user asked to archive 006 as not relevant, fix the audit, and multi-target net10 and net11 in this task. Session id 01a1b27c-6184-7133-a255-0dd9b04df0a0. The blank 007 kitchen was created only because id 006 was ambiguous.
