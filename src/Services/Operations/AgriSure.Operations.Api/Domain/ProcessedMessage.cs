namespace AgriSure.Operations.Api.Domain;

public sealed class ProcessedMessage
{
    private ProcessedMessage() { }

    public ProcessedMessage(Guid eventId, string eventType, DateTimeOffset processedAtUtc)
    {
        EventId = eventId;
        EventType = eventType;
        ProcessedAtUtc = processedAtUtc;
    }

    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAtUtc { get; private set; }
}
