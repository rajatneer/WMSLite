using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.Api.DTOs;
using WMSLite.Api.Services;

namespace WMSLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ITenantContext _tenantContext;

    public OrdersController(IOrderService orderService, ITenantContext tenantContext)
    {
        _orderService = orderService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _orderService.GetAllAsync(_tenantContext.TenantId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        => Ok(await _orderService.CreateAsync(_tenantContext.TenantId, request));

    [HttpPost("{orderId:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid orderId)
    {
        var order = await _orderService.DispatchAsync(_tenantContext.TenantId, orderId);
        return order is null ? NotFound() : Ok(order);
    }
}
