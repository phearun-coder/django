using ProductInventoryAPI.Models.DTOs.Products;
using ProductInventoryAPI.Models.DTOs.Reports;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Product business logic
    /// </summary>
    public interface IProductService
    {
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync();
        Task<ApiResponse<PagedResult<ProductDto>>> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            string? searchTerm = null,
            int? categoryId = null);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetByCategoryAsync(int categoryId);
        Task<ApiResponse<ProductDto>> GetBySkuAsync(string sku);
        Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto createDto);
        Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto updateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> ExistsAsync(int id);
        
        // Reporting methods using stored procedures
        Task<ApiResponse<IEnumerable<ProductInventoryReport>>> GetInventoryReportAsync(
            DateTime? fromDate = null, 
            DateTime? toDate = null);
        Task<ApiResponse<IEnumerable<LowStockReport>>> GetLowStockReportAsync(int threshold = 10);
    }
}