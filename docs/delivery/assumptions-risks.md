# Assumptions and risk register

## Assumptions

- The first increment uses one synthetic Minnesota corn policy and two fields.
- Rating, regulatory submission, payment, document, and weather providers are outside the implemented boundary.
- Actual production is entered as total bushels for the insured field.
- Tenant membership and user identity are supplied by a trusted upstream identity boundary in production.
- Event ordering is naturally preserved for one aggregate in the demo but is not guaranteed across partitions in a scaled system.

## Risks

| Risk | Impact | Current control | Production mitigation |
|---|---|---|---|
| Federal rules vary by crop year | Incorrect decisions | No official rule claim; calculation labeled demonstration | Version rules and reference data by crop year with traceable source metadata |
| Duplicate message delivery | Duplicate updates | Processed-event ledger | Retain inbox records according to replay and compliance policy |
| Event arrives out of order | Projection regression | Updated timestamp is stored | Add aggregate version and reject stale versions |
| Claim-number race | Duplicate number under concurrency | Unique database index | Use database sequence or dedicated number allocator |
| Multiple outbox workers publish same row | Duplicate delivery | Idempotent consumer | Add `FOR UPDATE SKIP LOCKED` or lease columns |
| Identity headers spoofed | Unauthorized access | Explicit demo warning | Validate OIDC at gateway; strip external identity headers |
| Tenant filter omitted in future query | Cross-tenant exposure | Tenant checks in every endpoint | Global query filter plus architecture/integration tests and database RLS evaluation |
| GeoJSON is malformed or huge | Mapping and performance problems | Synthetic seed only | Validate geometry, use PostGIS, spatial indexes, size limits, simplification |
| Uploaded evidence contains malware | Security incident | Evidence upload not implemented | Object storage quarantine and malware scanning |
| External service outage | Workflow delay | Outbox retries for events | Timeouts, circuit breaker, durable commands, reconciliation queue |
| `EnsureCreated` cannot evolve schemas | Deployment failure | Portfolio-only bootstrap | EF migrations with backward-compatible rollout |
| Sensitive insurance data logged | Privacy incident | Synthetic data only | Data classification, structured-log allowlist, redaction, access audit |
