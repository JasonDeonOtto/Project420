using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Unified transaction detail for all transaction types (Option A - Movement Architecture).
/// Links to header via HeaderId + TransactionType discriminator.
/// </summary>
/// <remarks>
/// Design Rationale:
/// - Single table stores all transaction line items regardless of transaction type
/// - TransactionType enum acts as discriminator to identify which header table the detail belongs to
/// - Enables consistent movement generation from all transaction types
/// - Simplifies reporting and querying across transaction types
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Batch numbers tracked for seed-to-sale traceability
/// - Serial numbers tracked for individual unit traceability (packaging)
/// - Full audit trail via AuditableEntity inheritance
/// - VAT breakdown for SARS compliance
///
/// Movement Generation:
/// - Each TransactionDetail generates corresponding Movement record(s)
/// - Movement direction determined by TransactionType
/// - SOH calculated from sum of movements (never stored directly)
/// </remarks>
public class TransactionDetail : AuditableEntity
{
    // ============================================================
    // HEADER REFERENCE (Discriminated Union Pattern)
    // ============================================================

    /// <summary>
    /// ID of the parent transaction header record.
    /// The specific header table is determined by TransactionType.
    /// </summary>
    /// <remarks>
    /// Example mappings:
    /// - TransactionType.Sale → RetailTransactionHeader.Id
    /// - TransactionType.GRV → GoodsReceivedVoucher.Id
    /// - TransactionType.ProductionOutput → ProductionBatch.Id
    /// </remarks>
    [Required]
    public int HeaderId { get; set; }

    /// <summary>
    /// Type of transaction this detail belongs to.
    /// Acts as discriminator to identify the header table.
    /// </summary>
    [Required]
    public TransactionType TransactionType { get; set; }

    // ============================================================
    // PRODUCT REFERENCE
    // ============================================================

    /// <summary>
    /// ID of the product (FK to Product in Management module).
    /// </summary>
    /// <remarks>
    /// Cross-module reference to Management.Product entity.
    /// </remarks>
    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Product SKU (denormalized for historical accuracy and reporting).
    /// </summary>
    /// <remarks>
    /// Denormalized because product SKU may change over time,
    /// but transaction details must reflect SKU at time of transaction.
    /// </remarks>
    [Required]
    [MaxLength(50)]
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>
    /// Product name (denormalized for historical accuracy and reporting).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    // ============================================================
    // QUANTITY & PRICING
    // ============================================================

    /// <summary>
    /// Quantity of product in this line item.
    /// Always positive - direction determined by TransactionType.
    /// </summary>
    /// <remarks>
    /// Uses decimal for weight-based products (e.g., cannabis flower in grams).
    /// For unit-based products, will be whole numbers.
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit price (VAT-inclusive for retail, VAT-exclusive for wholesale).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Discount amount applied to this line item.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// VAT amount for this line item.
    /// </summary>
    /// <remarks>
    /// SARS Compliance: VAT must be calculated and tracked separately.
    /// SA VAT Rate: 15% (as of 2025)
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal VATAmount { get; set; }

    /// <summary>
    /// Total amount for this line item (Quantity * UnitPrice - DiscountAmount).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Cost price at time of transaction (for margin calculations and COGS).
    /// </summary>
    /// <remarks>
    /// Optional for sales transactions.
    /// Required for GRV and production transactions.
    /// </remarks>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostPrice { get; set; }

    // ============================================================
    // CANNABIS TRACEABILITY (SAHPRA Compliance)
    // ============================================================

    /// <summary>
    /// Batch number for seed-to-sale traceability.
    /// </summary>
    /// <remarks>
    /// SAHPRA Compliance: Batch number MUST be tracked for all cannabis products.
    /// Links back to ProductionBatch -> HarvestBatch -> Plants.
    /// Format: Site(2) + Type(2) + YYYYMMDD(8) + Sequence(4) = 16 digits
    /// </remarks>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Serial number for individual unit tracking (packaged products).
    /// </summary>
    /// <remarks>
    /// Used for serialized products (e.g., pre-rolls, vape cartridges).
    /// Full format: 28 digits with embedded traceability data.
    /// Short format: 13 digits for barcode scanning.
    /// </remarks>
    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Weight in grams (for cannabis products).
    /// </summary>
    /// <remarks>
    /// SAHPRA Compliance: Weight must be tracked for cannabis products.
    /// Used for weight-based inventory reconciliation.
    /// </remarks>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? WeightGrams { get; set; }

    // ============================================================
    // NOTES & METADATA
    // ============================================================

    /// <summary>
    /// Additional notes for this line item.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
