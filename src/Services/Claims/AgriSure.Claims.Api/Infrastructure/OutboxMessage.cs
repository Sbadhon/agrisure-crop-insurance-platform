namespace AgriSure.Claims.Api.Infrastructure;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public OutboxMessage(
        Guid id,
        string eventType,
        string routingKey,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        EventType = eventType;
        RoutingKey = routingKey;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string RoutingKey { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error.Length > 1000 ? error[..1000] : error;
    }
}
