namespace AgriSure.Claims.Api.Domain;

public sealed class ClaimTimelineEntry
{
    private ClaimTimelineEntry() { }

    public ClaimTimelineEntry(
        Guid id,
        Guid claimId,
        ClaimStatus status,
        string note,
        string actorName,
        string actorRole,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        ClaimId = claimId;
        Status = status;
        Note = note;
        ActorName = actorName;
        ActorRole = actorRole;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ClaimId { get; private set; }
    public ClaimStatus Status { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public string ActorName { get; private set; } = string.Empty;
    public string ActorRole { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
