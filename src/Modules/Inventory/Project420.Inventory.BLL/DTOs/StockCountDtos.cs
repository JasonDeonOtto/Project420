namespace Project420.Inventory.BLL.DTOs;

public class CreateStockCountDto
{
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public string? CountType { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public string CountedBy { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockCountDto
{
    public int Id { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public string? CountType { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public string CountedBy { get; set; } = string.Empty;
    public string? VerifiedBy { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class StockCountDto
{
    public int Id { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public string? CountType { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int VarianceQuantity => ActualQuantity - SystemQuantity; // Calculated property
    public decimal VariancePercentage => SystemQuantity > 0
        ? Math.Round((decimal)(ActualQuantity - SystemQuantity) / SystemQuantity * 100, 2)
        : 0;
    public string CountedBy { get; set; } = string.Empty;
    public string? VerifiedBy { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
