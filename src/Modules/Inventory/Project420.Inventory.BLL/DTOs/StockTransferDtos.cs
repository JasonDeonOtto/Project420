namespace Project420.Inventory.BLL.DTOs;

/// <summary>
/// DTO for creating a stock transfer with line items.
/// </summary>
public class CreateStockTransferDto
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public string? RequestedBy { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Line items being transferred
    /// </summary>
    public List<StockTransferLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Line item in a stock transfer
/// </summary>
public class StockTransferLineDto
{
    public int ProductId { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public int Quantity { get; set; }
    public decimal? WeightGrams { get; set; }
    public decimal? UnitCost { get; set; }
}

public class UpdateStockTransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? AuthorizedBy { get; set; }
    public string? Notes { get; set; }
}

public class StockTransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? RequestedBy { get; set; }
    public string? AuthorizedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Result of a stock transfer operation
/// </summary>
public class StockTransferResultDto
{
    public int TransferId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public int MovementsCreated { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for approving a transfer and generating TransferOut movements
/// </summary>
public class ApproveStockTransferDto
{
    public int TransferId { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }

    /// <summary>
    /// Line items being shipped (may differ from original if partial shipment)
    /// </summary>
    public List<StockTransferLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for completing/receiving a transfer and generating TransferIn movements
/// </summary>
public class CompleteStockTransferDto
{
    public int TransferId { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime CompletionDate { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Line items received (may differ from shipped if discrepancies)
    /// </summary>
    public List<StockTransferLineDto> Lines { get; set; } = new();
}
