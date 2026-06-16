namespace AgriSure.Contracts;

public sealed record ClaimEventPayload(
    Guid ClaimId,
    Guid PolicyId,
    string ClaimNumber,
    string PolicyNumber,
    string ProducerName,
    string Crop,
    string County,
    string Status,
    decimal? EstimatedIndemnity,
    string Note,
    DateTimeOffset UpdatedAtUtc);
