using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Models.DTOs.Reports;

namespace ProductInventoryAPI.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Product entity operations
    /// Demonstrates hybrid approach with both EF Core and stored procedures
    /// </summary>
    public interface IProductRepository
    {
        // Standard CRUD operations using EF Core
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<Product> GetBySkuAsync(string sku);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null);

        // Advanced queries with EF Core
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            string? searchTerm = null,
            int? categoryId = null);

        // Stored Procedure operations for complex reporting
        Task<IEnumerable<ProductInventoryReport>> GetInventoryReportAsync(
            DateTime? fromDate = null, 
            DateTime? toDate = null);
        
        Task<IEnumerable<LowStockReport>> GetLowStockReportAsync(int threshold = 10);
        
        Task<bool> UpdateInventoryBatchAsync(
            IEnumerable<InventoryUpdateItem> inventoryUpdates);
    }
}