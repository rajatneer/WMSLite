namespace WMSLite.Models;

public class Order : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
    public List<OrderLine> Lines { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DispatchedAtUtc { get; set; }
}

public class OrderLine
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
