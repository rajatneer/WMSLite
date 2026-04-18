using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController : ControllerBase
{
    private readonly IJsonRepository<Tenant> _tenantRepository;

    public TenantController(IJsonRepository<Tenant> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpGet("details")]
    public async Task<ActionResult<Tenant>> Details()
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        var all = await _tenantRepository.GetAllAsync();
        var tenant = all.FirstOrDefault(x => x.Id == tenantId);
        return tenant is null ? NotFound() : Ok(tenant);
    }
}
