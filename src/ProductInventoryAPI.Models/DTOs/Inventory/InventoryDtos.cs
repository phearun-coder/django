using System.ComponentModel.DataAnnotations;

namespace ProductInventoryAPI.Models.DTOs.Inventory
{
    /// <summary>
    /// DTO for inventory information
    /// </summary>
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for creating new inventory
    /// </summary>
    public class CreateInventoryDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity on hand is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity on hand must be non-negative")]
        public int QuantityOnHand { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be non-negative")]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage = "Max stock level is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Max stock level must be positive")]
        public int MaxStockLevel { get; set; }
    }

    /// <summary>
    /// DTO for updating inventory
    /// </summary>
    public class UpdateInventoryDto
    {
        [Required(ErrorMessage = "Quantity on hand is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity on hand must be non-negative")]
        public int QuantityOnHand { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be non-negative")]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage = "Max stock level is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Max stock level must be positive")]
        public int MaxStockLevel { get; set; }
    }

    /// <summary>
    /// DTO for updating stock quantity
    /// </summary>
    public class UpdateStockDto
    {
        [Required(ErrorMessage = "Quantity change is required")]
        public int QuantityChange { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for setting stock quantity
    /// </summary>
    public class SetStockDto
    {
        [Required(ErrorMessage = "New quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "New quantity must be non-negative")]
        public int NewQuantity { get; set; }

        public string? Reason { get; set; }
    }
}