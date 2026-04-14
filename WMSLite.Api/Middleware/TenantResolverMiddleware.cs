using System.Security.Claims;
using WMSLite.Api.Services;

namespace WMSLite.Api.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenantId")?.Value;
            var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst(ClaimTypes.Name)?.Value
                           ?? context.User.FindFirst("sub")?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                tenantContext.TenantId = tenantId;
            }

            if (Guid.TryParse(subClaim, out var userId))
            {
                tenantContext.UserId = userId;
            }

            tenantContext.Role = role;
        }

        await _next(context);
    }
}
