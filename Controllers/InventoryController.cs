using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.DTOs;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IJsonRepository<InventoryRecord> _inventoryRepository;

    public InventoryController(IJsonRepository<InventoryRecord> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryRecord>>> ViewInventory()
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        var data = (await _inventoryRepository.GetAllAsync()).Where(x => x.TenantId == tenantId).ToList();
        return Ok(data);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateInventory([FromBody] UpdateInventoryRequest request)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        var existing = (await _inventoryRepository.GetAllAsync())
            .FirstOrDefault(x => x.TenantId == tenantId && x.ItemId == request.ItemId && x.LocationCode == request.LocationCode);

        if (existing is null)
        {
            existing = new InventoryRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                TenantId = tenantId,
                ItemId = request.ItemId,
                LocationCode = request.LocationCode,
                Quantity = request.Quantity
            };
            await _inventoryRepository.InsertAsync(existing);
        }
        else
        {
            existing.Quantity = request.Quantity;
            await _inventoryRepository.UpdateAsync(existing);
        }

        return Ok(existing);
    }
}
