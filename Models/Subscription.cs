namespace WMSLite.Models;

public class Subscription : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public DateTime TrialEndsAtUtc { get; set; }
    public bool IsPaidActive { get; set; }
    public int SeatsPurchased { get; set; }
    public DateTime? PaidEndsAtUtc { get; set; }

    public bool IsWriteAccessAllowed(DateTime nowUtc)
    {
        if (nowUtc <= TrialEndsAtUtc)
        {
            return true;
        }

        return IsPaidActive && PaidEndsAtUtc.HasValue && nowUtc <= PaidEndsAtUtc.Value;
    }
}
