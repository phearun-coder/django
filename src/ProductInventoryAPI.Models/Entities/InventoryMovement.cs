namespace ProductInventoryAPI.Models.Entities;

/// <summary>
/// Represents an inventory movement record for tracking stock changes
/// </summary>
public class InventoryMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty; // Inbound, Outbound, Adjustment, Transfer, Sale, Purchase, Return
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User? User { get; set; }
}