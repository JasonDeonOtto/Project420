using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.Models.Entities
{
    /// <summary>
    /// Represents an adjustment to stock levels (corrections, damage, shrinkage, etc.)
    /// </summary>
    /// <remarks>
    /// SAHPRA/SARS Compliance:
    /// - All adjustments must be documented with reason
    /// - Negative adjustments (waste) must be tracked
    /// - Discrepancies must be investigated and explained
    ///
    /// Common adjustment reasons:
    /// - Stock count variance
    /// - Damaged product
    /// - Expired product destroyed
    /// - Theft/shrinkage
    /// - Data entry error correction
    /// </remarks>
    public class StockAdjustment : AuditableEntity
    {
        /// <summary>
        /// Unique adjustment number
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Adjustment Number")]
        public string AdjustmentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Adjustment date
        /// </summary>
        [Required]
        [Display(Name = "Adjustment Date")]
        public DateTime AdjustmentDate { get; set; }

        /// <summary>
        /// Product SKU being adjusted
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Product SKU")]
        public string ProductSKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name (denormalized)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Batch number (if applicable)
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Adjustment quantity (positive = increase, negative = decrease)
        /// </summary>
        [Required]
        [Display(Name = "Adjustment Quantity")]
        public int AdjustmentQuantity { get; set; }

        /// <summary>
        /// System quantity before adjustment
        /// </summary>
        [Display(Name = "Before Quantity")]
        public int? BeforeQuantity { get; set; }

        /// <summary>
        /// System quantity after adjustment
        /// </summary>
        [Display(Name = "After Quantity")]
        public int? AfterQuantity { get; set; }

        /// <summary>
        /// Reason for adjustment
        /// </summary>
        /// <remarks>
        /// REQUIRED for compliance
        /// Examples: "Stock count variance", "Damaged - water leak", "Expired product destroyed"
        /// </remarks>
        [Required]
        [MaxLength(500)]
        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Who authorized this adjustment
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Authorized By")]
        public string? AuthorizedBy { get; set; }

        /// <summary>
        /// Location where adjustment occurred
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        /// <summary>
        /// Cost impact of adjustment
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost Impact")]
        public decimal? CostImpact { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
