using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ITenantContext _tenantContext;

    public TenantController(ITenantService tenantService, ITenantContext tenantContext)
    {
        _tenantService = tenantService;
        _tenantContext = tenantContext;
    }

    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var tenant = await _tenantService.GetDetailsAsync(_tenantContext.TenantId);
        return tenant is null ? NotFound() : Ok(tenant);
    }
}
