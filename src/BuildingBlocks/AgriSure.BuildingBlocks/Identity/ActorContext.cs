using Microsoft.AspNetCore.Http;

namespace AgriSure.BuildingBlocks.Identity;

public sealed record ActorContext(
    string TenantId,
    string ActorId,
    string ActorName,
    string Role)
{
    public const string TenantHeader = "X-Tenant-Id";
    public const string ActorIdHeader = "X-Actor-Id";
    public const string ActorNameHeader = "X-Actor-Name";
    public const string RoleHeader = "X-Role";

    public static ActorContext From(HttpContext context)
    {
        return new ActorContext(
            Read(context, TenantHeader, "northstar-agency"),
            Read(context, ActorIdHeader, "agent-2001"),
            Read(context, ActorNameHeader, "Avery Johnson"),
            Read(context, RoleHeader, Roles.Agent));
    }

    private static string Read(HttpContext context, string headerName, string fallback)
    {
        var value = context.Request.Headers[headerName].ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}

public static class Roles
{
    public const string Producer = "Producer";
    public const string Agent = "Agent";
    public const string Adjuster = "Adjuster";
    public const string ClaimsReviewer = "ClaimsReviewer";
    public const string Operations = "Operations";
    public const string Administrator = "Administrator";
}
