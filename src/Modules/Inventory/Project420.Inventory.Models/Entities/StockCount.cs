using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.Models.Entities
{
    /// <summary>
    /// Represents a physical stock count (cycle count or full inventory)
    /// </summary>
    /// <remarks>
    /// SAHPRA/SARS Compliance:
    /// - Regular stock counts required for cannabis inventory
    /// - Variances must be investigated and documented
    /// - Annual physical inventory typically required
    ///
    /// Best practices:
    /// - Cycle counts: Monthly or quarterly for high-value items
    /// - Full counts: Annually or bi-annually
    /// - Count variance tolerance: ±2% acceptable, >2% requires investigation
    /// </remarks>
    public class StockCount : AuditableEntity
    {
        /// <summary>
        /// Unique count reference number
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Count Number")]
        public string CountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Count date
        /// </summary>
        [Required]
        [Display(Name = "Count Date")]
        public DateTime CountDate { get; set; }

        /// <summary>
        /// Type of count
        /// </summary>
        /// <remarks>
        /// Values: "Cycle Count", "Full Inventory", "Spot Check", "Year-End Count"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Count Type")]
        public string? CountType { get; set; }

        /// <summary>
        /// Product SKU being counted
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
        /// System quantity before count
        /// </summary>
        [Required]
        [Display(Name = "System Quantity")]
        public int SystemQuantity { get; set; }

        /// <summary>
        /// Actual physical quantity counted
        /// </summary>
        [Required]
        [Display(Name = "Counted Quantity")]
        public int CountedQuantity { get; set; }

        /// <summary>
        /// Variance (Counted - System)
        /// </summary>
        [Display(Name = "Variance")]
        public int Variance { get; set; }

        /// <summary>
        /// Who performed the count
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Counted By")]
        public string CountedBy { get; set; } = string.Empty;

        /// <summary>
        /// Who verified/approved the count
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Verified By")]
        public string? VerifiedBy { get; set; }

        /// <summary>
        /// Location where count occurred
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        /// <summary>
        /// Whether variance has been investigated
        /// </summary>
        [Display(Name = "Variance Investigated")]
        public bool VarianceInvestigated { get; set; } = false;

        /// <summary>
        /// Explanation of variance
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Variance Reason")]
        public string? VarianceReason { get; set; }

        /// <summary>
        /// Whether adjustment was created for this count
        /// </summary>
        [Display(Name = "Adjustment Created")]
        public bool AdjustmentCreated { get; set; } = false;

        /// <summary>
        /// Reference to adjustment (if created)
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Adjustment Number")]
        public string? AdjustmentNumber { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
