namespace WMSLite.Api.Models;

public class Order : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
    public List<OrderLine> Lines { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DispatchedAtUtc { get; set; }
}

public class OrderLine
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}
