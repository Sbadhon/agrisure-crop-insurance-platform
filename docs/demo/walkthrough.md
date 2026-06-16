# Walkthrough

## Opening

“AgriSure is an independent crop-insurance workflow project based on public industry processes. I deliberately did not copy a carrier interface or attempt to reproduce proprietary rating logic. I focused on the engineering concerns a lead would own: bounded contexts, workflow correctness, tenant security, integration reliability, operational projections, risks, and delivery sequencing.”

## Five-minute product demonstration

1. Show the bound policy and synthetic field boundaries.
2. Switch to Producer and submit a Notice of Loss.
3. Explain that Claims synchronously verifies policy eligibility before creation.
4. Switch to ClaimsReviewer and assign the adjuster.
5. Switch to Adjuster and record actual production.
6. Switch to ClaimsReviewer and approve; explain the intentionally simplified formula.
7. Request payment, switch to Operations, and mark paid.
8. Open the dashboard and explain eventual consistency.

## Architecture discussion

- Policy and Claims are separate because each owns a coherent lifecycle and source of truth.
- Operations is a read model, not another transactional authority.
- The outbox prevents a committed claim update from losing its corresponding event.
- The consumer inbox prevents duplicate broker delivery from duplicating the projection.
- The tenant is part of every lookup; producer and adjuster checks restrict resource ownership further.
- There are only three domain APIs because service count should follow business ownership, not entity count.

## Failure demonstration

Stop RabbitMQ, perform a valid claim transition, and show that:

- The Claims transaction remains committed.
- The outbox row remains pending.
- The publisher logs retries.
- Restarting RabbitMQ allows the projection to catch up.

Then retry an invalid workflow action and show the HTTP 409 problem response.

## Lead-level tradeoffs to discuss

- Header identity is a clear demo seam, not production security.
- `EnsureCreated` optimizes first-run convenience; migrations are the first production-hardening story.
- GeoJSON keeps the repository portable; PostGIS is the planned spatial-query increment.
- The claim-number generator is intentionally simple and protected by an index; production requires a concurrency-safe allocator.
- A synchronous Policy lookup is acceptable for claim creation because eligibility is required immediately. Portfolio reporting is asynchronous because temporary staleness is acceptable.

## Close

“The most important artifact is not the number of screens. It is the end-to-end reasoning: I can explain where data lives, which failures are recoverable, what must be strongly consistent, what can be eventually consistent, how roles are constrained, and how I would sequence the next four sprints.”
