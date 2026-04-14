using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class TenantService : ITenantService
{
    private readonly IJsonRepository<Tenant> _tenantRepo;

    public TenantService(IJsonRepository<Tenant> tenantRepo)
    {
        _tenantRepo = tenantRepo;
    }

    public Task<Tenant?> GetDetailsAsync(Guid tenantId)
        => _tenantRepo.GetByIdAsync(tenantId);
}
