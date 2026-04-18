using WMSLite.Models;
using WMSLite.Repositories;

namespace WMSLite.Services;

public class BillingService : IBillingService
{
    private readonly IJsonRepository<Subscription> _subscriptionRepository;

    public BillingService(IJsonRepository<Subscription> subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Subscription?> GetByTenantIdAsync(string tenantId)
    {
        var all = await _subscriptionRepository.GetAllAsync();
        return all.FirstOrDefault(x => x.TenantId == tenantId);
    }

    public async Task<Subscription> SubscribeAsync(string tenantId, int seats, int durationDays)
    {
        var sub = await GetByTenantIdAsync(tenantId) ?? throw new InvalidOperationException("Subscription not found.");

        sub.IsPaidActive = true;
        sub.SeatsPurchased = seats;
        sub.PaidEndsAtUtc = DateTime.UtcNow.AddDays(durationDays);

        await _subscriptionRepository.UpdateAsync(sub);
        return sub;
    }
}
