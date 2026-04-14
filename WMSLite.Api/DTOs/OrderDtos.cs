namespace WMSLite.Api.DTOs;

public record CreateOrderLineRequest(Guid ItemId, int Quantity);
public record CreateOrderRequest(string OrderNumber, List<CreateOrderLineRequest> Lines);
