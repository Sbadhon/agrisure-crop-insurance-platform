# Implementation plan and estimated backlog

Estimates are relative engineering effort for one experienced full-stack engineer and are intended to demonstrate decomposition, not to promise calendar delivery.

## Completed vertical slice

| Capability | Estimate | Result |
|---|---:|---|
| Repository, service boundaries, Compose | M | Complete |
| Synthetic policy and field read model | M | Complete |
| Role- and tenant-aware API access | M | Complete |
| Notice of Loss and claim aggregate | M | Complete |
| Assignment and adjuster inspection | M | Complete |
| Approval and demonstration settlement | M | Complete |
| Transactional outbox publisher | M | Complete |
| RabbitMQ idempotent projection | M | Complete |
| React multi-persona workflow | L | Complete |
| Claim-state unit tests | S | Complete |
| ADRs, risks, and panel walkthrough | M | Complete |

## Next release: production-shaped foundation

| Item | Size | Acceptance target |
|---|---:|---|
| EF Core migrations | M | Clean install and rolling upgrade tested |
| Real OIDC/JWT | L | Gateway and APIs reject spoofed identity headers |
| Optimistic concurrency | M | Concurrent transition returns a clear conflict |
| Outbox leasing and retention | M | Multiple publisher replicas do not contend unsafely |
| Integration tests with Testcontainers | L | PostgreSQL and RabbitMQ workflow runs in CI |
| OpenTelemetry collector/dashboard | M | One trace follows gateway → Claims → Policy |
| Projection reconciliation | M | Missing/stale projections are detected and repairable |

## Domain expansion

| Item | Size | Key dependency |
|---|---:|---|
| Quote/application workflow | XL | Versioned reference data and coverage model |
| Underwriting work queue | L | Application state machine |
| Acreage reporting | XL | PostGIS and external submission contract |
| Document generation/storage | L | Object store and malware scanning |
| Weather-risk alerts | L | Weather provider and notification preferences |
| Billing and premium statements | XL | Accounting/payment boundary |
| Livestock quote workflow | XL | Separate product rules and domain discovery |

## Suggested sprint slicing

### Sprint 1

- Migrations
- Testcontainers policy/claims integration test
- Claim concurrency token
- CI database test

### Sprint 2

- OIDC integration
- Gateway claim forwarding
- Authorization policies
- Security tests

### Sprint 3

- PostGIS field geometry
- Spatial validation/indexing
- Field-editing UI
- Performance baseline

### Sprint 4

- Acreage report aggregate
- Submission simulator
- Reconciliation queue
- Operational metrics
