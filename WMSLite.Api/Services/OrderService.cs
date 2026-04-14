using WMSLite.Api.DTOs;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class OrderService : IOrderService
{
    private readonly IJsonRepository<Order> _orderRepo;

    public OrderService(IJsonRepository<Order> orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<Order> CreateAsync(Guid tenantId, CreateOrderRequest request)
    {
        var order = new Order
        {
            TenantId = tenantId,
            OrderNumber = request.OrderNumber,
            Status = "Created",
            Lines = request.Lines.Select(l => new OrderLine { ItemId = l.ItemId, Quantity = l.Quantity }).ToList()
        };
        await _orderRepo.InsertAsync(order);
        return order;
    }

    public async Task<Order?> DispatchAsync(Guid tenantId, Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order is null || order.TenantId != tenantId) return null;

        order.Status = "Dispatched";
        order.DispatchedAtUtc = DateTime.UtcNow;
        await _orderRepo.UpdateAsync(order);
        return order;
    }

    public async Task<List<Order>> GetAllAsync(Guid tenantId)
        => (await _orderRepo.GetAllAsync()).Where(o => o.TenantId == tenantId).ToList();
}
