using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Infrastructure.Data;
using ProductInventoryAPI.Infrastructure.Repositories.Contracts;
using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Models.DTOs;

namespace ProductInventoryAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of Category repository with hybrid ORM support
    /// </summary>
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly IStoredProcedureRepository _storedProcedureRepository;

        public CategoryRepository(ProductInventoryDbContext context, IStoredProcedureRepository storedProcedureRepository) 
            : base(context)
        {
            _storedProcedureRepository = storedProcedureRepository;
        }

        /// <summary>
        /// Get category by ID with optional product loading
        /// </summary>
        public new async Task<Category> GetByIdAsync(int id)
        {
            var category = await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            return category ?? throw new KeyNotFoundException($"Category with ID {id} not found");
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get all categories with products count
        /// </summary>
        public new async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get paged categories
        /// </summary>
        public async Task<IEnumerable<Category>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbSet
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Search categories by name or description
        /// </summary>
        public async Task<IEnumerable<Category>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            var searchLower = searchTerm.ToLower();
            return await _dbSet
                .Where(c => c.Name.ToLower().Contains(searchLower) ||
                           (c.Description != null && c.Description.ToLower().Contains(searchLower)))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Check if category exists by ID
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Check if category has associated products
        /// </summary>
        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        /// <summary>
        /// Create new category
        /// </summary>
        public new async Task<Category> CreateAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Add(category);
            return category;
        }

        /// <summary>
        /// Update existing category
        /// </summary>
        public new async Task<Category> UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(category);
            return category;
        }

        /// <summary>
        /// Delete category by ID (soft delete if has products, hard delete otherwise)
        /// </summary>
        public new async Task<bool> DeleteAsync(int id)
        {
            var category = await _dbSet.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category has products
            var hasProducts = await HasProductsAsync(id);
            if (hasProducts)
            {
                // Soft delete - mark as inactive
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(category);
            }
            else
            {
                // Hard delete - remove completely
                _dbSet.Remove(category);
            }

            return true;
        }

        /// <summary>
        /// Count all categories
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        /// <summary>
        /// Count active categories
        /// </summary>
        public async Task<int> CountActiveAsync()
        {
            return await _dbSet.CountAsync(c => c.IsActive);
        }

        #region Hybrid ORM Methods using Stored Procedures

        /// <summary>
        /// Get comprehensive category statistics using stored procedure
        /// </summary>
        public async Task<IEnumerable<CategoryStatisticsDto>> GetCategoryStatisticsAsync()
        {
            var parameters = Array.Empty<SqlParameter>();
            return await _storedProcedureRepository.ExecuteStoredProcedureAsync<CategoryStatisticsDto>(
                "sp_GetCategoryStatistics", parameters);
        }

        /// <summary>
        /// Get category statistics by ID using stored procedure
        /// </summary>
        public async Task<CategoryStatisticsDto?> GetCategoryStatisticsByIdAsync(int categoryId)
        {
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", categoryId)
            };

            var results = await _storedProcedureRepository.ExecuteStoredProcedureAsync<CategoryStatisticsDto>(
                "sp_GetCategoryStatisticsById", parameters);

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Get categories that have products with low stock using stored procedure
        /// </summary>
        public async Task<IEnumerable<Category>> GetCategoriesWithLowStockProductsAsync()
        {
            var parameters = Array.Empty<SqlParameter>();
            return await _storedProcedureRepository.ExecuteStoredProcedureAsync<Category>(
                "sp_GetCategoriesWithLowStockProducts", parameters);
        }

        /// <summary>
        /// Get top categories by product count using stored procedure
        /// </summary>
        public async Task<IEnumerable<Category>> GetTopCategoriesByProductCountAsync(int topCount = 10)
        {
            var parameters = new[]
            {
                new SqlParameter("@TopCount", topCount)
            };

            return await _storedProcedureRepository.ExecuteStoredProcedureAsync<Category>(
                "sp_GetTopCategoriesByProductCount", parameters);
        }

        /// <summary>
        /// Bulk update category status using stored procedure
        /// </summary>
        public async Task<bool> BulkUpdateCategoryStatusAsync(List<int> categoryIds, bool isActive)
        {
            var categoryIdsTable = string.Join(",", categoryIds);
            var parameters = new[]
            {
                new SqlParameter("@CategoryIds", categoryIdsTable),
                new SqlParameter("@IsActive", isActive)
            };

            var rowsAffected = await _storedProcedureRepository.ExecuteStoredProcedureNonQueryAsync(
                "sp_BulkUpdateCategoryStatus", parameters);

            return rowsAffected > 0;
        }

        /// <summary>
        /// Get product count for a specific category using stored procedure
        /// </summary>
        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", categoryId)
            };

            var result = await _storedProcedureRepository.ExecuteStoredProcedureScalarAsync<int>(
                "sp_GetProductCountByCategory", parameters);

            return result;
        }

        /// <summary>
        /// Get total value of products in a category using stored procedure
        /// </summary>
        public async Task<decimal> GetCategoryTotalValueAsync(int categoryId)
        {
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", categoryId)
            };

            var result = await _storedProcedureRepository.ExecuteStoredProcedureScalarAsync<decimal>(
                "sp_GetCategoryTotalValue", parameters);

            return result;
        }

        #endregion
    }
}