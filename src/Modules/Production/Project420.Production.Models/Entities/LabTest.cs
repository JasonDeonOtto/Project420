using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Production.Models.Entities
{
    /// <summary>
    /// Represents laboratory testing results for a production batch
    /// </summary>
    /// <remarks>
    /// ⚠️ SAHPRA MANDATORY REQUIREMENT ⚠️
    ///
    /// SAHPRA Section 22C Compliance:
    /// - ALL medical cannabis must be tested by ISO/IEC 17025 accredited lab
    /// - Certificate of Analysis (COA) required before sale
    /// - Testing must include:
    ///   * Potency (THC, CBD, other cannabinoids)
    ///   * Contaminants (pesticides, heavy metals)
    ///   * Microbiological (bacteria, mold, yeast)
    /// - Failed batches CANNOT be sold and must be destroyed
    ///
    /// COA must be retained for 7 years (POPIA + SARS requirement)
    /// </remarks>
    public class LabTest : AuditableEntity
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
        // LAB INFORMATION
        // ============================================================

        /// <summary>
        /// Name of testing laboratory
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: Must be ISO/IEC 17025 accredited
        /// </remarks>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Lab Name")]
        public string LabName { get; set; } = string.Empty;

        /// <summary>
        /// Lab certificate/accreditation number
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Lab Certificate Number")]
        public string? LabCertificateNumber { get; set; }

        /// <summary>
        /// Certificate of Analysis (COA) reference number
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "COA Number")]
        public string COANumber { get; set; } = string.Empty;

        /// <summary>
        /// Date sample was sent to lab
        /// </summary>
        [Required]
        [Display(Name = "Sample Date")]
        public DateTime SampleDate { get; set; }

        /// <summary>
        /// Date lab results were received
        /// </summary>
        [Display(Name = "Results Date")]
        public DateTime? ResultsDate { get; set; }

        // ============================================================
        // POTENCY RESULTS (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// THC percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "THC %")]
        public decimal? THCPercentage { get; set; }

        /// <summary>
        /// CBD percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "CBD %")]
        public decimal? CBDPercentage { get; set; }

        /// <summary>
        /// Total cannabinoids percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Total Cannabinoids %")]
        public decimal? TotalCannabinoidsPercentage { get; set; }

        // ============================================================
        // CONTAMINANT TESTING (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Whether batch passed pesticide testing
        /// </summary>
        [Display(Name = "Pesticides Passed")]
        public bool? PesticidesPassed { get; set; }

        /// <summary>
        /// Whether batch passed heavy metals testing
        /// </summary>
        [Display(Name = "Heavy Metals Passed")]
        public bool? HeavyMetalsPassed { get; set; }

        /// <summary>
        /// Whether batch passed microbial testing (bacteria, mold, yeast)
        /// </summary>
        [Display(Name = "Microbial Passed")]
        public bool? MicrobialPassed { get; set; }

        // ============================================================
        // OVERALL RESULT
        // ============================================================

        /// <summary>
        /// Whether batch passed ALL lab tests
        /// </summary>
        /// <remarks>
        /// SAHPRA: Failed batches CANNOT proceed to sale
        /// Must be true for batch to be released to inventory
        /// </remarks>
        [Required]
        [Display(Name = "Overall Pass")]
        public bool OverallPass { get; set; }

        /// <summary>
        /// Details of any failed tests
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Failure Details")]
        public string? FailureDetails { get; set; }

        // ============================================================
        // DOCUMENTATION
        // ============================================================

        /// <summary>
        /// File path or URL to COA document
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: COA must be retained for 7 years
        /// Store in document management system
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "COA Document Path")]
        public string? COADocumentPath { get; set; }

        /// <summary>
        /// Additional notes about lab testing
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
