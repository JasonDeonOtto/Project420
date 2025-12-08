namespace Project420.Inventory.BLL.DTOs;

public class CreateStockAdjustmentDto
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int AdjustmentQuantity { get; set; }
    public int? BeforeQuantity { get; set; }
    public int? AfterQuantity { get; set; }
    public string? Reason { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockAdjustmentDto
{
    public int Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public int AdjustmentQuantity { get; set; }
    public int? BeforeQuantity { get; set; }
    public int? AfterQuantity { get; set; }
    public string? Reason { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class StockAdjustmentDto
{
    public int Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int AdjustmentQuantity { get; set; }
    public int? BeforeQuantity { get; set; }
    public int? AfterQuantity { get; set; }
    public string? Reason { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
