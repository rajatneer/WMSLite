using System.Security.Claims;

namespace WMSLite.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.User.FindFirstValue("tenant_id");
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            context.Items["TenantId"] = tenantId;
        }

        await _next(context);
    }
}
