using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Models.DTOs;

namespace ProductInventoryAPI.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Category entity operations with hybrid ORM support
    /// </summary>
    public interface ICategoryRepository
    {
        // Standard CRUD operations using EF Core
        Task<Category> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetActiveAsync();
        Task<IEnumerable<Category>> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<Category>> SearchAsync(string searchTerm);
        Task<int> CountAsync();
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasProductsAsync(int categoryId);

        // Hybrid ORM operations using stored procedures
        Task<IEnumerable<CategoryStatisticsDto>> GetCategoryStatisticsAsync();
        Task<CategoryStatisticsDto?> GetCategoryStatisticsByIdAsync(int categoryId);
        Task<IEnumerable<Category>> GetCategoriesWithLowStockProductsAsync();
        Task<IEnumerable<Category>> GetTopCategoriesByProductCountAsync(int topCount = 10);
        Task<bool> BulkUpdateCategoryStatusAsync(List<int> categoryIds, bool isActive);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
        Task<decimal> GetCategoryTotalValueAsync(int categoryId);
    }
}