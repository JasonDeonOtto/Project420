using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Cultivation.Models.Entities
{
    /// <summary>
    /// Represents a batch of harvested plants ready for processing
    /// </summary>
    /// <remarks>
    /// ⚠️ CRITICAL TRACEABILITY LINK ⚠️
    ///
    /// This entity bridges Cultivation → Production modules
    ///
    /// SAHPRA Compliance Requirements:
    /// - Each harvest batch must be uniquely identified
    /// - Must link back to individual plants (seed-to-sale)
    /// - Harvest date and weight must be recorded
    /// - Batch must be tracked through processing to retail
    ///
    /// GMP (Good Manufacturing Practice):
    /// - Batches must be kept separate to prevent cross-contamination
    /// - Each batch requires Certificate of Analysis (COA) from lab
    /// - Batch testing results (THC/CBD, contaminants) required
    ///
    /// Workflow:
    /// 1. Plants harvested → HarvestBatch created
    /// 2. HarvestBatch → Drying/Curing (Production module)
    /// 3. HarvestBatch → Lab Testing (COA)
    /// 4. HarvestBatch → Trimming/Packaging (Production module)
    /// 5. HarvestBatch → Inventory (Stock module)
    /// 6. HarvestBatch → Retail Sales (POS module)
    ///
    /// This ensures EVERY gram sold can trace back to specific plants.
    /// </remarks>
    public class HarvestBatch : AuditableEntity
    {
        // ============================================================
        // UNIQUE IDENTIFICATION (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Unique batch number/code (e.g., "HB-2024-001", "BATCH-GC001-NOV")
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Each batch MUST have unique identifier
        /// This batch number will appear on:
        /// - Product labels
        /// - Lab test certificates (COA)
        /// - Retail receipts
        /// - Compliance reports
        ///
        /// Best practices:
        /// - Include grow cycle reference
        /// - Include harvest date
        /// - Use sequential numbering
        /// Example: "HB-GC001-20241201-A"
        /// </remarks>
        [Required(ErrorMessage = "Batch number is required for SAHPRA compliance")]
        [MaxLength(100, ErrorMessage = "Batch number cannot exceed 100 characters")]
        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; } = string.Empty;

        /// <summary>
        /// Batch name/description
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Batch Name")]
        public string? Name { get; set; }

        // ============================================================
        // GROW CYCLE REFERENCE
        // ============================================================

        /// <summary>
        /// Reference to the grow cycle this batch came from
        /// </summary>
        /// <remarks>
        /// Links harvest batch back to cultivation cycle
        /// Required for traceability and reporting
        /// </remarks>
        [Required(ErrorMessage = "Grow cycle is required")]
        [Display(Name = "Grow Cycle")]
        public int GrowCycleId { get; set; }

        /// <summary>
        /// Navigation property to GrowCycle
        /// </summary>
        [ForeignKey(nameof(GrowCycleId))]
        public virtual GrowCycle GrowCycle { get; set; } = null!;

        // ============================================================
        // STRAIN & GENETICS
        // ============================================================

        /// <summary>
        /// Cannabis strain name for this batch
        /// </summary>
        /// <remarks>
        /// Inherited from GrowCycle but stored here for historical accuracy
        /// Critical for:
        /// - Product labeling
        /// - Medical cannabis tracking
        /// - Customer preferences
        /// - Lab testing results
        /// </remarks>
        [Required(ErrorMessage = "Strain name is required")]
        [MaxLength(100)]
        [Display(Name = "Strain Name")]
        public string StrainName { get; set; } = string.Empty;

        // ============================================================
        // HARVEST DATES (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Date when harvest began
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest start date required
        /// Used for compliance reporting and timeline tracking
        /// </remarks>
        [Required(ErrorMessage = "Harvest date is required")]
        [Display(Name = "Harvest Date")]
        public DateTime HarvestDate { get; set; }

        /// <summary>
        /// Date when drying process completed
        /// </summary>
        /// <remarks>
        /// Drying typically takes 7-14 days
        /// Dry date triggers lab testing and packaging
        /// </remarks>
        [Display(Name = "Dry Date")]
        public DateTime? DryDate { get; set; }

        /// <summary>
        /// Date when batch completed curing process
        /// </summary>
        /// <remarks>
        /// Curing typically takes 2-8 weeks
        /// Improves flavor, smoothness, and potency
        /// Cure date indicates batch is ready for sale
        /// </remarks>
        [Display(Name = "Cure Date")]
        public DateTime? CureDate { get; set; }

        // ============================================================
        // WEIGHT TRACKING (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Total wet weight at harvest (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest weight MUST be recorded
        /// Wet weight = sum of all plants in batch immediately after cutting
        /// Typically 70-80% water content
        /// Used to predict dry weight yield
        /// </remarks>
        [Required(ErrorMessage = "Wet weight is required for SAHPRA compliance")]
        [Range(0.01, 1000000, ErrorMessage = "Wet weight must be between 0.01 and 1,000,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Wet Weight (g)")]
        public decimal TotalWetWeightGrams { get; set; }

        /// <summary>
        /// Total dry weight after drying (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Dry weight used for inventory tracking
        /// Dry weight typically 20-30% of wet weight
        /// This is the weight used for:
        /// - Inventory valuation
        /// - Tax calculations (SARS)
        /// - Sales tracking
        /// - Compliance reporting
        /// </remarks>
        [Range(0, 1000000, ErrorMessage = "Dry weight must be between 0 and 1,000,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Dry Weight (g)")]
        public decimal? TotalDryWeightGrams { get; set; }

        /// <summary>
        /// Number of plants included in this batch
        /// </summary>
        /// <remarks>
        /// SAHPRA Reporting: Plant count per batch required
        /// Used for yield calculations (grams per plant)
        /// Quality control metric
        /// </remarks>
        [Required]
        [Range(1, 10000, ErrorMessage = "Plant count must be between 1 and 10,000")]
        [Display(Name = "Plant Count")]
        public int PlantCount { get; set; }

        // ============================================================
        // LAB TESTING & QUALITY CONTROL
        // ============================================================

        /// <summary>
        /// THC percentage from lab test results
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Lab testing REQUIRED for all medical cannabis
        /// Must be from ISO/IEC 17025 accredited laboratory
        /// Displayed on product labels
        /// Critical for dosing and compliance
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "THC %")]
        public string? THCPercentage { get; set; }

        /// <summary>
        /// CBD percentage from lab test results
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: CBD content required on labels
        /// Important for medical cannabis patients
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "CBD %")]
        public string? CBDPercentage { get; set; }

        /// <summary>
        /// Date when lab testing was performed
        /// </summary>
        /// <remarks>
        /// Certificate of Analysis (COA) date
        /// Required before batch can be sold
        /// </remarks>
        [Display(Name = "Lab Test Date")]
        public DateTime? LabTestDate { get; set; }

        /// <summary>
        /// Lab test certificate reference number
        /// </summary>
        /// <remarks>
        /// Links to COA document
        /// Required for SAHPRA audits
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Lab Test Certificate")]
        public string? LabTestCertificateNumber { get; set; }

        /// <summary>
        /// Whether batch passed lab testing (contaminants, pesticides, heavy metals)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Failed batches cannot be sold
        /// Must be destroyed if failed
        /// </remarks>
        [Display(Name = "Lab Test Passed")]
        public bool? LabTestPassed { get; set; }

        // ============================================================
        // PROCESSING STATUS
        // ============================================================

        /// <summary>
        /// Current processing status of the batch
        /// </summary>
        /// <remarks>
        /// Workflow stages:
        /// - "Harvested" - freshly cut
        /// - "Drying" - hanging to dry
        /// - "Curing" - in cure jars/room
        /// - "Testing" - sent to lab
        /// - "Trimming" - being trimmed/manicured
        /// - "Packaging" - being packaged for sale
        /// - "Completed" - ready for inventory/sale
        /// - "Failed" - did not pass quality control
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Processing Status")]
        public string? ProcessingStatus { get; set; }

        /// <summary>
        /// Whether this batch is currently active
        /// </summary>
        /// <remarks>
        /// Active = batch in processing or inventory
        /// Inactive = batch fully consumed/sold or destroyed
        /// </remarks>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // STORAGE & LOCATION
        // ============================================================

        /// <summary>
        /// Current storage location (drying room, cure room, inventory)
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Storage Location")]
        public string? StorageLocation { get; set; }

        /// <summary>
        /// Notes about this batch (quality, issues, observations)
        /// </summary>
        /// <remarks>
        /// GMP Compliance: Quality observations must be documented
        /// Examples:
        /// - "Excellent trichome development"
        /// - "Minor mold on 3 plants - removed from batch"
        /// - "Strong terpene profile - citrus/pine"
        /// </remarks>
        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of individual plants in this harvest batch
        /// </summary>
        /// <remarks>
        /// CRITICAL for seed-to-sale traceability
        /// Links batch back to individual plant tags
        /// Required for SAHPRA compliance audits
        /// </remarks>
        public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

        // Future: Link to Production module
        // public virtual ProductionBatch? ProductionBatch { get; set; }
    }
}
