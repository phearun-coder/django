using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Products;
using ProductInventoryAPI.Models.DTOs.Reports;
using ProductInventoryAPI.Shared.Common;
using ProductInventoryAPI.Shared.Constants;
using System.Security.Claims;

namespace ProductInventoryAPI.Web.Controllers.v1
{
    /// <summary>
    /// Controller for managing products
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
        {
            _logger.LogInformation("Getting all products");
            
            var result = await _productService.GetAllAsync();
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get products with pagination and filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="searchTerm">Search term for product name or SKU</param>
        /// <param name="categoryId">Filter by category ID</param>
        /// <returns>Paginated list of products</returns>
        [HttpGet("paged")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? categoryId = null)
        {
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Page number must be greater than 0"));
            }

            if (pageSize < 1 || pageSize > AppConstants.Pagination.MaxPageSize)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Page size must be between 1 and {AppConstants.Pagination.MaxPageSize}"));
            }

            _logger.LogInformation("Getting paged products - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, Category: {CategoryId}", 
                pageNumber, pageSize, searchTerm, categoryId);
            
            var result = await _productService.GetPagedAsync(pageNumber, pageSize, searchTerm, categoryId);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            
            var result = await _productService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get product by SKU
        /// </summary>
        /// <param name="sku">Product SKU</param>
        /// <returns>Product details</returns>
        [HttpGet("sku/{sku}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetBySku(string sku)
        {
            _logger.LogInformation("Getting product with SKU: {SKU}", sku);
            
            var result = await _productService.GetBySkuAsync(sku);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of products in the category</returns>
        [HttpGet("category/{categoryId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(int categoryId)
        {
            _logger.LogInformation("Getting products by category ID: {CategoryId}", categoryId);
            
            var result = await _productService.GetByCategoryAsync(categoryId);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createDto">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating product with name: {ProductName} by user: {UserId}", createDto.Name, userId);
            
            var result = await _productService.CreateAsync(createDto);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("already exists") == true || result.Message?.Contains("does not exist") == true)
                {
                    return Conflict(result);
                }
                
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data?.Id },
                result);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateDto">Product update data</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating product with ID: {ProductId} by user: {UserId}", id, userId);
            
            var result = await _productService.UpdateAsync(id, updateDto);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                
                if (result.Message?.Contains("already exists") == true || result.Message?.Contains("does not exist") == true)
                {
                    return Conflict(result);
                }
                
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = AppConstants.UserRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting product with ID: {ProductId} by user: {UserId}", id, userId);
            
            var result = await _productService.DeleteAsync(id);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get inventory report for all products
        /// </summary>
        /// <param name="fromDate">Start date filter (optional)</param>
        /// <param name="toDate">End date filter (optional)</param>
        /// <returns>Inventory report</returns>
        [HttpGet("reports/inventory")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductInventoryReport>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductInventoryReport>>>> GetInventoryReport(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            _logger.LogInformation("Getting inventory report from {FromDate} to {ToDate}", fromDate, toDate);
            
            var result = await _productService.GetInventoryReportAsync(fromDate, toDate);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get low stock report
        /// </summary>
        /// <param name="threshold">Stock threshold (default: 10)</param>
        /// <returns>Low stock report</returns>
        [HttpGet("reports/low-stock")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LowStockReport>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LowStockReport>>>> GetLowStockReport([FromQuery] int threshold = 10)
        {
            if (threshold < 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Threshold must be non-negative"));
            }

            _logger.LogInformation("Getting low stock report with threshold: {Threshold}", threshold);
            
            var result = await _productService.GetLowStockReportAsync(threshold);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Check if product exists
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Existence result</returns>
        [HttpHead("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Exists(int id)
        {
            var result = await _productService.ExistsAsync(id);
            
            if (!result.Success || !result.Data)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}