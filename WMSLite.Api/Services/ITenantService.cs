using WMSLite.Api.Models;

namespace WMSLite.Api.Services;

public interface ITenantService
{
    Task<Tenant?> GetDetailsAsync(Guid tenantId);
}
