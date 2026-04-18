using WMSLite.Models;

namespace WMSLite.Services;

public interface IBillingService
{
    Task<Subscription?> GetByTenantIdAsync(string tenantId);
    Task<Subscription> SubscribeAsync(string tenantId, int seats, int durationDays);
}
