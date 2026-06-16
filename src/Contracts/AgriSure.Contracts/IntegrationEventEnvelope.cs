using System.Text.Json;

namespace AgriSure.Contracts;

public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    string TenantId,
    string CorrelationId,
    DateTimeOffset OccurredAtUtc,
    JsonElement Payload)
{
    public static IntegrationEventEnvelope Create<TPayload>(
        string eventType,
        string tenantId,
        string correlationId,
        TPayload payload,
        DateTimeOffset? occurredAtUtc = null)
    {
        return new IntegrationEventEnvelope(
            Guid.NewGuid(),
            eventType,
            1,
            tenantId,
            correlationId,
            occurredAtUtc ?? DateTimeOffset.UtcNow,
            JsonSerializer.SerializeToElement(payload));
    }
}
