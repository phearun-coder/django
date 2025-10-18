namespace ProductInventoryAPI.Models.DTOs.Reports
{
    /// <summary>
    /// DTO for product inventory report (from stored procedure)
    /// </summary>
    public class ProductInventoryReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderPoint { get; set; }
        public decimal StockValue { get; set; }
        public string StockStatus { get; set; } = string.Empty; // Low, Normal, High
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for low stock report (from stored procedure)
    /// </summary>
    public class LowStockReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderPoint { get; set; }
        public int ShortfallQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ReorderValue { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public int DaysSinceLastUpdate { get; set; }
    }

    /// <summary>
    /// DTO for batch inventory updates
    /// </summary>
    public class InventoryUpdateItem
    {
        public int ProductId { get; set; }
        public int NewQuantity { get; set; }
        public string? UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO for inventory summary statistics
    /// </summary>
    public class InventorySummaryDto
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public DateTime LastCalculated { get; set; }
    }
}