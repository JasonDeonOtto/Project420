using System.ComponentModel.DataAnnotations;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.BLL.DTOs;

// ============================================================
// GRV DTOs (Phase 12 - Purchasing Workflow)
// ============================================================

/// <summary>
/// DTO for creating a new GRV.
/// </summary>
public class CreateGrvDto
{
    /// <summary>Supplier ID (optional).</summary>
    public int? SupplierId { get; set; }

    /// <summary>Supplier name.</summary>
    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>Supplier's invoice/delivery note reference.</summary>
    [MaxLength(100)]
    public string? SupplierReference { get; set; }

    /// <summary>Related Purchase Order ID (optional).</summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>Purchase Order number (optional).</summary>
    [MaxLength(50)]
    public string? PurchaseOrderNumber { get; set; }

    /// <summary>Date goods were received.</summary>
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    /// <summary>User receiving the goods.</summary>
    [Required]
    [MaxLength(100)]
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>Location ID where goods are received.</summary>
    public int? LocationId { get; set; }

    /// <summary>Location name.</summary>
    [MaxLength(200)]
    public string? LocationName { get; set; }

    /// <summary>Notes about the delivery.</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>Line items being received.</summary>
    [Required]
    public List<GrvLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for a GRV line item.
/// </summary>
public class GrvLineDto
{
    /// <summary>Product ID.</summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>Product SKU (for validation).</summary>
    [Required]
    [MaxLength(50)]
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>Product name (denormalized).</summary>
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity ordered (from PO).</summary>
    [Range(0, 1000000)]
    public decimal QuantityOrdered { get; set; }

    /// <summary>Quantity actually received.</summary>
    [Required]
    [Range(0.0001, 1000000)]
    public decimal QuantityReceived { get; set; }

    /// <summary>Unit cost (excluding VAT).</summary>
    [Required]
    [Range(0, 10000000)]
    public decimal UnitCost { get; set; }

    /// <summary>Batch number for this line (cannabis compliance).</summary>
    [MaxLength(50)]
    public string? BatchNumber { get; set; }

    /// <summary>Serial number (if applicable).</summary>
    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    /// <summary>Weight in grams (for cannabis products).</summary>
    [Range(0, 1000000)]
    public decimal? WeightGrams { get; set; }

    /// <summary>Expiry date (if applicable).</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Notes about this line.</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for GRV display/reading.
/// </summary>
public class GrvDto
{
    /// <summary>GRV ID.</summary>
    public int Id { get; set; }

    /// <summary>GRV number.</summary>
    public string GrvNumber { get; set; } = string.Empty;

    /// <summary>Supplier ID.</summary>
    public int? SupplierId { get; set; }

    /// <summary>Supplier name.</summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>Supplier's reference number.</summary>
    public string? SupplierReference { get; set; }

    /// <summary>Purchase Order ID (if linked).</summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>Purchase Order number.</summary>
    public string? PurchaseOrderNumber { get; set; }

    /// <summary>Date received.</summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>User who received.</summary>
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>Location ID.</summary>
    public int? LocationId { get; set; }

    /// <summary>Location name.</summary>
    public string? LocationName { get; set; }

    /// <summary>Total lines.</summary>
    public int TotalLines { get; set; }

    /// <summary>Total quantity.</summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>Total cost (excl VAT).</summary>
    public decimal TotalCost { get; set; }

    /// <summary>VAT amount.</summary>
    public decimal VatAmount { get; set; }

    /// <summary>Total with VAT.</summary>
    public decimal TotalWithVat { get; set; }

    /// <summary>Current status.</summary>
    public GrvStatus Status { get; set; }

    /// <summary>Status as string.</summary>
    public string StatusName => Status.ToString();

    /// <summary>User who approved.</summary>
    public string? ApprovedBy { get; set; }

    /// <summary>Approval date.</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Discrepancy notes.</summary>
    public string? DiscrepancyNotes { get; set; }

    /// <summary>Whether batch numbers are assigned.</summary>
    public bool BatchNumbersAssigned { get; set; }

    /// <summary>Whether lab certificates are verified.</summary>
    public bool LabCertificatesVerified { get; set; }

    /// <summary>Line items.</summary>
    public List<GrvLineDetailDto> Lines { get; set; } = new();

    /// <summary>Created date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Created by.</summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for GRV line detail display.
/// </summary>
public class GrvLineDetailDto
{
    /// <summary>Line ID.</summary>
    public int Id { get; set; }

    /// <summary>Product ID.</summary>
    public int ProductId { get; set; }

    /// <summary>Product SKU.</summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity ordered.</summary>
    public decimal QuantityOrdered { get; set; }

    /// <summary>Quantity received.</summary>
    public decimal QuantityReceived { get; set; }

    /// <summary>Variance (received - ordered).</summary>
    public decimal Variance => QuantityReceived - QuantityOrdered;

    /// <summary>Whether there's a variance.</summary>
    public bool HasVariance => Variance != 0;

    /// <summary>Unit cost.</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Line total (excl VAT).</summary>
    public decimal LineTotal => QuantityReceived * UnitCost;

    /// <summary>Batch number.</summary>
    public string? BatchNumber { get; set; }

    /// <summary>Serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Weight in grams.</summary>
    public decimal? WeightGrams { get; set; }

    /// <summary>Expiry date.</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for assigning batch numbers to GRV lines.
/// </summary>
public class AssignBatchNumbersDto
{
    /// <summary>GRV ID.</summary>
    [Required]
    public int GrvId { get; set; }

    /// <summary>
    /// Dictionary mapping line index to batch number.
    /// Key: Line index (0-based), Value: Batch number to assign.
    /// </summary>
    [Required]
    public Dictionary<int, string> LineBatchNumbers { get; set; } = new();
}

/// <summary>
/// DTO for approving a GRV.
/// </summary>
public class ApproveGrvDto
{
    /// <summary>GRV ID to approve.</summary>
    [Required]
    public int GrvId { get; set; }

    /// <summary>User approving the GRV.</summary>
    [Required]
    [MaxLength(100)]
    public string ApprovedBy { get; set; } = string.Empty;

    /// <summary>Optional approval notes.</summary>
    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }
}

/// <summary>
/// Result of GRV approval.
/// </summary>
public class GrvApprovalResult
{
    /// <summary>Whether approval was successful.</summary>
    public bool Success { get; set; }

    /// <summary>GRV ID.</summary>
    public int GrvId { get; set; }

    /// <summary>GRV number.</summary>
    public string GrvNumber { get; set; } = string.Empty;

    /// <summary>Number of movements created.</summary>
    public int MovementsCreated { get; set; }

    /// <summary>Error message (if failed).</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Validation errors (if any).</summary>
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// Filter criteria for GRV queries.
/// </summary>
public class GrvFilterDto
{
    /// <summary>Filter by status.</summary>
    public GrvStatus? Status { get; set; }

    /// <summary>Filter by supplier ID.</summary>
    public int? SupplierId { get; set; }

    /// <summary>Filter by supplier name (contains).</summary>
    public string? SupplierName { get; set; }

    /// <summary>Filter by date range start.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Filter by date range end.</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>Filter by location.</summary>
    public int? LocationId { get; set; }

    /// <summary>Search by GRV number or supplier reference.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Page size.</summary>
    public int PageSize { get; set; } = 20;
}
