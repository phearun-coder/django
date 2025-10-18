using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductInventoryAPI.Models.Entities
{
    /// <summary>
    /// Inventory entity representing stock levels for products
    /// </summary>
    [Table("Inventory")]
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int QuantityOnHand { get; set; } = 0;

        public int Quantity { get; set; } = 0; // Keeping for backward compatibility

        public int ReorderLevel { get; set; } = 0;

        public int ReorderPoint { get; set; } = 0; // Keeping for backward compatibility

        public int MaxStockLevel { get; set; } = 100;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}