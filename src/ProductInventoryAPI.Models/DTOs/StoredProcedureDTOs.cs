namespace ProductInventoryAPI.Models.DTOs;

/// <summary>
/// DTO for category statistics returned from stored procedures
/// </summary>
public class CategoryStatisticsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockProducts { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO for product inventory summary returned from stored procedures
/// </summary>
public class ProductInventorySummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime LastStockUpdate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for inventory movement history returned from stored procedures
/// </summary>
public class InventoryMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for sales analytics returned from stored procedures
/// </summary>
public class SalesAnalyticsDto
{
    public DateTime Period { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
}

/// <summary>
/// DTO for stock alert information returned from stored procedures
/// </summary>
public class StockAlertDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public string AlertLevel { get; set; } = string.Empty; // Critical, Warning, Info
    public DateTime LastRestocked { get; set; }
    public int DaysOutOfStock { get; set; }
}

/// <summary>
/// DTO for user activity audit returned from stored procedures
/// </summary>
public class UserActivityDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}