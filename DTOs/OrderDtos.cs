namespace WMSLite.DTOs;

public record CreateOrderRequest(List<CreateOrderLineRequest> Lines);
public record CreateOrderLineRequest(string ItemId, int Quantity);
