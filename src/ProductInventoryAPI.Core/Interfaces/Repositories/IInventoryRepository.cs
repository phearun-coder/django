using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Inventory entity operations
    /// </summary>
    public interface IInventoryRepository
    {
        Task<Inventory> GetByIdAsync(int id);
        Task<Inventory> GetByProductIdAsync(int productId);
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<(IEnumerable<Inventory>, int)> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold = 10);
        Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold = 10);
        Task<Inventory> CreateAsync(Inventory inventory);
        Task<Inventory> UpdateAsync(Inventory inventory);
        Task<bool> UpdateQuantityAsync(int productId, int newQuantity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForProductAsync(int productId);
        
        // Bulk operations
        Task<bool> UpdateMultipleAsync(IEnumerable<Inventory> inventoryItems);
    }
}