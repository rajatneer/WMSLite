namespace WMSLite.Api.Models;

public class Tenant : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime TrialEndsAtUtc { get; set; }
}
