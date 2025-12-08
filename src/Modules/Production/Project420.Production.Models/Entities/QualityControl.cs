using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Production.Models.Enums;

namespace Project420.Production.Models.Entities
{
    /// <summary>
    /// Represents a quality control checkpoint during production
    /// </summary>
    /// <remarks>
    /// SAHPRA GMP Compliance:
    /// - Quality checks required at critical control points
    /// - Failed checks must halt production
    /// - All checks must be documented with results
    /// - Inspector/operator must be identified
    /// </remarks>
    public class QualityControl : AuditableEntity
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
        // CHECK INFORMATION
        // ============================================================

        /// <summary>
        /// Type of quality check performed
        /// </summary>
        [Required]
        [Display(Name = "Check Type")]
        public QualityCheckType CheckType { get; set; }

        /// <summary>
        /// Date/time when check was performed
        /// </summary>
        [Required]
        [Display(Name = "Check Date")]
        public DateTime CheckDate { get; set; }

        /// <summary>
        /// User/inspector who performed the check
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Inspector")]
        public string Inspector { get; set; } = string.Empty;

        // ============================================================
        // RESULTS
        // ============================================================

        /// <summary>
        /// Whether the batch passed this quality check
        /// </summary>
        [Required]
        [Display(Name = "Passed")]
        public bool Passed { get; set; }

        /// <summary>
        /// Detailed results or measurements
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Moisture: 11.5%" (MoistureTest)
        /// - "No visible mold or pests" (VisualInspection)
        /// - "Weight: 502.3g (expected 500g)" (WeightCheck)
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Results")]
        public string? Results { get; set; }

        /// <summary>
        /// Defects or issues found
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Defects Found")]
        public string? DefectsFound { get; set; }

        /// <summary>
        /// Corrective actions taken if check failed
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Corrective Actions")]
        public string? CorrectiveActions { get; set; }

        // ============================================================
        // NOTES
        // ============================================================

        /// <summary>
        /// Additional notes about this quality check
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
