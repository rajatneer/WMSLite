using WMSLite.Api.DTOs;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;

namespace WMSLite.Api.Services;

public class InventoryService : IInventoryService
{
    private readonly IJsonRepository<InventoryRecord> _inventoryRepo;

    public InventoryService(IJsonRepository<InventoryRecord> inventoryRepo)
    {
        _inventoryRepo = inventoryRepo;
    }

    public async Task<List<InventoryDto>> GetInventoryAsync(Guid tenantId)
    {
        return (await _inventoryRepo.GetAllAsync())
            .Where(i => i.TenantId == tenantId)
            .Select(i => new InventoryDto(i.Id, i.ItemId, i.LocationId, i.Quantity, i.UpdatedAtUtc))
            .ToList();
    }

    public async Task<InventoryDto> UpdateInventoryAsync(Guid tenantId, UpdateInventoryRequest request)
    {
        var all = await _inventoryRepo.GetAllAsync();
        var record = all.FirstOrDefault(i => i.TenantId == tenantId && i.ItemId == request.ItemId && i.LocationId == request.LocationId);

        if (record is null)
        {
            record = new InventoryRecord
            {
                TenantId = tenantId,
                ItemId = request.ItemId,
                LocationId = request.LocationId,
                Quantity = request.QuantityDelta,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _inventoryRepo.InsertAsync(record);
        }
        else
        {
            record.Quantity += request.QuantityDelta;
            record.UpdatedAtUtc = DateTime.UtcNow;
            await _inventoryRepo.UpdateAsync(record);
        }

        return new InventoryDto(record.Id, record.ItemId, record.LocationId, record.Quantity, record.UpdatedAtUtc);
    }
}
