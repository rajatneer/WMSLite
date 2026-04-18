namespace WMSLite.DTOs;

public record UpdateInventoryRequest(string ItemId, string LocationCode, int Quantity);
