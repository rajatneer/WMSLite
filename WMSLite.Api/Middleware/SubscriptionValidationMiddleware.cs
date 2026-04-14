using WMSLite.Api.Services;

namespace WMSLite.Api.Middleware;

public class SubscriptionValidationMiddleware
{
    private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    private readonly RequestDelegate _next;

    public SubscriptionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantContext tenantContext, IBillingService billingService)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var isAuthOrBillingRoute = path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)
                                || path.StartsWith("/api/billing", StringComparison.OrdinalIgnoreCase);

        if (context.User.Identity?.IsAuthenticated == true
            && tenantContext.IsResolved
            && WriteMethods.Contains(context.Request.Method)
            && !isAuthOrBillingRoute)
        {
            var canWrite = await billingService.CanWriteAsync(tenantContext.TenantId);
            if (!canWrite)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Trial/subscription expired. Payment required to continue write operations."
                });
                return;
            }
        }

        await _next(context);
    }
}
