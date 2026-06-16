using System.Text.Json;
using AgriSure.BuildingBlocks.Web;
using AgriSure.Claims.Api.Domain;
using AgriSure.Contracts;

namespace AgriSure.Claims.Api.Infrastructure;

public static class ClaimEventFactory
{
    public static OutboxMessage Create(
        Claim claim,
        string eventType,
        string routingKey,
        string note,
        HttpContext context)
    {
        var payload = new ClaimEventPayload(
            claim.Id,
            claim.PolicyId,
            claim.ClaimNumber,
            claim.PolicyNumber,
            claim.ProducerName,
            claim.Crop,
            claim.County,
            claim.Status.ToString(),
            claim.EstimatedIndemnity,
            note,
            claim.UpdatedAtUtc);

        var envelope = IntegrationEventEnvelope.Create(
            eventType,
            claim.TenantId,
            CorrelationIdMiddleware.Get(context),
            payload,
            claim.UpdatedAtUtc);

        return new OutboxMessage(
            envelope.EventId,
            envelope.EventType,
            routingKey,
            JsonSerializer.Serialize(envelope),
            envelope.OccurredAtUtc);
    }

    public static OutboxMessage CreateForSeed(
        Claim claim,
        string eventType,
        string routingKey,
        string note)
    {
        var payload = new ClaimEventPayload(
            claim.Id,
            claim.PolicyId,
            claim.ClaimNumber,
            claim.PolicyNumber,
            claim.ProducerName,
            claim.Crop,
            claim.County,
            claim.Status.ToString(),
            claim.EstimatedIndemnity,
            note,
            claim.UpdatedAtUtc);

        var envelope = IntegrationEventEnvelope.Create(
            eventType,
            claim.TenantId,
            "seed-data",
            payload,
            claim.UpdatedAtUtc);

        return new OutboxMessage(
            envelope.EventId,
            envelope.EventType,
            routingKey,
            JsonSerializer.Serialize(envelope),
            envelope.OccurredAtUtc);
    }
}
