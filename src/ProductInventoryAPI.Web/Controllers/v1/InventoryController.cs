using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Inventory;
using ProductInventoryAPI.Shared.Common;
using ProductInventoryAPI.Shared.Constants;
using System.Security.Claims;

namespace ProductInventoryAPI.Web.Controllers.v1
{
    /// <summary>
    /// Controller for managing inventory
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all inventory records
        /// </summary>
        /// <returns>List of all inventory records</returns>
        [HttpGet]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetAll()
        {
            _logger.LogInformation("Getting all inventory records");
            
            var result = await _inventoryService.GetAllAsync();
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get inventory records with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>Paginated list of inventory records</returns>
        [HttpGet("paged")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<InventoryDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize)
        {
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Page number must be greater than 0"));
            }

            if (pageSize < 1 || pageSize > AppConstants.Pagination.MaxPageSize)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Page size must be between 1 and {AppConstants.Pagination.MaxPageSize}"));
            }

            _logger.LogInformation("Getting paged inventory records - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var result = await _inventoryService.GetPagedAsync(pageNumber, pageSize);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get inventory by ID
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <returns>Inventory details</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> GetById(int id)
        {
            _logger.LogInformation("Getting inventory with ID: {InventoryId}", id);
            
            var result = await _inventoryService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get inventory by product ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Inventory details for the product</returns>
        [HttpGet("product/{productId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> GetByProductId(int productId)
        {
            _logger.LogInformation("Getting inventory for product ID: {ProductId}", productId);
            
            var result = await _inventoryService.GetByProductIdAsync(productId);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get low stock items
        /// </summary>
        /// <param name="threshold">Stock threshold (default: 10)</param>
        /// <returns>List of low stock items</returns>
        [HttpGet("low-stock")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetLowStockItems([FromQuery] int threshold = 10)
        {
            if (threshold < 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Threshold must be non-negative"));
            }

            _logger.LogInformation("Getting low stock items with threshold: {Threshold}", threshold);
            
            var result = await _inventoryService.GetLowStockItemsAsync(threshold);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Create inventory for a product
        /// </summary>
        /// <param name="createDto">Inventory creation data</param>
        /// <returns>Created inventory</returns>
        [HttpPost]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> Create([FromBody] CreateInventoryDto createDto)
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
            _logger.LogInformation("Creating inventory for product ID: {ProductId} by user: {UserId}", createDto.ProductId, userId);
            
            var result = await _inventoryService.CreateAsync(createDto);
            
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
        /// Update inventory
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <param name="updateDto">Inventory update data</param>
        /// <returns>Updated inventory</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> Update(int id, [FromBody] UpdateInventoryDto updateDto)
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
            _logger.LogInformation("Updating inventory with ID: {InventoryId} by user: {UserId}", id, userId);
            
            var result = await _inventoryService.UpdateAsync(id, updateDto);
            
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
        /// Update stock quantity for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="updateStockDto">Stock update data</param>
        /// <returns>Updated inventory</returns>
        [HttpPatch("product/{productId:int}/stock")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager},{AppConstants.UserRoles.User}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> UpdateStock(int productId, [FromBody] UpdateStockDto updateStockDto)
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
            _logger.LogInformation("Updating stock for product ID: {ProductId} by user: {UserId}, Change: {QuantityChange}", 
                productId, userId, updateStockDto.QuantityChange);
            
            var result = await _inventoryService.UpdateStockAsync(productId, updateStockDto);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                
                if (result.Message?.Contains("negative") == true)
                {
                    return BadRequest(result);
                }
                
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Set absolute stock quantity for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="setStockDto">Stock set data</param>
        /// <returns>Updated inventory</returns>
        [HttpPut("product/{productId:int}/stock")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> SetStock(int productId, [FromBody] SetStockDto setStockDto)
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
            _logger.LogInformation("Setting stock for product ID: {ProductId} by user: {UserId}, New Quantity: {NewQuantity}", 
                productId, userId, setStockDto.NewQuantity);
            
            var result = await _inventoryService.SetStockAsync(productId, setStockDto);
            
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
        /// Delete inventory record
        /// </summary>
        /// <param name="id">Inventory ID</param>
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
            _logger.LogInformation("Deleting inventory with ID: {InventoryId} by user: {UserId}", id, userId);
            
            var result = await _inventoryService.DeleteAsync(id);
            
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
        /// Check if inventory exists for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Existence result</returns>
        [HttpHead("product/{productId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExistsForProduct(int productId)
        {
            var result = await _inventoryService.ExistsForProductAsync(productId);
            
            if (!result.Success || !result.Data)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}