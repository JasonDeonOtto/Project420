using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Production.Models.Enums
{
    /// <summary>
    /// Types of quality control checks performed during production
    /// </summary>
    /// <remarks>
    /// SAHPRA GMP Compliance:
    /// - Quality checks required at critical control points
    /// - Failed batches cannot proceed to next step
    /// - All checks must be documented
    /// </remarks>
    public enum QualityCheckType
    {
        /// <summary>
        /// Visual inspection for defects, mold, pests, seeds
        /// </summary>
        [Display(Name = "Visual Inspection")]
        VisualInspection = 0,

        /// <summary>
        /// Weight verification (expected vs actual)
        /// </summary>
        /// <remarks>
        /// SARS Compliance: Weight discrepancies must be explained
        /// Significant loss may indicate theft or processing issues
        /// </remarks>
        [Display(Name = "Weight Check")]
        WeightCheck = 1,

        /// <summary>
        /// Moisture content testing (% moisture)
        /// </summary>
        /// <remarks>
        /// Target: 10-12% moisture for storage
        /// Too high: mold risk
        /// Too low: crumbly, harsh smoke
        /// </remarks>
        [Display(Name = "Moisture Test")]
        MoistureTest = 2,

        /// <summary>
        /// Lab testing for potency (THC/CBD %)
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: Lab tests from accredited facility
        /// ISO/IEC 17025 certified laboratory required
        /// </remarks>
        [Display(Name = "Potency Test")]
        PotencyTest = 3,

        /// <summary>
        /// Lab testing for contaminants (pesticides, heavy metals, microbes)
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: Contaminant testing mandatory
        /// Failed batches must be destroyed
        /// </remarks>
        [Display(Name = "Contaminant Test")]
        ContaminantTest = 4,

        /// <summary>
        /// Terpene profile analysis
        /// </summary>
        /// <remarks>
        /// Optional but valuable for:
        /// - Product differentiation
        /// - Medical applications (entourage effect)
        /// - Customer education
        /// </remarks>
        [Display(Name = "Terpene Analysis")]
        TerpeneAnalysis = 5,

        /// <summary>
        /// Final pre-packaging inspection
        /// </summary>
        [Display(Name = "Final Inspection")]
        FinalInspection = 6
    }
}
