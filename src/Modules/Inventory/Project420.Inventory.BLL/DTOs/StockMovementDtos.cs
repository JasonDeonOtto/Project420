using Project420.Inventory.Models.Enums;

namespace Project420.Inventory.BLL.DTOs;

public class CreateStockMovementDto
{
    public string MovementNumber { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// Product ID for FK reference
    /// </summary>
    public int ProductId { get; set; }

    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Quantity moved (positive = increase, use with appropriate MovementType)
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Weight in grams (for cannabis products - SAHPRA compliance)
    /// </summary>
    public decimal? WeightGrams { get; set; }

    public decimal? UnitCost { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReferenceType { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? UserId { get; set; }
    public int? LocationId { get; set; }
}

/// <summary>
/// Result of creating a stock movement
/// </summary>
public class StockMovementResultDto
{
    public int StockMovementId { get; set; }
    public int? MovementId { get; set; }
    public string MovementNumber { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UpdateStockMovementDto
{
    public int Id { get; set; }
    public StockMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Location { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public string MovementNumber { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Location { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
