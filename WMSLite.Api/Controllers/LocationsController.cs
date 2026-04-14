using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IJsonRepository<Location> _locationRepo;
    private readonly ITenantContext _tenantContext;

    public LocationsController(IJsonRepository<Location> locationRepo, ITenantContext tenantContext)
    {
        _locationRepo = locationRepo;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await _locationRepo.GetAllAsync()).Where(i => i.TenantId == _tenantContext.TenantId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Location location)
    {
        location.Id = Guid.NewGuid();
        location.TenantId = _tenantContext.TenantId;
        return Ok(await _locationRepo.InsertAsync(location));
    }
}
