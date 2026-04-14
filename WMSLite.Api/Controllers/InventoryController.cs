using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.DTOs;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ITenantContext _tenantContext;

    public InventoryController(IInventoryService inventoryService, ITenantContext tenantContext)
    {
        _inventoryService = inventoryService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> View()
        => Ok(await _inventoryService.GetInventoryAsync(_tenantContext.TenantId));

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateInventoryRequest request)
        => Ok(await _inventoryService.UpdateInventoryAsync(_tenantContext.TenantId, request));
}
