using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Categories;
using ProductInventoryAPI.Shared.Common;
using ProductInventoryAPI.Shared.Constants;
using System.Security.Claims;

namespace ProductInventoryAPI.Web.Controllers.v1
{
    /// <summary>
    /// Controller for managing product categories
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of all categories</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
        {
            _logger.LogInformation("Getting all categories");
            
            var result = await _categoryService.GetAllAsync();
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get categories with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>Paginated list of categories</returns>
        [HttpGet("paged")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<CategoryDto>>>> GetPaged(
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

            _logger.LogInformation("Getting paged categories - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var result = await _categoryService.GetPagedAsync(pageNumber, pageSize);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            
            var result = await _categoryService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Search categories by name
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching categories</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Search term is required"));
            }

            if (searchTerm.Length < 2)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Search term must be at least 2 characters long"));
            }

            _logger.LogInformation("Searching categories with term: {SearchTerm}", searchTerm);
            
            var result = await _categoryService.SearchAsync(searchTerm);
            
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="createDto">Category creation data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto createDto)
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
            _logger.LogInformation("Creating category with name: {CategoryName} by user: {UserId}", createDto.Name, userId);
            
            var result = await _categoryService.CreateAsync(createDto);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("already exists") == true)
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
        /// Update an existing category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="updateDto">Category update data</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{AppConstants.UserRoles.Admin},{AppConstants.UserRoles.Manager}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryDto updateDto)
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
            _logger.LogInformation("Updating category with ID: {CategoryId} by user: {UserId}", id, userId);
            
            var result = await _categoryService.UpdateAsync(id, updateDto);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                
                if (result.Message?.Contains("already exists") == true)
                {
                    return Conflict(result);
                }
                
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category ID</param>
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
            _logger.LogInformation("Deleting category with ID: {CategoryId} by user: {UserId}", id, userId);
            
            var result = await _categoryService.DeleteAsync(id);
            
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
        /// Check if category exists
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Existence result</returns>
        [HttpHead("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Exists(int id)
        {
            var result = await _categoryService.ExistsAsync(id);
            
            if (!result.Success || !result.Data)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}