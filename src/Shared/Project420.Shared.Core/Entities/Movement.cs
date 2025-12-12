using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Movement record for the inventory ledger (Option A - Movement Architecture).
/// Each record represents a single stock movement event.
/// </summary>
/// <remarks>
/// Architecture Principles:
/// - Movement is the SOURCE OF TRUTH for SOH (Stock on Hand)
/// - SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
/// - SOH is NEVER stored as a snapshot - always calculated from movements
/// - Movements are immutable once created (soft delete only)
///
/// Relationship to TransactionDetail:
/// - Each TransactionDetail can generate one or more Movement records
/// - Movement.DetailId links back to TransactionDetail.Id
/// - This enables full traceability: Transaction -> Detail -> Movement -> SOH
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Full audit trail via AuditableEntity
/// - Batch and serial number tracking for seed-to-sale
/// - Weight tracking for reconciliation
/// - Movement reason documented for compliance reporting
///
/// Performance Considerations:
/// - Indexed on ProductId + TransactionDate for SOH queries
/// - Indexed on BatchNumber and SerialNumber for traceability queries
/// - Use filtered indexes (WHERE NOT IsDeleted) for active records
/// </remarks>
public class Movement : AuditableEntity
{
    // ============================================================
    // PRODUCT REFERENCE
    // ============================================================

    /// <summary>
    /// ID of the product (FK to Product in Management module).
    /// </summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Product SKU (denormalized for query performance and historical accuracy).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>
    /// Product name (denormalized for reporting).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    // ============================================================
    // MOVEMENT CLASSIFICATION
    // ============================================================

    /// <summary>
    /// Type of movement (for categorization and reporting).
    /// </summary>
    /// <remarks>
    /// Descriptive name like "Sale", "GRV", "Production Output".
    /// Derived from TransactionType but stored for query efficiency.
    /// </remarks>
    [Required]
    [MaxLength(50)]
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// Direction of movement (IN increases SOH, OUT decreases SOH).
    /// </summary>
    [Required]
    public MovementDirection Direction { get; set; }

    // ============================================================
    // QUANTITIES
    // ============================================================

    /// <summary>
    /// Quantity of product moved (always positive).
    /// Direction field determines whether this adds or subtracts from SOH.
    /// </summary>
    /// <remarks>
    /// Uses decimal(18,4) to support fractional quantities (e.g., grams of cannabis).
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Mass in grams (for weight-based products like cannabis).
    /// </summary>
    /// <remarks>
    /// SAHPRA Compliance: Cannabis products must track weight for reconciliation.
    /// May differ from Quantity if product has standard weight per unit.
    /// </remarks>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Mass { get; set; } = 0;

    /// <summary>
    /// Monetary value of this movement (for COGS and valuation).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } = 0;

    // ============================================================
    // CANNABIS TRACEABILITY (SAHPRA Compliance)
    // ============================================================

    /// <summary>
    /// Batch number for seed-to-sale traceability.
    /// </summary>
    /// <remarks>
    /// SAHPRA Compliance: Required for all cannabis products.
    /// Format: 16 digits - Site(2) + Type(2) + YYYYMMDD(8) + Sequence(4)
    /// </remarks>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Serial number for individual unit tracking.
    /// </summary>
    /// <remarks>
    /// Used for serialized products (pre-rolls, vape cartridges).
    /// Full format: 28 digits. Short format: 13 digits for barcode.
    /// </remarks>
    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    // ============================================================
    // SOURCE TRANSACTION REFERENCE
    // ============================================================

    /// <summary>
    /// Type of transaction that created this movement.
    /// Links to TransactionDetail.TransactionType.
    /// </summary>
    [Required]
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// ID of the transaction header that created this movement.
    /// Links to TransactionDetail.HeaderId.
    /// </summary>
    [Required]
    public int HeaderId { get; set; }

    /// <summary>
    /// ID of the transaction detail that created this movement.
    /// Links to TransactionDetail.Id.
    /// </summary>
    [Required]
    public int DetailId { get; set; }

    // ============================================================
    // MOVEMENT METADATA
    // ============================================================

    /// <summary>
    /// Human-readable reason for this movement.
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Reason is REQUIRED for adjustments, waste, and returns.
    /// Automatically generated for sales and GRVs.
    /// Example: "Sale transaction SALE-20251211-001", "Waste - mold found on batch"
    /// </remarks>
    [Required]
    [MaxLength(500)]
    public string MovementReason { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of the transaction (business date, not audit date).
    /// </summary>
    /// <remarks>
    /// May differ from CreatedAt if transaction is back-dated (with proper authorization).
    /// Used for SOH as-of-date calculations.
    /// </remarks>
    [Required]
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// User who performed the transaction.
    /// </summary>
    /// <remarks>
    /// May differ from CreatedBy if movement is system-generated.
    /// Used for audit trail and authorization verification.
    /// </remarks>
    [MaxLength(100)]
    public string? UserId { get; set; }

    /// <summary>
    /// Location/warehouse ID where movement occurred.
    /// </summary>
    /// <remarks>
    /// Used for multi-location inventory tracking.
    /// NULL for single-location deployments.
    /// </remarks>
    public int? LocationId { get; set; }

    /// <summary>
    /// Location name (denormalized for reporting).
    /// </summary>
    [MaxLength(200)]
    public string? LocationName { get; set; }
}
