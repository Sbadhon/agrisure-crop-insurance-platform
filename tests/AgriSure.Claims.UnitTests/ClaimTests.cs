using AgriSure.Claims.Api.Domain;
using Xunit;

namespace AgriSure.Claims.UnitTests;

public sealed class ClaimTests
{
    [Fact]
    public void ApproveBeforeInspection_IsRejected()
    {
        var claim = CreateClaim();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.Approve("Casey Patel", "ClaimsReviewer"));

        Assert.Contains("requires InspectionCompleted", exception.Message);
    }

    [Fact]
    public void FullWorkflow_CalculatesExpectedDemonstrationIndemnity()
    {
        var claim = CreateClaim();

        claim.AssignAdjuster(
            "adjuster-3001",
            "Morgan Lee",
            "Casey Patel",
            "ClaimsReviewer");
        claim.RecordInspection(
            "adjuster-3001",
            12_000m,
            "Hail damage confirmed.",
            "Morgan Lee",
            "Adjuster");
        claim.Approve("Casey Patel", "ClaimsReviewer");
        claim.RequestPayment("Casey Patel", "ClaimsReviewer");
        claim.MarkPaid("Riley Chen", "Operations");

        Assert.Equal(ClaimStatus.Paid, claim.Status);
        Assert.Equal(22_654.80m, claim.EstimatedIndemnity);
        Assert.Equal(6, claim.Timeline.Count);
    }

    [Fact]
    public void InspectionByDifferentAdjuster_IsRejected()
    {
        var claim = CreateClaim();
        claim.AssignAdjuster(
            "adjuster-3001",
            "Morgan Lee",
            "Casey Patel",
            "ClaimsReviewer");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RecordInspection(
                "adjuster-other",
                12_000m,
                "Attempted update.",
                "Other Adjuster",
                "Adjuster"));

        Assert.Contains("assigned adjuster", exception.Message);
    }

    private static Claim CreateClaim() => Claim.ReportLoss(
        Guid.NewGuid(),
        "northstar-agency",
        "CLM-2026-TEST",
        Guid.NewGuid(),
        "MPCI-MN-2026-00421",
        Guid.NewGuid(),
        "producer-1001",
        "Jordan Miller",
        "Corn",
        "Washington",
        Guid.NewGuid(),
        "F-102",
        118.40m,
        190m,
        0.75m,
        4.65m,
        new DateOnly(2026, 6, 5),
        "Hail",
        "Hail damage reported.",
        "Jordan Miller",
        "Producer");
}
