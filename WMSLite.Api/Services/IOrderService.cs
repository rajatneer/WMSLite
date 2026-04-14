using WMSLite.Api.DTOs;
using WMSLite.Api.Models;

namespace WMSLite.Api.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(Guid tenantId, CreateOrderRequest request);
    Task<Order?> DispatchAsync(Guid tenantId, Guid orderId);
    Task<List<Order>> GetAllAsync(Guid tenantId);
}
