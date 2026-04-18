using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSLite.DTOs;
using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IJsonRepository<Order> _orderRepository;

    public OrdersController(IJsonRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized();
        }

        var order = new Order
        {
            Id = Guid.NewGuid().ToString("N"),
            TenantId = tenantId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = "Created",
            CreatedAtUtc = DateTime.UtcNow,
            Lines = request.Lines.Select(x => new OrderLine { ItemId = x.ItemId, Quantity = x.Quantity }).ToList()
        };

        await _orderRepository.InsertAsync(order);
        return Ok(order);
    }

    [HttpPost("{id}/dispatch")]
    public async Task<ActionResult<Order>> Dispatch(string id)
    {
        var tenantId = HttpContext.Items["TenantId"] as string;
        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null || order.TenantId != tenantId)
        {
            return NotFound();
        }

        order.Status = "Dispatched";
        order.DispatchedAtUtc = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
        return Ok(order);
    }
}
