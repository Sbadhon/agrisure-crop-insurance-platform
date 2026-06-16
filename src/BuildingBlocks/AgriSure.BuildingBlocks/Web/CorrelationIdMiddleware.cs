using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AgriSure.BuildingBlocks.Web;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].ToString().Trim();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        }

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        await next(context);
    }

    public static string Get(HttpContext context) =>
        context.Items.TryGetValue(HeaderName, out var value)
            ? value?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;
}
