using AgriSure.BuildingBlocks.Identity;
using AgriSure.Policy.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Policy.Api.Features;

public static class PolicyEndpoints
{
    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/policies").WithTags("Policies");

        group.MapGet("", ListPoliciesAsync);
        group.MapGet("/{policyId:guid}", GetPolicyAsync);
        group.MapGet("/{policyId:guid}/claim-eligibility", GetClaimEligibilityAsync);

        return routes;
    }

    private static async Task<IResult> ListPoliciesAsync(
        HttpContext context,
        PolicyDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Producer,
            Roles.Agent,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var query = db.Policies
            .AsNoTracking()
            .Where(x => x.TenantId == actor.TenantId);

        if (actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase))
        {
            var producerId = await db.Producers
                .Where(x => x.TenantId == actor.TenantId && x.ExternalActorId == actor.ActorId)
                .Select(x => (Guid?)x.Id)
                .SingleOrDefaultAsync(cancellationToken);

            query = query.Where(x => x.ProducerId == producerId);
        }

        var policies = await query
            .OrderByDescending(x => x.CropYear)
            .ThenBy(x => x.PolicyNumber)
            .Select(x => new PolicySummaryResponse(
                x.Id,
                x.PolicyNumber,
                x.ProducerId,
                x.ProducerName,
                x.Crop,
                x.CropYear,
                x.County,
                x.State,
                x.Status,
                x.CoverageLevel,
                x.Fields.Sum(field => field.InsuredAcres)))
            .ToListAsync(cancellationToken);

        return Results.Ok(policies);
    }

    private static async Task<IResult> GetPolicyAsync(
        Guid policyId,
        HttpContext context,
        PolicyDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Producer,
            Roles.Agent,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var policy = await db.Policies
            .AsNoTracking()
            .Include(x => x.Fields)
            .SingleOrDefaultAsync(
                x => x.Id == policyId && x.TenantId == actor.TenantId,
                cancellationToken);

        if (policy is null) return Results.NotFound();

        if (actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase))
        {
            var ownsPolicy = await db.Producers.AnyAsync(
                x => x.Id == policy.ProducerId &&
                     x.TenantId == actor.TenantId &&
                     x.ExternalActorId == actor.ActorId,
                cancellationToken);
            if (!ownsPolicy) return Results.Forbid();
        }

        return Results.Ok(new PolicyDetailResponse(
            policy.Id,
            policy.PolicyNumber,
            policy.ProducerId,
            policy.ProducerName,
            policy.Crop,
            policy.CropYear,
            policy.County,
            policy.State,
            policy.Status,
            policy.CoverageLevel,
            policy.ApprovedYield,
            policy.DemonstrationPrice,
            policy.EffectiveDate,
            policy.ExpirationDate,
            policy.Fields
                .OrderBy(x => x.FieldNumber)
                .Select(x => new FieldResponse(
                    x.Id,
                    x.FieldNumber,
                    x.FarmNumber,
                    x.TractNumber,
                    x.InsuredAcres,
                    x.PlantingDate,
                    x.GeoJson))));
    }

    private static async Task<IResult> GetClaimEligibilityAsync(
        Guid policyId,
        HttpContext context,
        PolicyDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Producer,
            Roles.Agent,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var policy = await db.Policies
            .AsNoTracking()
            .Include(x => x.Fields)
            .SingleOrDefaultAsync(
                x => x.Id == policyId && x.TenantId == actor.TenantId,
                cancellationToken);

        if (policy is null) return Results.NotFound();

        if (actor.Role.Equals(Roles.Producer, StringComparison.OrdinalIgnoreCase))
        {
            var ownsPolicy = await db.Producers.AnyAsync(
                x => x.Id == policy.ProducerId &&
                     x.TenantId == actor.TenantId &&
                     x.ExternalActorId == actor.ActorId,
                cancellationToken);
            if (!ownsPolicy) return Results.Forbid();
        }

        return Results.Ok(new ClaimEligibilityResponse(
            policy.Id,
            policy.PolicyNumber,
            policy.ProducerId,
            policy.ProducerName,
            policy.Crop,
            policy.County,
            policy.Status == "Bound",
            policy.CoverageLevel,
            policy.ApprovedYield,
            policy.DemonstrationPrice,
            policy.Fields.Select(x => new EligibleFieldResponse(
                x.Id,
                x.FieldNumber,
                x.InsuredAcres))));
    }

    private sealed record PolicySummaryResponse(
        Guid Id,
        string PolicyNumber,
        Guid ProducerId,
        string ProducerName,
        string Crop,
        int CropYear,
        string County,
        string State,
        string Status,
        decimal CoverageLevel,
        decimal TotalInsuredAcres);

    private sealed record PolicyDetailResponse(
        Guid Id,
        string PolicyNumber,
        Guid ProducerId,
        string ProducerName,
        string Crop,
        int CropYear,
        string County,
        string State,
        string Status,
        decimal CoverageLevel,
        decimal ApprovedYield,
        decimal DemonstrationPrice,
        DateOnly EffectiveDate,
        DateOnly ExpirationDate,
        IEnumerable<FieldResponse> Fields);

    private sealed record FieldResponse(
        Guid Id,
        string FieldNumber,
        string FarmNumber,
        string TractNumber,
        decimal InsuredAcres,
        DateOnly PlantingDate,
        string GeoJson);

    public sealed record ClaimEligibilityResponse(
        Guid PolicyId,
        string PolicyNumber,
        Guid ProducerId,
        string ProducerName,
        string Crop,
        string County,
        bool IsEligible,
        decimal CoverageLevel,
        decimal ApprovedYield,
        decimal DemonstrationPrice,
        IEnumerable<EligibleFieldResponse> Fields);

    public sealed record EligibleFieldResponse(
        Guid FieldId,
        string FieldNumber,
        decimal InsuredAcres);
}
