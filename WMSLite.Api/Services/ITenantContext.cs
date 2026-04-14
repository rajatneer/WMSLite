namespace WMSLite.Api.Services;

public interface ITenantContext
{
    Guid TenantId { get; set; }
    Guid UserId { get; set; }
    string Role { get; set; }
    bool IsResolved { get; }
}
