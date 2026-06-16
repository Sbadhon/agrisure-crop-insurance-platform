namespace AgriSure.Policy.Api.Domain;

public sealed class InsuredField
{
    private InsuredField() { }

    public InsuredField(
        Guid id,
        Guid policyId,
        string fieldNumber,
        string farmNumber,
        string tractNumber,
        decimal insuredAcres,
        DateOnly plantingDate,
        string geoJson)
    {
        Id = id;
        PolicyId = policyId;
        FieldNumber = fieldNumber;
        FarmNumber = farmNumber;
        TractNumber = tractNumber;
        InsuredAcres = insuredAcres;
        PlantingDate = plantingDate;
        GeoJson = geoJson;
    }

    public Guid Id { get; private set; }
    public Guid PolicyId { get; private set; }
    public string FieldNumber { get; private set; } = string.Empty;
    public string FarmNumber { get; private set; } = string.Empty;
    public string TractNumber { get; private set; } = string.Empty;
    public decimal InsuredAcres { get; private set; }
    public DateOnly PlantingDate { get; private set; }
    public string GeoJson { get; private set; } = string.Empty;
}
