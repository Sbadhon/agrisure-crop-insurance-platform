namespace AgriSure.Claims.Api.Domain;

public sealed class Claim
{
    private Claim() { }

    private Claim(
        Guid id,
        string tenantId,
        string claimNumber,
        Guid policyId,
        string policyNumber,
        Guid producerId,
        string producerActorId,
        string producerName,
        string crop,
        string county,
        Guid fieldId,
        string fieldNumber,
        decimal insuredAcres,
        decimal approvedYield,
        decimal coverageLevel,
        decimal demonstrationPrice,
        DateOnly lossDate,
        string lossCause,
        string description,
        string actorName,
        string actorRole,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        ClaimNumber = claimNumber;
        PolicyId = policyId;
        PolicyNumber = policyNumber;
        ProducerId = producerId;
        ProducerActorId = producerActorId;
        ProducerName = producerName;
        Crop = crop;
        County = county;
        FieldId = fieldId;
        FieldNumber = fieldNumber;
        InsuredAcres = insuredAcres;
        ApprovedYield = approvedYield;
        CoverageLevel = coverageLevel;
        DemonstrationPrice = demonstrationPrice;
        LossDate = lossDate;
        LossCause = lossCause;
        Description = description;
        Status = ClaimStatus.LossReported;
        CreatedAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
        AddTimeline("Notice of Loss submitted.", actorName, actorRole, occurredAtUtc);
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string ClaimNumber { get; private set; } = string.Empty;
    public Guid PolicyId { get; private set; }
    public string PolicyNumber { get; private set; } = string.Empty;
    public Guid ProducerId { get; private set; }
    public string ProducerActorId { get; private set; } = string.Empty;
    public string ProducerName { get; private set; } = string.Empty;
    public string Crop { get; private set; } = string.Empty;
    public string County { get; private set; } = string.Empty;
    public Guid FieldId { get; private set; }
    public string FieldNumber { get; private set; } = string.Empty;
    public decimal InsuredAcres { get; private set; }
    public decimal ApprovedYield { get; private set; }
    public decimal CoverageLevel { get; private set; }
    public decimal DemonstrationPrice { get; private set; }
    public DateOnly LossDate { get; private set; }
    public string LossCause { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ClaimStatus Status { get; private set; }
    public string? AssignedAdjusterId { get; private set; }
    public string? AssignedAdjusterName { get; private set; }
    public decimal? ActualProduction { get; private set; }
    public string? InspectionNotes { get; private set; }
    public decimal? EstimatedIndemnity { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public List<ClaimTimelineEntry> Timeline { get; private set; } = [];

    public static Claim ReportLoss(
        Guid id,
        string tenantId,
        string claimNumber,
        Guid policyId,
        string policyNumber,
        Guid producerId,
        string producerActorId,
        string producerName,
        string crop,
        string county,
        Guid fieldId,
        string fieldNumber,
        decimal insuredAcres,
        decimal approvedYield,
        decimal coverageLevel,
        decimal demonstrationPrice,
        DateOnly lossDate,
        string lossCause,
        string description,
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        if (lossDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Loss date cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(lossCause))
        {
            throw new ArgumentException("Loss cause is required.");
        }

        return new Claim(
            id,
            tenantId,
            claimNumber,
            policyId,
            policyNumber,
            producerId,
            producerActorId,
            producerName,
            crop,
            county,
            fieldId,
            fieldNumber,
            insuredAcres,
            approvedYield,
            coverageLevel,
            demonstrationPrice,
            lossDate,
            lossCause.Trim(),
            description.Trim(),
            actorName,
            actorRole,
            occurredAtUtc ?? DateTimeOffset.UtcNow);
    }

    public void AssignAdjuster(
        string adjusterId,
        string adjusterName,
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        EnsureStatus(ClaimStatus.LossReported);
        if (string.IsNullOrWhiteSpace(adjusterId) || string.IsNullOrWhiteSpace(adjusterName))
        {
            throw new ArgumentException("Adjuster id and name are required.");
        }

        AssignedAdjusterId = adjusterId.Trim();
        AssignedAdjusterName = adjusterName.Trim();
        MoveTo(
            ClaimStatus.AdjusterAssigned,
            $"Assigned to adjuster {AssignedAdjusterName}.",
            actorName,
            actorRole,
            occurredAtUtc);
    }

    public void RecordInspection(
        string adjusterId,
        decimal actualProduction,
        string notes,
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        EnsureStatus(ClaimStatus.AdjusterAssigned);
        if (!string.Equals(AssignedAdjusterId, adjusterId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the assigned adjuster can record this inspection.");
        }

        if (actualProduction < 0)
        {
            throw new ArgumentException("Actual production cannot be negative.");
        }

        ActualProduction = actualProduction;
        InspectionNotes = notes.Trim();
        MoveTo(
            ClaimStatus.InspectionCompleted,
            "Field inspection completed and production evidence recorded.",
            actorName,
            actorRole,
            occurredAtUtc);
    }

    public void Approve(
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        EnsureStatus(ClaimStatus.InspectionCompleted);
        if (ActualProduction is null)
        {
            throw new InvalidOperationException("Inspection production is required before approval.");
        }

        var productionGuarantee = ApprovedYield * CoverageLevel * InsuredAcres;
        var lossQuantity = Math.Max(productionGuarantee - ActualProduction.Value, 0m);
        EstimatedIndemnity = decimal.Round(lossQuantity * DemonstrationPrice, 2);

        MoveTo(
            ClaimStatus.Approved,
            $"Claim approved. Demonstration indemnity: {EstimatedIndemnity:C}.",
            actorName,
            actorRole,
            occurredAtUtc);
    }

    public void RequestPayment(
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        EnsureStatus(ClaimStatus.Approved);
        MoveTo(
            ClaimStatus.PaymentRequested,
            "Payment request submitted to the settlement simulator.",
            actorName,
            actorRole,
            occurredAtUtc);
    }

    public void MarkPaid(
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc = null)
    {
        EnsureStatus(ClaimStatus.PaymentRequested);
        MoveTo(
            ClaimStatus.Paid,
            "Settlement simulator confirmed payment.",
            actorName,
            actorRole,
            occurredAtUtc);
    }

    private void MoveTo(
        ClaimStatus status,
        string note,
        string actorName,
        string actorRole,
        DateTimeOffset? occurredAtUtc)
    {
        Status = status;
        UpdatedAtUtc = occurredAtUtc ?? DateTimeOffset.UtcNow;
        AddTimeline(note, actorName, actorRole, UpdatedAtUtc);
    }

    private void EnsureStatus(ClaimStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Claim is {Status}; this action requires {expected}.");
        }
    }

    private void AddTimeline(
        string note,
        string actorName,
        string actorRole,
        DateTimeOffset occurredAtUtc)
    {
        Timeline.Add(new ClaimTimelineEntry(
            Guid.NewGuid(),
            Id,
            Status,
            note,
            actorName,
            actorRole,
            occurredAtUtc));
    }
}
