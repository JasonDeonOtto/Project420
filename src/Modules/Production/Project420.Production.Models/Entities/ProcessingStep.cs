using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Production.Models.Enums;

namespace Project420.Production.Models.Entities
{
    /// <summary>
    /// Represents a single processing step in the production workflow
    /// </summary>
    /// <remarks>
    /// SAHPRA GMP Compliance:
    /// - Each processing step must be documented
    /// - Start/end times must be tracked
    /// - Environmental conditions recorded where applicable
    ///
    /// Typical workflow sequence:
    /// 1. Drying (7-14 days)
    /// 2. Curing (2-8 weeks)
    /// 3. Trimming (1-3 days)
    /// 4. Quality Control (1 day)
    /// 5. Packaging (1-2 days)
    /// </remarks>
    public class ProcessingStep : AuditableEntity
    {
        // ============================================================
        // BATCH REFERENCE
        // ============================================================

        /// <summary>
        /// Reference to production batch
        /// </summary>
        [Required]
        [Display(Name = "Production Batch")]
        public int ProductionBatchId { get; set; }

        /// <summary>
        /// Navigation property to ProductionBatch
        /// </summary>
        [ForeignKey(nameof(ProductionBatchId))]
        public virtual ProductionBatch ProductionBatch { get; set; } = null!;

        // ============================================================
        // STEP INFORMATION
        // ============================================================

        /// <summary>
        /// Type of processing step
        /// </summary>
        [Required]
        [Display(Name = "Step Type")]
        public ProcessingStepType StepType { get; set; }

        /// <summary>
        /// Sequence order of this step
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Step Number")]
        public int StepNumber { get; set; }

        /// <summary>
        /// Date/time when step started
        /// </summary>
        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Date/time when step completed
        /// </summary>
        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Duration in hours (calculated)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Duration (hours)")]
        public decimal? DurationHours { get; set; }

        // ============================================================
        // WEIGHT TRACKING
        // ============================================================

        /// <summary>
        /// Weight at start of step (grams)
        /// </summary>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Start Weight (g)")]
        public decimal? StartWeightGrams { get; set; }

        /// <summary>
        /// Weight at end of step (grams)
        /// </summary>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "End Weight (g)")]
        public decimal? EndWeightGrams { get; set; }

        /// <summary>
        /// Waste generated during this step (grams)
        /// </summary>
        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Waste (g)")]
        public decimal? WasteGrams { get; set; }

        // ============================================================
        // ENVIRONMENTAL CONDITIONS (GMP)
        // ============================================================

        /// <summary>
        /// Temperature during processing (e.g., "18-21°C")
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "Temperature")]
        public string? Temperature { get; set; }

        /// <summary>
        /// Humidity during processing (e.g., "55-60%")
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "Humidity")]
        public string? Humidity { get; set; }

        // ============================================================
        // STATUS & NOTES
        // ============================================================

        /// <summary>
        /// Current status of this step
        /// </summary>
        /// <remarks>
        /// Values: "Pending", "In Progress", "Completed", "Failed", "Skipped"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        /// <summary>
        /// User/operator who performed this step
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; }

        /// <summary>
        /// Notes about this processing step
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
