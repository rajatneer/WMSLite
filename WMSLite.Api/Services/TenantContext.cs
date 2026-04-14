namespace WMSLite.Api.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsResolved => TenantId != Guid.Empty;
}
