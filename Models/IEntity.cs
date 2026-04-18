namespace WMSLite.Models;

public interface IEntity
{
    string Id { get; set; }
    string TenantId { get; set; }
}
