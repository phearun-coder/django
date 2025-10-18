using ProductInventoryAPI.Models.DTOs.Inventory;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Inventory business logic
    /// </summary>
    public interface IInventoryService
    {
        Task<ApiResponse<InventoryDto>> GetByIdAsync(int id);
        Task<ApiResponse<InventoryDto>> GetByProductIdAsync(int productId);
        Task<ApiResponse<IEnumerable<InventoryDto>>> GetAllAsync();
        Task<ApiResponse<PagedResult<InventoryDto>>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ApiResponse<IEnumerable<InventoryDto>>> GetLowStockItemsAsync(int threshold = 10);
        Task<ApiResponse<InventoryDto>> CreateAsync(CreateInventoryDto createDto);
        Task<ApiResponse<InventoryDto>> UpdateAsync(int id, UpdateInventoryDto updateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<InventoryDto>> UpdateStockAsync(int productId, UpdateStockDto updateStockDto);
        Task<ApiResponse<InventoryDto>> SetStockAsync(int productId, SetStockDto setStockDto);
        Task<ApiResponse<bool>> ExistsAsync(int id);
        Task<ApiResponse<bool>> ExistsForProductAsync(int productId);
    }
}