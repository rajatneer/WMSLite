using WMSLite.Api.DTOs;

namespace WMSLite.Api.Services;

public interface IInventoryService
{
    Task<List<InventoryDto>> GetInventoryAsync(Guid tenantId);
    Task<InventoryDto> UpdateInventoryAsync(Guid tenantId, UpdateInventoryRequest request);
}
