using WMSLite.Api.DTOs;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class BillingService : IBillingService
{
    private readonly IJsonRepository<Tenant> _tenantRepo;
    private readonly IJsonRepository<Subscription> _subscriptionRepo;
    private readonly IJsonRepository<User> _userRepo;

    public BillingService(
        IJsonRepository<Tenant> tenantRepo,
        IJsonRepository<Subscription> subscriptionRepo,
        IJsonRepository<User> userRepo)
    {
        _tenantRepo = tenantRepo;
        _subscriptionRepo = subscriptionRepo;
        _userRepo = userRepo;
    }

    public async Task<BillingStatusResponse> GetStatusAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId) ?? throw new InvalidOperationException("Tenant not found.");
        var sub = (await _subscriptionRepo.GetAllAsync())
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        var trialActive = tenant.TrialEndsAtUtc >= DateTime.UtcNow;
        var subActive = sub is not null && sub.EndDateUtc >= DateTime.UtcNow;

        return new BillingStatusResponse(
            tenantId,
            trialActive,
            tenant.TrialEndsAtUtc,
            subActive,
            sub?.PurchasedSeats ?? 0,
            sub?.EndDateUtc);
    }

    public async Task<BillingStatusResponse> SubscribeAsync(Guid tenantId, SubscribeRequest request)
    {
        var subs = await _subscriptionRepo.GetAllAsync();
        var existingActive = subs
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        if (existingActive is not null)
        {
            existingActive.IsActive = false;
            await _subscriptionRepo.UpdateAsync(existingActive);
        }

        var start = DateTime.UtcNow;
        var subscription = new Subscription
        {
            TenantId = tenantId,
            IsActive = true,
            PurchasedSeats = request.Seats,
            StartDateUtc = start,
            EndDateUtc = start.AddDays(request.DurationDays)
        };

        await _subscriptionRepo.InsertAsync(subscription);
        return await GetStatusAsync(tenantId);
    }

    public async Task<bool> CanWriteAsync(Guid tenantId)
    {
        var status = await GetStatusAsync(tenantId);
        return status.TrialActive || status.SubscriptionActive;
    }

    public async Task<bool> CanAddUserAsync(Guid tenantId)
    {
        var users = (await _userRepo.GetAllAsync()).Count(u => u.TenantId == tenantId);
        var status = await GetStatusAsync(tenantId);

        if (status.TrialActive) return true;
        if (!status.SubscriptionActive) return false;

        return users < status.PurchasedSeats;
    }
}
