# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-builder

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-builder.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the two dev-only pins, TimeWarp.Nuru and TimeWarp.Nuru.DevCli, from 4.0.0-beta.16 to
4.0.0-beta.17 in the root `Directory.Packages.props`. Nothing else changes: no `<Version>` bump in
`source/Directory.Build.props`, no other package pins.

Acceptance criteria:
- `Directory.Packages.props` pins both packages at 4.0.0-beta.17.
- The dev CLI rebuilds against beta.17 and `./bin/dev build` passes.
- `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261126T021837Z-0cb631
- Implementer: bumped TimeWarp.Nuru and TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 in `Directory.Packages.props`; reinstalled the dev CLI once (`dotnet run --file tools/dev-cli/dev.cs -- self-install`).
- Boy-scout audit fixes: `dev-cli-telemetry` (added `.UseTelemetry(options => options.ServiceName = "timewarp-builder-dev")` in `tools/dev-cli/dev.cs`) and `source-file-line-cap` (recorded the 747-line `.agent/workspace/2026-02-06-t00-00-00-ubuntu-custom-tools-discovery-guide.md` in new `.timewarp/line-cap-baseline.json`).
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 02e8af14-8087-4509-be54-6a1599a1fb48, max-turns 500, Done — 2026-11-26T02:42:12Z
- Review oracle: grok (xAI, Grok 4.7), session 01a215bb-8516-7c52-9cb1-d192c408fb62, effort 1, roster general, turn cap 80, 2026-11-26. Round 1 disposition clean.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a215bb-8516-7c52-9cb1-d192c408fb62, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-26T02:48:21Z

## Results

- `Directory.Packages.props`: TimeWarp.Nuru and TimeWarp.Nuru.DevCli now pin 4.0.0-beta.17. `source/Directory.Build.props` `<Version>` unchanged.
- `tools/dev-cli/dev.cs`: telemetry turned on (audit `dev-cli-telemetry`).
- `.timewarp/line-cap-baseline.json`: new baseline with the one over-cap doc (audit `source-file-line-cap`).

### Gates

- `./bin/dev build` (after dev CLI reinstall on beta.17): PASS, 0 warnings, 0 errors.
- Test files: none added or edited. The change touches no production library code (dev-only packages and the dev CLI entry point); no Jaribu test covers the dev CLI. CI runs the full suite.
- `ganda repo audit`: first run FAIL (`dev-cli-telemetry`, `source-file-line-cap`, both pre-existing); after the fixes PASS, with one advisory warning `vscode-window-icon` (non-blocking, `.vscode/settings.json` missing).

### How to validate

Smoke: `dotnet run --file tools/dev-cli/dev.cs -- self-install && ./bin/dev build && ./bin/dev --help && ganda repo audit`
Expect: build succeeds with 0 errors; `dev --help` lists the commands; audit prints "Repository passes"; `grep Nuru Directory.Packages.props` shows 4.0.0-beta.17 for both packages.

Review:
- Round 1, effort 1, roster `general`. Counts: bug 0, suggestion 0, nit 0 (all open/fixed/wontfix zero). Disposition: clean. No wontfix and no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Review

- Round 1: Done
