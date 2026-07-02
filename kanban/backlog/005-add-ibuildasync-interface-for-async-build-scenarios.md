# Add IBuildAsync interface for async build scenarios

## Description

Add an `IBuildAsync<TBuilt>` interface for builders whose build step is inherently asynchronous
(for example, builders that resolve external resources or perform I/O when producing the object).

Split out from task 001 (v1.0.0 release blockers) as a post-1.0 feature for a future v1.x.

## Requirements

- Design the shape: likely `ValueTask<TBuilt> BuildAsync(CancellationToken ct = default)` — note
  that `out TBuilt` covariance (as on `IBuilder<TBuilt>`) does not compose with `ValueTask<TBuilt>`
  returns, so the variance story needs a deliberate decision
- Decide whether a nested counterpart (`INestedBuilderAsync<TParent>` / `DoneAsync()`) is warranted
- Keep the library AOT/trim clean — no reflection
- Non-breaking addition; no changes to existing interfaces

## Checklist

- [ ] Survey TimeWarp.Nuru / TimeWarp.Terminal for concrete async-build needs before committing to a shape
- [ ] Design the interface (variance, ValueTask vs Task, CancellationToken)
- [ ] Implement with XML docs and Purpose/Design regions
- [ ] Add Jaribu tests under tests/
- [ ] Update readme

## Notes

Deferred from the v1.0.0 release (task 001, shipped 2026-07-02) because no current consumer
needed it; adding it later is non-breaking.

## Session

- Created: 2026-07-02 (split from task 001 during the v1.0.0 release session)
