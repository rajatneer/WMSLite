namespace WMSLite.Api.DTOs;

public record UpdateInventoryRequest(Guid ItemId, Guid LocationId, int QuantityDelta);
public record InventoryDto(Guid Id, Guid ItemId, Guid LocationId, int Quantity, DateTime UpdatedAtUtc);
