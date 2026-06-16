# Contributing

1. Create a branch from `main`.
2. Keep domain behavior inside the owning service.
3. Do not add a shared abstraction until at least two concrete callers need the same technical behavior.
4. Add or update tests for every workflow rule.
5. Add an ADR for changes to service ownership, data ownership, messaging guarantees, identity, or deployment architecture.
6. Run `make build`, `make test`, and `make web-build` before opening a pull request.

Use conventional, imperative commits such as:

```text
Add claim concurrency protection
Implement acreage report submission
Document outbox leasing decision
```
