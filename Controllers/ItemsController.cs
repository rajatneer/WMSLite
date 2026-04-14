using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IJsonRepository<Item> _itemRepository;

    public ItemsController(IJsonRepository<Item> itemRepository)
    {
        _itemRepository = itemRepository;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        var items = (await _itemRepository.GetAllAsync()).Where(x => x.TenantId == tenantId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create([FromBody] Item request)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        request.Id = Guid.NewGuid().ToString("N");
        request.TenantId = tenantId;

        await _itemRepository.InsertAsync(request);
        return Ok(request);
    }
}
