using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IJsonRepository<Item> _itemRepo;
    private readonly ITenantContext _tenantContext;

    public ItemsController(IJsonRepository<Item> itemRepo, ITenantContext tenantContext)
    {
        _itemRepo = itemRepo;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await _itemRepo.GetAllAsync()).Where(i => i.TenantId == _tenantContext.TenantId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Item item)
    {
        item.Id = Guid.NewGuid();
        item.TenantId = _tenantContext.TenantId;
        return Ok(await _itemRepo.InsertAsync(item));
    }
}
