using AgriSure.BuildingBlocks.Identity;
using AgriSure.Claims.Api.Data;
using AgriSure.Claims.Api.Domain;
using AgriSure.Claims.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Claims.Api.Features;

public static class ClaimEndpoints
{
    public static IEndpointRouteBuilder MapClaimEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/claims").WithTags("Claims");

        group.MapGet("", ListClaimsAsync);
        group.MapGet("/{claimId:guid}", GetClaimAsync);
        group.MapPost("", ReportLossAsync);
        group.MapPost("/{claimId:guid}/assign", AssignAdjusterAsync);
        group.MapPost("/{claimId:guid}/inspection", RecordInspectionAsync);
        group.MapPost("/{claimId:guid}/approve", ApproveAsync);
        group.MapPost("/{claimId:guid}/request-payment", RequestPaymentAsync);
        group.MapPost("/{claimId:guid}/mark-paid", MarkPaidAsync);

        return routes;
    }

    private static async Task<IResult> ListClaimsAsync(
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Producer,
            Roles.Agent,
            Roles.Adjuster,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var query = db.Claims
            .AsNoTracking()
            .Where(x => x.TenantId == actor.TenantId);

        if (actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.ProducerActorId == actor.ActorId);
        }
        else if (actor.Role.Equals(Roles.Adjuster, StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.AssignedAdjusterId == actor.ActorId);
        }

        var claims = await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new ClaimSummaryResponse(
                x.Id,
                x.ClaimNumber,
                x.PolicyNumber,
                x.ProducerName,
                x.Crop,
                x.County,
                x.FieldNumber,
                x.LossDate,
                x.LossCause,
                x.Status.ToString(),
                x.AssignedAdjusterName,
                x.EstimatedIndemnity,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Results.Ok(claims);
    }

    private static async Task<IResult> GetClaimAsync(
        Guid claimId,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Producer,
            Roles.Agent,
            Roles.Adjuster,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await db.Claims
            .AsNoTracking()
            .Include(x => x.Timeline)
            .SingleOrDefaultAsync(
                x => x.Id == claimId && x.TenantId == actor.TenantId,
                cancellationToken);

        if (claim is null) return Results.NotFound();
        if (actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase) &&
            claim.ProducerActorId != actor.ActorId)
        {
            return Results.Forbid();
        }
        if (actor.Role.Equals(Roles.Adjuster, StringComparison.OrdinalIgnoreCase) &&
            claim.AssignedAdjusterId != actor.ActorId)
        {
            return Results.Forbid();
        }

        return Results.Ok(ToDetail(claim));
    }

    private static async Task<IResult> ReportLossAsync(
        ReportLossRequest request,
        HttpContext context,
        ClaimsDbContext db,
        PolicyClient policyClient,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.Producer, Roles.Agent);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var policy = await policyClient.GetClaimEligibilityAsync(
            request.PolicyId,
            actor,
            cancellationToken);

        if (policy is null) return Results.NotFound(new { message = "Policy was not found." });
        if (!policy.IsEligible)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Policy is not eligible",
                detail: "Only bound policies can accept a Notice of Loss.");
        }

        var field = policy.Fields.SingleOrDefault(x => x.FieldId == request.FieldId);
        if (field is null)
        {
            return Results.BadRequest(new { message = "Selected field does not belong to the policy." });
        }

        var producerActorId = actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase)
            ? actor.ActorId
            : request.ProducerActorId?.Trim() ?? "producer-1001";

        var nextNumber = await db.Claims.CountAsync(
            x => x.TenantId == actor.TenantId,
            cancellationToken) + 1;
        var claimNumber = $"CLM-{DateTime.UtcNow.Year}-{nextNumber:0000}";

        var claim = Claim.ReportLoss(
            Guid.NewGuid(),
            actor.TenantId,
            claimNumber,
            policy.PolicyId,
            policy.PolicyNumber,
            policy.ProducerId,
            producerActorId,
            policy.ProducerName,
            policy.Crop,
            policy.County,
            field.FieldId,
            field.FieldNumber,
            field.InsuredAcres,
            policy.ApprovedYield,
            policy.CoverageLevel,
            policy.DemonstrationPrice,
            request.LossDate,
            request.LossCause,
            request.Description,
            actor.ActorName,
            actor.Role);

        db.Claims.Add(claim);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimLossReported",
            "claims.loss-reported",
            "Notice of Loss submitted.",
            context));
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/claims/{claim.Id}", ToDetail(claim));
    }

    private static async Task<IResult> AssignAdjusterAsync(
        Guid claimId,
        AssignAdjusterRequest request,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.ClaimsReviewer, Roles.Operations);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await FindClaimAsync(claimId, actor.TenantId, db, cancellationToken);
        if (claim is null) return Results.NotFound();

        claim.AssignAdjuster(
            request.AdjusterId,
            request.AdjusterName,
            actor.ActorName,
            actor.Role);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimAdjusterAssigned",
            "claims.adjuster-assigned",
            $"Assigned to {request.AdjusterName}.",
            context));
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToDetail(claim));
    }

    private static async Task<IResult> RecordInspectionAsync(
        Guid claimId,
        RecordInspectionRequest request,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.Adjuster);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await FindClaimAsync(claimId, actor.TenantId, db, cancellationToken);
        if (claim is null) return Results.NotFound();

        claim.RecordInspection(
            actor.ActorId,
            request.ActualProduction,
            request.Notes,
            actor.ActorName,
            actor.Role);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimInspectionCompleted",
            "claims.inspection-completed",
            "Inspection completed.",
            context));
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToDetail(claim));
    }

    private static async Task<IResult> ApproveAsync(
        Guid claimId,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.ClaimsReviewer);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await FindClaimAsync(claimId, actor.TenantId, db, cancellationToken);
        if (claim is null) return Results.NotFound();

        claim.Approve(actor.ActorName, actor.Role);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimApproved",
            "claims.approved",
            "Claim approved with demonstration indemnity.",
            context));
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToDetail(claim));
    }

    private static async Task<IResult> RequestPaymentAsync(
        Guid claimId,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.ClaimsReviewer);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await FindClaimAsync(claimId, actor.TenantId, db, cancellationToken);
        if (claim is null) return Results.NotFound();

        claim.RequestPayment(actor.ActorName, actor.Role);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimPaymentRequested",
            "claims.payment-requested",
            "Payment requested.",
            context));
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToDetail(claim));
    }

    private static async Task<IResult> MarkPaidAsync(
        Guid claimId,
        HttpContext context,
        ClaimsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(context, Roles.Operations);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claim = await FindClaimAsync(claimId, actor.TenantId, db, cancellationToken);
        if (claim is null) return Results.NotFound();

        claim.MarkPaid(actor.ActorName, actor.Role);
        db.OutboxMessages.Add(ClaimEventFactory.Create(
            claim,
            "ClaimPaid",
            "claims.paid",
            "Payment confirmed.",
            context));
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToDetail(claim));
    }

    private static Task<Claim?> FindClaimAsync(
        Guid claimId,
        string tenantId,
        ClaimsDbContext db,
        CancellationToken cancellationToken) =>
        db.Claims
            .Include(x => x.Timeline)
            .SingleOrDefaultAsync(
                x => x.Id == claimId && x.TenantId == tenantId,
                cancellationToken);

    private static ClaimDetailResponse ToDetail(Claim claim) => new(
        claim.Id,
        claim.ClaimNumber,
        claim.PolicyId,
        claim.PolicyNumber,
        claim.ProducerId,
        claim.ProducerName,
        claim.Crop,
        claim.County,
        claim.FieldId,
        claim.FieldNumber,
        claim.InsuredAcres,
        claim.LossDate,
        claim.LossCause,
        claim.Description,
        claim.Status.ToString(),
        claim.AssignedAdjusterId,
        claim.AssignedAdjusterName,
        claim.ActualProduction,
        claim.InspectionNotes,
        claim.ApprovedYield,
        claim.CoverageLevel,
        claim.DemonstrationPrice,
        claim.EstimatedIndemnity,
        claim.CreatedAtUtc,
        claim.UpdatedAtUtc,
        claim.Timeline
            .OrderBy(x => x.OccurredAtUtc)
            .Select(x => new TimelineResponse(
                x.Id,
                x.Status.ToString(),
                x.Note,
                x.ActorName,
                x.ActorRole,
                x.OccurredAtUtc)));

    public sealed record ReportLossRequest(
        Guid PolicyId,
        Guid FieldId,
        DateOnly LossDate,
        string LossCause,
        string Description,
        string? ProducerActorId);

    public sealed record AssignAdjusterRequest(string AdjusterId, string AdjusterName);
    public sealed record RecordInspectionRequest(decimal ActualProduction, string Notes);

    private sealed record ClaimSummaryResponse(
        Guid Id,
        string ClaimNumber,
        string PolicyNumber,
        string ProducerName,
        string Crop,
        string County,
        string FieldNumber,
        DateOnly LossDate,
        string LossCause,
        string Status,
        string? AssignedAdjusterName,
        decimal? EstimatedIndemnity,
        DateTimeOffset UpdatedAtUtc);

    private sealed record ClaimDetailResponse(
        Guid Id,
        string ClaimNumber,
        Guid PolicyId,
        string PolicyNumber,
        Guid ProducerId,
        string ProducerName,
        string Crop,
        string County,
        Guid FieldId,
        string FieldNumber,
        decimal InsuredAcres,
        DateOnly LossDate,
        string LossCause,
        string Description,
        string Status,
        string? AssignedAdjusterId,
        string? AssignedAdjusterName,
        decimal? ActualProduction,
        string? InspectionNotes,
        decimal ApprovedYield,
        decimal CoverageLevel,
        decimal DemonstrationPrice,
        decimal? EstimatedIndemnity,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        IEnumerable<TimelineResponse> Timeline);

    private sealed record TimelineResponse(
        Guid Id,
        string Status,
        string Note,
        string ActorName,
        string ActorRole,
        DateTimeOffset OccurredAtUtc);
}
