using ProductInventoryAPI.Models.DTOs.Categories;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Category business logic
    /// </summary>
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryDto>> GetByIdAsync(int id);
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetActiveAsync();
        Task<ApiResponse<PagedResult<CategoryDto>>> GetPagedAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<IEnumerable<CategoryDto>>> SearchAsync(string searchTerm);
        Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto createDto);
        Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> ExistsAsync(int id);
    }
}