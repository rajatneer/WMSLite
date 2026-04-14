namespace WMSLite.Api.DTOs;

public record SubscribeRequest(int Seats, int DurationDays);
public record BillingStatusResponse(Guid TenantId, bool TrialActive, DateTime TrialEndsAtUtc, bool SubscriptionActive, int PurchasedSeats, DateTime? SubscriptionEndsAtUtc);
