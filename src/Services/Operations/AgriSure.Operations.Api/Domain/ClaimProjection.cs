namespace AgriSure.Operations.Api.Domain;

public sealed class ClaimProjection
{
    private ClaimProjection() { }

    public ClaimProjection(
        Guid claimId,
        string tenantId,
        Guid policyId,
        string claimNumber,
        string policyNumber,
        string producerName,
        string crop,
        string county,
        string status,
        decimal? estimatedIndemnity,
        string lastNote,
        DateTimeOffset updatedAtUtc)
    {
        ClaimId = claimId;
        TenantId = tenantId;
        PolicyId = policyId;
        ClaimNumber = claimNumber;
        PolicyNumber = policyNumber;
        ProducerName = producerName;
        Crop = crop;
        County = county;
        Status = status;
        EstimatedIndemnity = estimatedIndemnity;
        LastNote = lastNote;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid ClaimId { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public Guid PolicyId { get; private set; }
    public string ClaimNumber { get; private set; } = string.Empty;
    public string PolicyNumber { get; private set; } = string.Empty;
    public string ProducerName { get; private set; } = string.Empty;
    public string Crop { get; private set; } = string.Empty;
    public string County { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public decimal? EstimatedIndemnity { get; private set; }
    public string LastNote { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Apply(
        string status,
        decimal? estimatedIndemnity,
        string lastNote,
        DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc < UpdatedAtUtc)
        {
            return;
        }

        Status = status;
        EstimatedIndemnity = estimatedIndemnity;
        LastNote = lastNote;
        UpdatedAtUtc = updatedAtUtc;
    }
}
