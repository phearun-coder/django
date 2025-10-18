using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductInventoryAPI.Infrastructure.Repositories.Contracts;
using ProductInventoryAPI.Models.DTOs;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Web.Controllers.v1;

/// <summary>
/// Controller for inventory movement operations using stored procedures
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class InventoryMovementController : ControllerBase
{
    private readonly IStoredProcedureRepository _storedProcedureRepository;
    private readonly ILogger<InventoryMovementController> _logger;

    public InventoryMovementController(
        IStoredProcedureRepository storedProcedureRepository,
        ILogger<InventoryMovementController> logger)
    {
        _storedProcedureRepository = storedProcedureRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive product inventory summary using stored procedures
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="lowStockOnly">Show only low stock items</param>
    /// <returns>Product inventory summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductInventorySummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductInventorySummaryDto>>>> GetInventorySummary(
        [FromQuery] int? categoryId = null,
        [FromQuery] bool lowStockOnly = false)
    {
        try
        {
            _logger.LogInformation("Getting inventory summary via stored procedure. CategoryId: {CategoryId}, LowStockOnly: {LowStockOnly}", 
                categoryId, lowStockOnly);

            var parameters = new[]
            {
                new SqlParameter("@CategoryId", categoryId.HasValue ? (object)categoryId.Value : DBNull.Value),
                new SqlParameter("@LowStockOnly", lowStockOnly)
            };

            var summary = await _storedProcedureRepository.ExecuteStoredProcedureAsync<ProductInventorySummaryDto>(
                "sp_GetProductInventorySummary", parameters);

            return Ok(ApiResponse<IEnumerable<ProductInventorySummaryDto>>.SuccessResult(
                summary, "Inventory summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory summary");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving inventory summary"));
        }
    }

    /// <summary>
    /// Get stock alerts using stored procedures
    /// </summary>
    /// <param name="alertLevel">Filter by alert level: Critical, Warning, Info, or All</param>
    /// <returns>Stock alerts</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockAlertDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockAlertDto>>>> GetStockAlerts(
        [FromQuery] string alertLevel = "All")
    {
        try
        {
            _logger.LogInformation("Getting stock alerts via stored procedure. AlertLevel: {AlertLevel}", alertLevel);

            var parameters = new[]
            {
                new SqlParameter("@AlertLevel", alertLevel)
            };

            var alerts = await _storedProcedureRepository.ExecuteStoredProcedureAsync<StockAlertDto>(
                "sp_GetStockAlerts", parameters);

            return Ok(ApiResponse<IEnumerable<StockAlertDto>>.SuccessResult(
                alerts, $"Stock alerts retrieved successfully for level: {alertLevel}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock alerts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while retrieving stock alerts"));
        }
    }

    /// <summary>
    /// Record an inventory movement using stored procedures
    /// </summary>
    /// <param name="request">Inventory movement request</param>
    /// <returns>Movement result</returns>
    [HttpPost("movements")]
    [ProducesResponseType(typeof(ApiResponse<InventoryMovementResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryMovementResult>>> RecordMovement([FromBody] RecordInventoryMovementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid request data"));
            }

            _logger.LogInformation("Recording inventory movement via stored procedure. ProductId: {ProductId}, Type: {MovementType}, Quantity: {Quantity}", 
                request.ProductId, request.MovementType, request.Quantity);

            // Get current user ID from JWT claims (simplified for demo)
            var userIdClaim = User.FindFirst("userId")?.Value;
            var userId = !string.IsNullOrEmpty(userIdClaim) ? int.Parse(userIdClaim) : (int?)null;

            var parameters = new[]
            {
                new SqlParameter("@ProductId", request.ProductId),
                new SqlParameter("@MovementType", request.MovementType),
                new SqlParameter("@Quantity", request.Quantity),
                new SqlParameter("@Reason", request.Reason ?? (object)DBNull.Value),
                new SqlParameter("@UserId", userId.HasValue ? (object)userId.Value : DBNull.Value)
            };

            var results = await _storedProcedureRepository.ExecuteStoredProcedureAsync<InventoryMovementResult>(
                "sp_RecordInventoryMovement", parameters);

            var result = results.FirstOrDefault();
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResult("Failed to record inventory movement"));
            }

            return CreatedAtAction(nameof(RecordMovement), 
                ApiResponse<InventoryMovementResult>.SuccessResult(result, "Inventory movement recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording inventory movement");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResult("An error occurred while recording inventory movement"));
        }
    }
}

/// <summary>
/// Request model for recording inventory movement
/// </summary>
public class RecordInventoryMovementRequest
{
    public int ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty; // Inbound, Outbound, Adjustment, Transfer, Sale, Purchase, Return
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Result model for inventory movement operation
/// </summary>
public class InventoryMovementResult
{
    public int MovementId { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string Message { get; set; } = string.Empty;
}