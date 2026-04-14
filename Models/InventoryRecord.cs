namespace WMSLite.Models;

public class InventoryRecord : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
