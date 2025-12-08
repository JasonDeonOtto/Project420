using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Production.Models.Enums
{
    /// <summary>
    /// Represents different processing steps in cannabis production workflow
    /// </summary>
    /// <remarks>
    /// SAHPRA GMP Compliance:
    /// - Each processing step must be documented
    /// - Quality control checkpoints required at key stages
    /// - Processing duration and conditions must be tracked
    ///
    /// Standard Cannabis Processing Workflow:
    /// Harvest → Drying → Curing → Trimming → Quality Control → Packaging → Inventory
    /// </remarks>
    public enum ProcessingStepType
    {
        /// <summary>
        /// Drying freshly harvested cannabis (7-14 days)
        /// </summary>
        /// <remarks>
        /// Critical parameters:
        /// - Temperature: 15-21°C
        /// - Humidity: 45-55%
        /// - Darkness (no light)
        /// - Air circulation (not direct)
        /// </remarks>
        [Display(Name = "Drying")]
        Drying = 0,

        /// <summary>
        /// Curing dried cannabis to improve quality (2-8 weeks)
        /// </summary>
        /// <remarks>
        /// Critical parameters:
        /// - Temperature: 15-21°C
        /// - Humidity: 58-62%
        /// - Burp containers daily (first 2 weeks)
        /// Improves: flavor, smoothness, potency preservation
        /// </remarks>
        [Display(Name = "Curing")]
        Curing = 1,

        /// <summary>
        /// Trimming/manicuring buds (removing excess leaves)
        /// </summary>
        /// <remarks>
        /// Options:
        /// - Wet trim (before drying)
        /// - Dry trim (after drying)
        /// Affects: appearance, bag appeal, weight
        /// </remarks>
        [Display(Name = "Trimming")]
        Trimming = 2,

        /// <summary>
        /// Quality control inspection and testing
        /// </summary>
        /// <remarks>
        /// SAHPRA Requirement: Quality checks before release
        /// Includes:
        /// - Visual inspection (mold, pests, seeds)
        /// - Lab testing (potency, contaminants)
        /// - Weight verification
        /// </remarks>
        [Display(Name = "Quality Control")]
        QualityControl = 3,

        /// <summary>
        /// Packaging for retail sale
        /// </summary>
        /// <remarks>
        /// SAHPRA Label Requirements:
        /// - THC/CBD content
        /// - Batch number
        /// - Lab test date
        /// - Warning labels
        /// - Packaging date
        /// </remarks>
        [Display(Name = "Packaging")]
        Packaging = 4,

        /// <summary>
        /// Kief/trichome extraction
        /// </summary>
        [Display(Name = "Extraction")]
        Extraction = 5,

        /// <summary>
        /// Processing waste/trim material
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Waste must be tracked
        /// Options:
        /// - Extraction (kief, hash)
        /// - Disposal (documented destruction)
        /// </remarks>
        [Display(Name = "Waste Processing")]
        WasteProcessing = 6
    }
}
