using AgriSure.BuildingBlocks.Identity;
using AgriSure.Operations.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Operations.Api.Features;

public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/operations").WithTags("Operations");
        group.MapGet("/dashboard", GetDashboardAsync);
        group.MapGet("/claims", GetClaimsAsync);
        return routes;
    }

    private static async Task<IResult> GetDashboardAsync(
        HttpContext context,
        OperationsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Agent,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var query = db.Claims.AsNoTracking().Where(x => x.TenantId == actor.TenantId);
        var claims = await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        var byStatus = claims
            .GroupBy(x => x.Status)
            .OrderBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Count());

        return Results.Ok(new DashboardResponse(
            claims.Count,
            claims.Count(x => x.Status is not "Paid" and not "Denied" and not "Withdrawn"),
            claims.Count(x => x.Status == "InspectionCompleted"),
            claims.Where(x => x.Status is "Approved" or "PaymentRequested" or "Paid")
                .Sum(x => x.EstimatedIndemnity ?? 0m),
            byStatus,
            claims.Take(5).Select(x => new OperationsClaimResponse(
                x.ClaimId,
                x.ClaimNumber,
                x.PolicyNumber,
                x.ProducerName,
                x.Crop,
                x.County,
                x.Status,
                x.EstimatedIndemnity,
                x.LastNote,
                x.UpdatedAtUtc))));
    }

    private static async Task<IResult> GetClaimsAsync(
        HttpContext context,
        OperationsDbContext db,
        CancellationToken cancellationToken)
    {
        var denied = RoleGuard.Ensure(
            context,
            Roles.Agent,
            Roles.ClaimsReviewer,
            Roles.Operations,
            Roles.Administrator);
        if (denied is not null) return denied;

        var actor = ActorContext.From(context);
        var claims = await db.Claims
            .AsNoTracking()
            .Where(x => x.TenantId == actor.TenantId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new OperationsClaimResponse(
                x.ClaimId,
                x.ClaimNumber,
                x.PolicyNumber,
                x.ProducerName,
                x.Crop,
                x.County,
                x.Status,
                x.EstimatedIndemnity,
                x.LastNote,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Results.Ok(claims);
    }

    private sealed record DashboardResponse(
        int TotalClaims,
        int OpenClaims,
        int AwaitingReview,
        decimal PortfolioIndemnity,
        IReadOnlyDictionary<string, int> ByStatus,
        IEnumerable<OperationsClaimResponse> RecentClaims);

    private sealed record OperationsClaimResponse(
        Guid ClaimId,
        string ClaimNumber,
        string PolicyNumber,
        string ProducerName,
        string Crop,
        string County,
        string Status,
        decimal? EstimatedIndemnity,
        string LastNote,
        DateTimeOffset UpdatedAtUtc);
}
