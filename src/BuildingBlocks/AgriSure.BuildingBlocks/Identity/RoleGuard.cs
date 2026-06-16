using Microsoft.AspNetCore.Http;

namespace AgriSure.BuildingBlocks.Identity;

public static class RoleGuard
{
    public static IResult? Ensure(HttpContext context, params string[] allowedRoles)
    {
        var actor = ActorContext.From(context);
        return allowedRoles.Contains(actor.Role, StringComparer.OrdinalIgnoreCase)
            ? null
            : Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Role is not authorized",
                detail: $"The {actor.Role} role cannot perform this action.");
    }
}
