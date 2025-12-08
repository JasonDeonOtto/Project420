using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Cultivation.Models.Enums
{
    /// <summary>
    /// Represents the different growth stages of a cannabis plant lifecycle
    /// </summary>
    /// <remarks>
    /// Cannabis Cultivation Compliance (SAHPRA Section 22C / DALRRD Hemp Permits):
    /// - Each stage must be tracked for seed-to-sale traceability
    /// - Stage transitions must be logged for GMP compliance
    /// - Accurate stage tracking required for yield prediction and reporting
    ///
    /// Typical Timeline (Indoor):
    /// - Seed: 1-7 days (germination)
    /// - Seedling: 2-3 weeks (establishing roots)
    /// - Vegetative: 3-8 weeks (growth phase)
    /// - Flowering: 6-12 weeks (bud development)
    /// - Harvested: Processing begins
    /// - Destroyed: Waste tracking for compliance
    /// </remarks>
    public enum PlantStage
    {
        /// <summary>
        /// Seed germination stage (0-7 days)
        /// </summary>
        [Display(Name = "Seed")]
        Seed = 0,

        /// <summary>
        /// Seedling stage - establishing roots and first leaves (2-3 weeks)
        /// </summary>
        [Display(Name = "Seedling")]
        Seedling = 1,

        /// <summary>
        /// Vegetative growth stage - plant is growing in size (3-8 weeks)
        /// </summary>
        [Display(Name = "Vegetative")]
        Vegetative = 2,

        /// <summary>
        /// Flowering stage - producing buds/flowers (6-12 weeks)
        /// </summary>
        [Display(Name = "Flowering")]
        Flowering = 3,

        /// <summary>
        /// Plant has been harvested and removed from grow area
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest date and weight must be recorded
        /// Links to HarvestBatch for processing traceability
        /// </remarks>
        [Display(Name = "Harvested")]
        Harvested = 4,

        /// <summary>
        /// Plant was destroyed (pest, disease, hermaphrodite, waste)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Waste must be tracked and documented
        /// SAHPRA requires reporting of destroyed plants
        /// Must document reason for destruction
        /// </remarks>
        [Display(Name = "Destroyed")]
        Destroyed = 5
    }
}
