using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.Models.Entities;

/// <summary>
/// Goods Received Voucher header - represents stock received from a supplier.
/// Links to unified TransactionDetails via HeaderId + TransactionType.GRV pattern.
/// </summary>
/// <remarks>
/// Movement Architecture (Option A):
/// - GRV creates IN movements when approved
/// - All stock received is tracked via unified TransactionDetails
/// - MovementService.GenerateMovementsAsync(TransactionType.GRV, GrvId) creates movements
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Must track supplier details for traceability
/// - All received stock must have batch numbers assigned
/// - Lab test certificates required for cannabis products
/// - Full audit trail for seed-to-sale tracking
/// </remarks>
public class GrvHeader : AuditableEntity
{
    // ============================================================
    // IDENTITY
    // ============================================================

    /// <summary>
    /// Unique GRV number (auto-generated).
    /// Format: GRV-YYYYMMDD-XXX
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string GrvNumber { get; set; } = string.Empty;

    // ============================================================
    // SUPPLIER INFORMATION
    // ============================================================

    /// <summary>
    /// Supplier identifier (FK to future Supplier table).
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Supplier name (denormalized for quick reference).
    /// </summary>
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Supplier invoice/delivery note number.
    /// </summary>
    [MaxLength(100)]
    public string? SupplierReference { get; set; }

    // ============================================================
    // PURCHASE ORDER REFERENCE
    // ============================================================

    /// <summary>
    /// Related Purchase Order ID (optional, for three-way matching).
    /// </summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>
    /// Purchase Order number (denormalized for quick reference).
    /// </summary>
    [MaxLength(50)]
    public string? PurchaseOrderNumber { get; set; }

    // ============================================================
    // DATES
    // ============================================================

    /// <summary>
    /// Date the goods were received.
    /// </summary>
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected delivery date (from PO).
    /// </summary>
    public DateTime? ExpectedDate { get; set; }

    // ============================================================
    // RECEIVING DETAILS
    // ============================================================

    /// <summary>
    /// User who received the goods.
    /// </summary>
    [MaxLength(100)]
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>
    /// Location where goods were received.
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Location name (denormalized).
    /// </summary>
    [MaxLength(200)]
    public string? LocationName { get; set; }

    // ============================================================
    // TOTALS
    // ============================================================

    /// <summary>
    /// Total number of line items.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Total quantity of all items received.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// Total value at cost (excluding VAT).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    /// <summary>
    /// VAT amount on this GRV.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Total value including VAT.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalWithVat { get; set; }

    // ============================================================
    // STATUS
    // ============================================================

    /// <summary>
    /// Current status of the GRV.
    /// </summary>
    public GrvStatus Status { get; set; } = GrvStatus.Draft;

    /// <summary>
    /// User who approved the GRV (when status = Approved).
    /// </summary>
    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date the GRV was approved.
    /// </summary>
    public DateTime? ApprovedDate { get; set; }

    // ============================================================
    // NOTES
    // ============================================================

    /// <summary>
    /// General notes about this delivery.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Any discrepancies noted (quantity, damage, etc.).
    /// </summary>
    [MaxLength(2000)]
    public string? DiscrepancyNotes { get; set; }

    // ============================================================
    // CANNABIS COMPLIANCE (SAHPRA)
    // ============================================================

    /// <summary>
    /// Whether all items have batch numbers assigned.
    /// </summary>
    public bool BatchNumbersAssigned { get; set; }

    /// <summary>
    /// Whether lab test certificates are attached/verified.
    /// </summary>
    public bool LabCertificatesVerified { get; set; }
}

/// <summary>
/// Status of a GRV throughout its lifecycle.
/// </summary>
public enum GrvStatus
{
    /// <summary>
    /// GRV is being created, not yet submitted.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// GRV is submitted for approval.
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// GRV is approved - movements can be generated.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// GRV is rejected.
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// GRV is cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// GRV is completed - movements have been generated.
    /// </summary>
    Completed = 5
}
