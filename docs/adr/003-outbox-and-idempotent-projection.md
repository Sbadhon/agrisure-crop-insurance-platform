# ADR 003: Use a transactional outbox and idempotent consumer

- Status: Accepted
- Date: 2026-06-14

## Context

Publishing directly to RabbitMQ after a database commit can lose an event if the process stops between the two operations. Publishing before the commit can expose an event for a transaction that later fails. Brokers may also redeliver messages.

## Decision

- Persist each event envelope in the Claims database within the aggregate transaction.
- Publish pending outbox rows asynchronously with publisher confirmations enabled.
- Mark an outbox row processed only after successful publication.
- Store every consumed event ID in Operations.
- Update the projection and processed-message ledger in the same transaction.
- Acknowledge delivery only after the projection transaction commits.

## Consequences

The system provides at-least-once delivery with effectively-once projection updates. The first release polls the outbox every two seconds. A production version would add leasing, batched locking, retention, metrics, and replay tooling.
