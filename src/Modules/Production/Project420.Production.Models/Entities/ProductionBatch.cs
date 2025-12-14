using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Production.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.Production.Models.Entities
{
    /// <summary>
    /// Represents a production batch being processed from harvest to retail-ready product
    /// </summary>
    /// <remarks>
    /// ⚠️ CRITICAL TRACEABILITY LINK ⚠️
    ///
    /// This entity connects Cultivation (HarvestBatch) → Production → Inventory
    ///
    /// SAHPRA GMP Compliance:
    /// - Each production batch must be uniquely identified
    /// - Processing steps must be documented
    /// - Quality control checks required
    /// - Batch testing (COA) mandatory before release
    ///
    /// Workflow:
    /// 1. HarvestBatch → ProductionBatch (link via foreign key or batch number)
    /// 2. ProductionBatch → Processing Steps (drying, curing, trimming, packaging)
    /// 3. ProductionBatch → Quality Control checks
    /// 4. ProductionBatch → Lab Testing (COA)
    /// 5. ProductionBatch → Inventory (ready for sale)
    ///
    /// This ensures seed-to-sale traceability from plant → harvest → production → retail
    /// </remarks>
    public class ProductionBatch : AuditableEntity
    {
        // ============================================================
        // UNIQUE IDENTIFICATION (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Unique production batch number (e.g., "PB-2024-001", "PROD-HB001-001")
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Each production batch MUST have unique ID
        /// Best practice: Link to HarvestBatch number
        /// Example: "PROD-HB-GC001-20241201-A"
        /// </remarks>
        [Required(ErrorMessage = "Production batch number is required")]
        [MaxLength(100)]
        [Display(Name = "Production Batch Number")]
        public string BatchNumber { get; set; } = string.Empty;

        /// <summary>
        /// Batch name/description
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Batch Name")]
        public string? Name { get; set; }

        // ============================================================
        // HARVEST BATCH LINK (TRACEABILITY)
        // ============================================================

        /// <summary>
        /// Reference harvest batch number from Cultivation module
        /// </summary>
        /// <remarks>
        /// CRITICAL for seed-to-sale traceability
        /// Links production back to specific harvest (and plants)
        /// Required for SAHPRA compliance audits
        ///
        /// Note: This is a string reference across modules
        /// In full implementation, consider cross-module foreign key
        /// </remarks>
        [Required(ErrorMessage = "Harvest batch reference is required")]
        [MaxLength(100)]
        [Display(Name = "Harvest Batch Number")]
        public string HarvestBatchNumber { get; set; } = string.Empty; //needs to properly link to Harvest batches

        /// <summary>
        /// Cannabis strain being processed
        /// </summary>
        /// <remarks>
        /// Inherited from HarvestBatch
        /// Stored here for historical accuracy and reporting
        /// </remarks>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Strain Name")]
        public string StrainName { get; set; } = string.Empty;

        // ============================================================
        // WEIGHT TRACKING (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Starting weight when batch entered production (grams)
        /// </summary>
        /// <remarks>
        /// Typically the dry weight from harvest batch
        /// Used as baseline for yield calculations and waste tracking
        /// </remarks>
        [Required]
        [Range(0.01, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Starting Weight (g)")]
        public decimal StartingWeightGrams { get; set; }

        /// <summary>
        /// Current weight during processing (grams)
        /// </summary>
        /// <remarks>
        /// Updated after each processing step
        /// Decreases due to trimming, waste removal
        /// </remarks>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Current Weight (g)")]
        public decimal? CurrentWeightGrams { get; set; }

        /// <summary>
        /// Final packaged weight (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA/SARS Compliance: Final weight for inventory and tax
        /// This is the sellable product weight
        /// Used for inventory valuation and sales tracking
        /// </remarks>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Final Weight (g)")]
        public decimal? FinalWeightGrams { get; set; }

        /// <summary>
        /// Total waste/trim weight (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Waste must be tracked and documented
        /// Includes: stems, fan leaves, trim, failed QC material
        /// Used for waste reconciliation reporting
        /// </remarks>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Waste Weight (g)")]
        public decimal? WasteWeightGrams { get; set; }

        // ============================================================
        // PROCESSING DATES
        // ============================================================

        /// <summary>
        /// Date when batch started production processing
        /// </summary>
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date when batch completed all processing
        /// </summary>
        /// <remarks>
        /// Batch is complete when:
        /// - All processing steps finished
        /// - Quality control passed
        /// - Lab testing passed (COA received)
        /// - Packaging completed
        /// - Ready for inventory
        /// </remarks>
        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }

        // ============================================================
        // PROCESSING STATUS
        // ============================================================

        /// <summary>
        /// Current processing status
        /// </summary>
        /// <remarks>
        /// Workflow statuses:
        /// - "In Production" - currently being processed
        /// - "Drying" - in drying room
        /// - "Curing" - in cure jars/room
        /// - "Trimming" - being trimmed
        /// - "Quality Control" - undergoing QC checks
        /// - "Lab Testing" - awaiting/undergoing lab tests
        /// - "Packaging" - being packaged
        /// - "Completed" - ready for inventory
        /// - "Failed QC" - failed quality control
        /// - "On Hold" - processing paused
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Batch Status")]
        public ProductionBatchStatus ProductionBatchStatus { get; set; }

        /// <summary>
        /// Whether this batch is currently active
        /// </summary>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // QUALITY & LAB TESTING
        // ============================================================

        /// <summary>
        /// Whether batch passed all quality control checks
        /// </summary>
        /// <remarks>
        /// SAHPRA: Failed batches cannot proceed to sale
        /// Null = not yet tested
        /// False = failed, must be destroyed or reprocessed
        /// True = passed, can proceed to packaging
        /// </remarks>
        [Display(Name = "QC Passed")]
        public bool? QualityControlPassed { get; set; }

        /// <summary>
        /// Whether batch passed lab testing (COA)
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: Lab testing mandatory
        /// Null = not yet tested
        /// False = failed, must be destroyed
        /// True = passed, can proceed to sale
        /// </remarks>
        [Display(Name = "Lab Test Passed")]
        public bool? LabTestPassed { get; set; }

        /// <summary>
        /// THC percentage from lab test results
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "THC %")]
        public string? THCPercentage { get; set; }

        /// <summary>
        /// CBD percentage from lab test results
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "CBD %")]
        public string? CBDPercentage { get; set; }

        // ============================================================
        // PACKAGING INFORMATION
        // ============================================================

        /// <summary>
        /// Number of units/packages produced from this batch
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - 100 x 3.5g packages
        /// - 50 x 7g packages
        /// - 25 x 28g packages
        /// </remarks>
        [Range(0, 100000)]
        [Display(Name = "Units Packaged")]
        public int? UnitsPackaged { get; set; }

        /// <summary>
        /// Package size (e.g., "3.5g", "7g", "28g")
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "Package Size")]
        public string? PackageSize { get; set; }

        /// <summary>
        /// Packaging date
        /// </summary>
        [Display(Name = "Packaging Date")]
        public DateTime? PackagingDate { get; set; }

        // ============================================================
        // NOTES
        // ============================================================

        /// <summary>
        /// Notes about processing, quality, issues
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of processing steps performed on this batch
        /// </summary>
        public virtual ICollection<ProcessingStep> ProcessingSteps { get; set; } = new List<ProcessingStep>();

        /// <summary>
        /// Collection of quality control checks performed
        /// </summary>
        public virtual ICollection<QualityControl> QualityControls { get; set; } = new List<QualityControl>();

        /// <summary>
        /// Collection of lab tests performed
        /// </summary>
        public virtual ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
    }
}
