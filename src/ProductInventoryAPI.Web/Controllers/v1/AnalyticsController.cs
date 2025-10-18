using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Models.DTOs;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Web.Controllers.v1;

/// <summary>
/// Controller for analytics operations using hybrid ORM (EF Core + Stored Procedures)
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        ICategoryRepository categoryRepository,
        ILogger<AnalyticsController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive category statistics using stored procedures
    /// </summary>
    /// <returns>Category statistics including product counts, stock levels, and values</returns>
    [HttpGet("categories/statistics")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryStatisticsDto>>>> GetCategoryStatistics()
    {
        try
        {
            _logger.LogInformation("Getting category statistics via stored procedure");

            var statistics = await _categoryRepository.GetCategoryStatisticsAsync();

            return Ok(ApiResponse<IEnumerable<CategoryStatisticsDto>>.SuccessResult(
                statistics,
                "Category statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving category statistics"));
        }
    }

    /// <summary>
    /// Get statistics for a specific category using stored procedures
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>Category statistics for the specified category</returns>
    [HttpGet("categories/{categoryId:int}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<CategoryStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryStatisticsDto>>> GetCategoryStatistics(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting statistics for category {CategoryId} via stored procedure", categoryId);

            var statistics = await _categoryRepository.GetCategoryStatisticsByIdAsync(categoryId);

            if (statistics == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult($"Category with ID {categoryId} not found"));
            }

            return Ok(ApiResponse<CategoryStatisticsDto>.SuccessResult(
                statistics,
                $"Statistics for category {categoryId} retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for category {CategoryId}", categoryId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving category statistics"));
        }
    }

    /// <summary>
    /// Get categories that have products with low stock using stored procedures
    /// </summary>
    /// <returns>Categories with low stock products</returns>
    [HttpGet("categories/low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetCategoriesWithLowStock()
    {
        try
        {
            _logger.LogInformation("Getting categories with low stock products via stored procedure");

            var categories = await _categoryRepository.GetCategoriesWithLowStockProductsAsync();

            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                ProductCount = c.Products?.Count ?? 0
            });

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResult(
                result,
                "Categories with low stock products retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories with low stock products");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving low stock categories"));
        }
    }

    /// <summary>
    /// Get top categories by product count using stored procedures
    /// </summary>
    /// <param name="topCount">Number of top categories to retrieve (default: 10)</param>
    /// <returns>Top categories by product count</returns>
    [HttpGet("categories/top-by-product-count")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetTopCategoriesByProductCount([FromQuery] int topCount = 10)
    {
        try
        {
            _logger.LogInformation("Getting top {TopCount} categories by product count via stored procedure", topCount);

            var categories = await _categoryRepository.GetTopCategoriesByProductCountAsync(topCount);

            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                ProductCount = c.Products?.Count ?? 0
            });

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResult(
                result,
                $"Top {topCount} categories by product count retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top categories by product count");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving top categories"));
        }
    }

    /// <summary>
    /// Get product count for a specific category using stored procedures
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>Product count for the specified category</returns>
    [HttpGet("categories/{categoryId:int}/product-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> GetProductCountByCategory(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting product count for category {CategoryId} via stored procedure", categoryId);

            var productCount = await _categoryRepository.GetProductCountByCategoryAsync(categoryId);

            return Ok(ApiResponse<int>.SuccessResult(
                productCount,
                $"Product count for category {categoryId} retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product count for category {CategoryId}", categoryId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving product count"));
        }
    }

    /// <summary>
    /// Get total value of products in a category using stored procedures
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>Total value for the specified category</returns>
    [HttpGet("categories/{categoryId:int}/total-value")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetCategoryTotalValue(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting total value for category {CategoryId} via stored procedure", categoryId);

            var totalValue = await _categoryRepository.GetCategoryTotalValueAsync(categoryId);

            return Ok(ApiResponse<decimal>.SuccessResult(
                totalValue,
                $"Total value for category {categoryId} retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total value for category {CategoryId}", categoryId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving category total value"));
        }
    }

    /// <summary>
    /// Bulk update category status using stored procedures
    /// </summary>
    /// <param name="request">Bulk update request containing category IDs and status</param>
    /// <returns>Result of the bulk update operation</returns>
    [HttpPut("categories/bulk-update-status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> BulkUpdateCategoryStatus([FromBody] BulkUpdateCategoryStatusRequest request)
    {
        try
        {
            if (request.CategoryIds == null || !request.CategoryIds.Any())
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Category IDs are required"));
            }

            _logger.LogInformation("Bulk updating status for {Count} categories via stored procedure", request.CategoryIds.Count);

            var success = await _categoryRepository.BulkUpdateCategoryStatusAsync(request.CategoryIds, request.IsActive);

            return Ok(ApiResponse<bool>.SuccessResult(
                success,
                $"Bulk status update completed for {request.CategoryIds.Count} categories"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk category status update");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred during bulk status update"));
        }
    }
}

/// <summary>
/// Request model for bulk updating category status
/// </summary>
public class BulkUpdateCategoryStatusRequest
{
    public List<int> CategoryIds { get; set; } = new();
    public bool IsActive { get; set; }
}