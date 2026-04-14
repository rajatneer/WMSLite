namespace WMSLite.Api.Models;

public interface ITenantEntity : IEntity
{
    Guid TenantId { get; set; }
}
