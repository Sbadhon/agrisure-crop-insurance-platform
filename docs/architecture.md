# Architecture

## Quality attributes

AgriSure prioritizes the following qualities for the first production-shaped vertical slice:

1. **Workflow correctness** — invalid state transitions are rejected inside the domain aggregate.
2. **Auditability** — every transition records actor, role, timestamp, status, and explanatory note.
3. **Tenant isolation** — all reads and writes are scoped by tenant before domain data is returned.
4. **Delivery reliability** — domain changes and outgoing events are committed together through the transactional outbox.
5. **Consumer idempotency** — the Operations service persists processed event IDs before acknowledging delivery.
6. **Operational visibility** — correlation IDs, health endpoints, structured logs, and OpenTelemetry hooks are present.
7. **Evolvability** — Policy, Claims, and Operations own their data and communicate through explicit APIs or versioned events.

## System context

The system supports five primary personas:

- Producer: views coverage and reports a loss.
- Agent: manages producer-facing policy and loss intake activities.
- Claims reviewer: assigns work, evaluates completed inspections, and requests settlement.
- Adjuster: records production evidence and field findings.
- Operations specialist: monitors the portfolio and confirms simulated downstream payment.

External systems are represented by boundaries rather than fabricated integrations:

- Identity provider
- Regulatory and actuarial reference-data provider
- Document storage and malware scanning
- Payment platform
- Acreage and policy-submission gateways

## Containers

### React application

Provides a role-aware demonstration surface. It does not contain business rules. It calls APIs through the gateway and renders permitted actions based on the selected demonstration actor.

### YARP gateway

Routes stable public paths to the three APIs. In production it would also terminate external authentication, apply rate limits, and forward validated identity claims.

### Policy API

Owns producer, policy, coverage, crop, and insured-field data. It exposes a narrow eligibility endpoint for the Claims service. The implementation stores valid GeoJSON to keep the demo lightweight; a production increment would add PostGIS geometry columns and spatial indexes.

### Claims API

Owns the claim aggregate and its timeline. It is the transactional source of truth for the loss-to-payment workflow. Each transition writes a versioned integration-event envelope into its outbox.

### Operations API

Consumes claim events and maintains a read model optimized for dashboard queries. The `processed_messages` table is the inbox/idempotency ledger.

## Data ownership

No service reads another service's database. The Claims service performs a synchronous policy-eligibility lookup because the decision must be confirmed before claim creation. All subsequent portfolio reporting is asynchronous.

| Data | Owner |
|---|---|
| Producer, policy, crop coverage, field | Policy API |
| Claim, inspection, settlement status, timeline | Claims API |
| Portfolio claim projection, processed event IDs | Operations API |

## Event flow

1. The Claims API changes the aggregate and appends an outbox message in one database transaction.
2. The outbox publisher reads unprocessed messages in order.
3. RabbitMQ publisher confirms protect against silent broker loss.
4. The Operations consumer receives a topic event.
5. The consumer checks `processed_messages` for the event ID.
6. It updates the read model and records the event ID in one database transaction.
7. It acknowledges the RabbitMQ delivery only after the database commit succeeds.
8. A malformed or repeatedly failing event is negatively acknowledged without requeue and reaches the dead-letter exchange.

## Failure behavior

| Failure | Behavior |
|---|---|
| RabbitMQ unavailable | Claim transaction succeeds; outbox message remains pending and is retried |
| Operations API unavailable | Broker retains durable message until consumer returns |
| Duplicate event delivery | Consumer detects the event ID and acknowledges without reapplying |
| Projection database failure | Delivery is not acknowledged; broker redelivers |
| Invalid workflow action | Domain throws; API returns HTTP 409 with a problem response |
| Unauthorized role | API returns HTTP 403 before changing state |
| Wrong tenant or hidden record | Query does not return the aggregate |

## Production hardening path

- Replace `EnsureCreated` with versioned EF Core migrations.
- Add OIDC/JWT validation and stop accepting identity headers from public callers.
- Use a managed secret store and workload identity.
- Add optimistic concurrency to claim aggregates.
- Add a globally safe claim-number generator.
- Add outbox row locking/leases for multiple publisher replicas.
- Add document object storage, virus scanning, retention, and legal-hold policy.
- Add PostGIS geometry, spatial validation, and map-tile strategy.
- Add automated reconciliation between Claims and Operations projections.
- Add rate limiting, audit export, PII classification, encryption policy, and disaster-recovery objectives.
