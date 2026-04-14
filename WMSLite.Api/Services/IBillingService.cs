using WMSLite.Api.DTOs;

namespace WMSLite.Api.Services;

public interface IBillingService
{
    Task<BillingStatusResponse> GetStatusAsync(Guid tenantId);
    Task<BillingStatusResponse> SubscribeAsync(Guid tenantId, SubscribeRequest request);
    Task<bool> CanWriteAsync(Guid tenantId);
    Task<bool> CanAddUserAsync(Guid tenantId);
}
