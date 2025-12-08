using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.Models.Entities
{
    /// <summary>
    /// Represents an individual cannabis plant tracked from seed/clone to harvest
    /// </summary>
    /// <remarks>
    /// ⚠️ CRITICAL COMPLIANCE REQUIREMENT ⚠️
    ///
    /// SAHPRA Section 22C Requirements:
    /// - EVERY individual plant MUST be tracked from seed to sale
    /// - Unique plant identifiers (tags/barcodes) required
    /// - Growth stage transitions must be logged
    /// - Harvest date and weight must be recorded
    /// - Plant destruction must be documented with reason
    ///
    /// DALRRD Hemp Permit Requirements:
    /// - Plant count must not exceed licensed limit
    /// - THC testing results must be tracked
    /// - Male plants must be destroyed (unless hemp breeding)
    ///
    /// GMP (Good Manufacturing Practice) Compliance:
    /// - Environmental conditions logged per growth stage
    /// - Pest/disease incidents documented
    /// - Nutrient/feeding schedules tracked
    ///
    /// This entity is the FOUNDATION of seed-to-sale traceability.
    /// Every cannabis product in retail must trace back to individual plants.
    /// </remarks>
    public class Plant : AuditableEntity
    {
        // ============================================================
        // UNIQUE IDENTIFICATION (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Unique plant tag/identifier (e.g., barcode, RFID, QR code)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Each plant MUST have unique identifier
        /// Best practices:
        /// - Physical tag attached to plant
        /// - RFID for automated tracking
        /// - QR code for mobile scanning
        /// Example formats: "PLT-2024-001", "PLANT-GC001-0042"
        /// </remarks>
        [Required(ErrorMessage = "Plant tag is required for SAHPRA compliance")]
        [MaxLength(100, ErrorMessage = "Plant tag cannot exceed 100 characters")]
        [Display(Name = "Plant Tag")]
        public string PlantTag { get; set; } = string.Empty;

        // ============================================================
        // GROW CYCLE & STRAIN
        // ============================================================

        /// <summary>
        /// Reference to the grow cycle this plant belongs to
        /// </summary>
        /// <remarks>
        /// Links individual plant to cultivation cycle
        /// Required for batch-level reporting and traceability
        /// </remarks>
        [Required(ErrorMessage = "Grow cycle is required")]
        [Display(Name = "Grow Cycle")]
        public int GrowCycleId { get; set; }

        /// <summary>
        /// Navigation property to GrowCycle
        /// </summary>
        [ForeignKey(nameof(GrowCycleId))]
        public virtual GrowCycle GrowCycle { get; set; } = null!;

        /// <summary>
        /// Cannabis strain name (inherited from GrowCycle but can be overridden)
        /// </summary>
        /// <remarks>
        /// Typically inherits from GrowCycle.StrainName
        /// Can be different if phenotype variations identified
        /// Critical for medical cannabis (specific strains prescribed)
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Strain Name")]
        public string? StrainName { get; set; }

        // ============================================================
        // GROWTH STAGE TRACKING (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Current growth stage of the plant
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Stage transitions must be tracked
        /// Used for:
        /// - Compliance reporting (plant count by stage)
        /// - Room assignment (veg vs flower rooms)
        /// - Nutrient scheduling (different needs per stage)
        /// - Harvest planning
        /// </remarks>
        [Required]
        [Display(Name = "Current Stage")]
        public PlantStage CurrentStage { get; set; } = PlantStage.Seed;

        /// <summary>
        /// Date when plant entered current stage
        /// </summary>
        /// <remarks>
        /// Used to track:
        /// - Days in current stage
        /// - Growth rate monitoring
        /// - Expected stage transition dates
        /// </remarks>
        [Display(Name = "Stage Start Date")]
        public DateTime? StageStartDate { get; set; }

        /// <summary>
        /// Date when seed was planted or clone was taken
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Plant origin date required
        /// Used to calculate plant age and expected harvest date
        /// </remarks>
        [Required(ErrorMessage = "Planted date is required")]
        [Display(Name = "Planted Date")]
        public DateTime PlantedDate { get; set; }

        /// <summary>
        /// Date when plant was harvested
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest date MUST be recorded
        /// Null until plant is harvested
        /// Links to HarvestBatch for processing traceability
        /// </remarks>
        [Display(Name = "Harvest Date")]
        public DateTime? HarvestDate { get; set; }

        // ============================================================
        // LOCATION TRACKING
        // ============================================================

        /// <summary>
        /// Current grow room where plant is located
        /// </summary>
        /// <remarks>
        /// GMP Compliance: Plant location must be tracked
        /// Plants move between rooms as they grow:
        /// - Clone room → Vegetative room → Flowering room
        /// </remarks>
        [Display(Name = "Current Grow Room")]
        public int? CurrentGrowRoomId { get; set; }

        /// <summary>
        /// Navigation property to GrowRoom
        /// </summary>
        [ForeignKey(nameof(CurrentGrowRoomId))]
        public virtual GrowRoom? CurrentGrowRoom { get; set; }

        // ============================================================
        // PLANT GENETICS & TYPE
        // ============================================================

        /// <summary>
        /// Whether plant is from seed or clone
        /// </summary>
        /// <remarks>
        /// Important for:
        /// - Genetic tracking (clones are genetically identical to mother)
        /// - Compliance reporting (seed source vs clone source)
        /// - Quality control (clones more consistent)
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "Plant Source")]
        public string? PlantSource { get; set; } // "Seed", "Clone"

        /// <summary>
        /// Reference to mother plant if this is a clone
        /// </summary>
        /// <remarks>
        /// Genetic lineage tracking
        /// Null if grown from seed
        /// Self-referencing foreign key
        /// </remarks>
        [Display(Name = "Mother Plant")]
        public int? MotherPlantId { get; set; }

        /// <summary>
        /// Navigation property to mother plant
        /// </summary>
        [ForeignKey(nameof(MotherPlantId))]
        public virtual Plant? MotherPlant { get; set; }

        /// <summary>
        /// Plant sex (Male, Female, Hermaphrodite, Unknown)
        /// </summary>
        /// <remarks>
        /// Critical for cultivation:
        /// - Female plants produce buds (desired for retail)
        /// - Male plants produce pollen (destroy unless breeding)
        /// - Hermaphrodites produce both (destroy to prevent seed formation)
        /// DALRRD: Male hemp plants acceptable for fiber/seed production
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "Plant Sex")]
        public string? PlantSex { get; set; } // "Male", "Female", "Hermaphrodite", "Unknown"

        // ============================================================
        // YIELD & HARVEST (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Wet weight of plant at harvest (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest weight MUST be recorded
        /// Wet weight = weight immediately after cutting
        /// Typically 70-80% water content
        /// </remarks>
        [Range(0, 100000, ErrorMessage = "Wet weight must be between 0 and 100,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Wet Weight (g)")]
        public decimal? WetWeightGrams { get; set; }

        /// <summary>
        /// Dry weight after drying/curing (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Dry weight used for inventory tracking
        /// Dry weight typically 20-30% of wet weight
        /// This is the saleable product weight
        /// Critical for SARS tax and inventory valuation
        /// </remarks>
        [Range(0, 100000, ErrorMessage = "Dry weight must be between 0 and 100,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Dry Weight (g)")]
        public decimal? DryWeightGrams { get; set; }

        /// <summary>
        /// Reference to harvest batch this plant was processed into
        /// </summary>
        /// <remarks>
        /// Links cultivation to production module
        /// Multiple plants combined into a harvest batch for processing
        /// Critical for seed-to-sale traceability
        /// </remarks>
        [Display(Name = "Harvest Batch")]
        public int? HarvestBatchId { get; set; }

        /// <summary>
        /// Navigation property to HarvestBatch
        /// </summary>
        [ForeignKey(nameof(HarvestBatchId))]
        public virtual HarvestBatch? HarvestBatch { get; set; }

        // ============================================================
        // HEALTH & QUALITY
        // ============================================================

        /// <summary>
        /// Current health status of plant
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Quality control
        /// - Pest/disease tracking
        /// - Culling decisions (destroy unhealthy plants)
        /// Values: "Healthy", "Sick", "Pest Infestation", "Nutrient Deficiency"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Health Status")]
        public string? HealthStatus { get; set; }

        /// <summary>
        /// Notes about plant health, issues, or observations
        /// </summary>
        /// <remarks>
        /// GMP Compliance: Issues must be documented
        /// Examples:
        /// - "Powdery mildew detected on leaves"
        /// - "Nitrogen deficiency - increased feeding"
        /// - "Hermaphrodite - destroyed to prevent pollination"
        /// </remarks>
        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // DESTRUCTION & WASTE TRACKING (SAHPRA REQUIRED)
        // ============================================================

        /// <summary>
        /// Date when plant was destroyed (if applicable)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Plant destruction MUST be documented
        /// Reasons for destruction:
        /// - Male plant (prevent pollination)
        /// - Hermaphrodite (prevent seed formation)
        /// - Disease/pest infestation (prevent spread)
        /// - Quality issues (not meeting standards)
        /// </remarks>
        [Display(Name = "Destroyed Date")]
        public DateTime? DestroyedDate { get; set; }

        /// <summary>
        /// Reason why plant was destroyed
        /// </summary>
        /// <remarks>
        /// SAHPRA Reporting: Destruction reason required
        /// Must be documented for compliance audits
        /// Examples: "Male", "Hermaphrodite", "Pest Infestation", "Poor Quality"
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Destruction Reason")]
        public string? DestructionReason { get; set; }

        /// <summary>
        /// Weight of waste when plant destroyed (grams)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Waste must be tracked and reported
        /// Used for waste reconciliation and compliance reporting
        /// </remarks>
        [Range(0, 100000, ErrorMessage = "Waste weight must be between 0 and 100,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Waste Weight (g)")]
        public decimal? WasteWeightGrams { get; set; }

        // ============================================================
        // STATUS
        // ============================================================

        /// <summary>
        /// Whether this plant record is currently active
        /// </summary>
        /// <remarks>
        /// Active = plant is alive and growing
        /// Inactive = plant harvested, destroyed, or died
        /// Use IsDeleted for soft delete (POPIA compliance)
        /// </remarks>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
