namespace AgriSure.Policy.Api.Domain;

public sealed class CropPolicy
{
    private CropPolicy() { }

    public CropPolicy(
        Guid id,
        string tenantId,
        string policyNumber,
        Guid producerId,
        string producerName,
        string crop,
        int cropYear,
        string county,
        string state,
        decimal coverageLevel,
        decimal approvedYield,
        decimal demonstrationPrice,
        DateOnly effectiveDate,
        DateOnly expirationDate)
    {
        Id = id;
        TenantId = tenantId;
        PolicyNumber = policyNumber;
        ProducerId = producerId;
        ProducerName = producerName;
        Crop = crop;
        CropYear = cropYear;
        County = county;
        State = state;
        CoverageLevel = coverageLevel;
        ApprovedYield = approvedYield;
        DemonstrationPrice = demonstrationPrice;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
        Status = "Bound";
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string PolicyNumber { get; private set; } = string.Empty;
    public Guid ProducerId { get; private set; }
    public string ProducerName { get; private set; } = string.Empty;
    public string Crop { get; private set; } = string.Empty;
    public int CropYear { get; private set; }
    public string County { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public decimal CoverageLevel { get; private set; }
    public decimal ApprovedYield { get; private set; }
    public decimal DemonstrationPrice { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public List<InsuredField> Fields { get; private set; } = [];
}
