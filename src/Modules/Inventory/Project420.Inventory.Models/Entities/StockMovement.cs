using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Inventory.Models.Enums;

namespace Project420.Inventory.Models.Entities
{
    /// <summary>
    /// Represents any movement of stock (in or out)
    /// </summary>
    /// <remarks>
    /// SAHPRA/SARS Compliance:
    /// - ALL stock movements must be tracked
    /// - Batch numbers must be recorded for seed-to-sale traceability
    /// - Weight discrepancies must be documented
    ///
    /// This is the universal stock ledger - every transaction that affects inventory
    /// creates a StockMovement record.
    /// </remarks>
    public class StockMovement : AuditableEntity
    {
        // ============================================================
        // MOVEMENT IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Unique movement reference number
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Movement Number")]
        public string MovementNumber { get; set; } = string.Empty;

        /// <summary>
        /// Type of stock movement
        /// </summary>
        [Required]
        [Display(Name = "Movement Type")]
        public StockMovementType MovementType { get; set; }

        /// <summary>
        /// Date/time of movement
        /// </summary>
        [Required]
        [Display(Name = "Movement Date")]
        public DateTime MovementDate { get; set; }

        // ============================================================
        // PRODUCT & BATCH TRACEABILITY
        // ============================================================

        /// <summary>
        /// Product SKU (reference to Management.Product)
        /// </summary>
        /// <remarks>
        /// Cross-module reference via SKU string
        /// Links to Product in Management module
        /// </remarks>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Product SKU")]
        public string ProductSKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name (denormalized for historical accuracy)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Batch number from production
        /// </summary>
        /// <remarks>
        /// CRITICAL for seed-to-sale traceability
        /// Links back to ProductionBatch → HarvestBatch → Plants
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        // ============================================================
        // QUANTITY & WEIGHT
        // ============================================================

        /// <summary>
        /// Quantity moved (positive = increase, negative = decrease)
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - GoodsReceived: +100
        /// - Sale: -1
        /// - Adjustment: +5 or -5
        /// </remarks>
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Weight in grams (for cannabis products)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Weight must be tracked for cannabis
        /// Used for reconciliation and compliance reporting
        /// </remarks>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Weight (g)")]
        public decimal? WeightGrams { get; set; }

        // ============================================================
        // LOCATION TRACKING
        // ============================================================

        /// <summary>
        /// Source location (where stock came from)
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "From Location")]
        public string? FromLocation { get; set; }

        /// <summary>
        /// Destination location (where stock went to)
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "To Location")]
        public string? ToLocation { get; set; }

        // ============================================================
        // REFERENCE DOCUMENTS
        // ============================================================

        /// <summary>
        /// Reference to source transaction (POS sale, GRV, etc.)
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Type of reference document
        /// </summary>
        /// <remarks>
        /// Examples: "POS Sale", "GRV", "Transfer", "Adjustment", "Production Receipt"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Reference Type")]
        public string? ReferenceType { get; set; }

        // ============================================================
        // COST/VALUE
        // ============================================================

        /// <summary>
        /// Unit cost at time of movement
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Cost")]
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Total value of movement
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Value")]
        public decimal? TotalValue { get; set; }

        // ============================================================
        // NOTES & REASON
        // ============================================================

        /// <summary>
        /// Reason for movement (especially for adjustments/waste)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Waste must be documented with reason
        /// Examples: "Damaged during transport", "Mold found", "Count variance"
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
