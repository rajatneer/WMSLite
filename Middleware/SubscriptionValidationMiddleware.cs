using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Middleware;

public class SubscriptionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJsonRepository<Subscription> subscriptionRepository)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method.ToUpperInvariant();
        var isWrite = method is "POST" or "PUT" or "PATCH" or "DELETE";
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var billingOrAuthPath = path.StartsWith("/api/billing") || path.StartsWith("/api/auth");

        if (!isWrite || billingOrAuthPath)
        {
            await _next(context);
            return;
        }

        var tenantId = context.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Tenant not resolved." });
            return;
        }

        var subs = await subscriptionRepository.GetAllAsync();
        var sub = subs.FirstOrDefault(x => x.TenantId == tenantId);
        if (sub is null || !sub.IsWriteAccessAllowed(DateTime.UtcNow))
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            await context.Response.WriteAsJsonAsync(new { message = "Trial/subscription expired. Payment required." });
            return;
        }

        await _next(context);
    }
}
