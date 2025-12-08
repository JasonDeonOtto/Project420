using Project420.Inventory.Models.Enums;

namespace Project420.Inventory.BLL.DTOs;

public class CreateStockMovementDto
{
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
