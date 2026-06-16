# ADR 002: Keep demonstration identity outside the domain

- Status: Accepted for portfolio environment
- Date: 2026-06-14

## Context

The project needs multiple personas and tenant-aware authorization, but implementing an identity server would distract from the crop-insurance workflow. Hard-coding authorization into the frontend would not protect APIs.

## Decision

The UI sends synthetic actor headers. Every API independently resolves the actor and enforces tenant and role rules. The actor model is isolated in `AgriSure.BuildingBlocks.Identity` so it can be replaced without changing domain aggregates.

## Production replacement

The gateway would validate OIDC/JWT tokens, remove any inbound identity headers, and forward signed or trusted claims. APIs would use standard ASP.NET Core authentication and authorization policies. Producer-resource ownership and assigned-adjuster checks would remain in application/domain logic.

## Consequences

- The demonstration remains immediately runnable.
- API authorization can be exercised across roles.
- The README clearly states that the header mechanism is not production authentication.
