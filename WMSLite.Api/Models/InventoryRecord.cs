namespace WMSLite.Api.Models;

public class InventoryRecord : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ItemId { get; set; }
    public Guid LocationId { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
