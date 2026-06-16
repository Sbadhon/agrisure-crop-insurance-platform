# Definition of Done

A production-facing increment is done only when:

- Business acceptance criteria and invalid transitions are documented.
- Tenant, role, and resource-ownership rules are enforced by the API.
- Database changes use reviewed migrations and rollback/forward-fix guidance.
- APIs expose problem responses and do not leak internal exceptions.
- Domain behavior has unit tests; persistence and messaging have integration tests.
- Events have stable names, schema versions, correlation IDs, and compatibility review.
- Consumers are idempotent and failure paths reach a monitored retry or dead-letter mechanism.
- Logs, traces, metrics, and health behavior are defined.
- PII and secrets are not logged or committed.
- Performance impact is measured for high-volume queries.
- ADRs and runbooks are updated.
- CI passes from a clean checkout.
- The feature is demonstrable through an end-to-end user workflow.
