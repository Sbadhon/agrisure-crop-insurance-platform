# ADR 001: Use three meaningful service boundaries

- Status: Accepted
- Date: 2026-06-14

## Context

The target domain includes policies, fields, loss intake, adjustment, settlement, dashboards, regulatory integrations, documents, and notifications. Splitting every noun into a service would create deployment and integration complexity without independent business ownership.

## Decision

Use three deployable APIs:

1. Policy: producer, policy, coverage, and insured-field authority.
2. Claims: loss, assignment, inspection, approval, and settlement authority.
3. Operations: asynchronous, read-optimized portfolio projection.

The gateway and React application are delivery components rather than domain services.

## Consequences

- The system demonstrates real data ownership without distributed-monolith boilerplate.
- Claims performs one synchronous eligibility call to Policy before creating a claim.
- Reporting remains available without joining transactional databases.
- Future quoting or underwriting capabilities begin as modules inside Policy and are extracted only when ownership, scale, or release cadence justifies it.
