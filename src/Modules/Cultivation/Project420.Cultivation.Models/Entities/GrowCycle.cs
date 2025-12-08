using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Cultivation.Models.Entities
{
    /// <summary>
    /// Represents a complete cultivation cycle from start to harvest
    /// </summary>
    /// <remarks>
    /// SAHPRA/DALRRD Compliance Requirements:
    /// - Each cultivation cycle must be uniquely identified
    /// - Start and end dates must be tracked for reporting
    /// - Strain and yield data required for compliance reports
    /// - Links to plants for seed-to-sale traceability
    ///
    /// A grow cycle typically includes:
    /// - Planting/germination date
    /// - Harvest completion date
    /// - Total yield (wet and dry weight)
    /// - Associated plants and batches
    /// </remarks>
    public class GrowCycle : AuditableEntity
    {
        // ============================================================
        // BASIC INFORMATION
        // ============================================================

        /// <summary>
        /// Cycle code/identifier (e.g., "GC-2024-001", "CYCLE-NOV-2024")
        /// </summary>
        [Required(ErrorMessage = "Cycle code is required")]
        [MaxLength(50, ErrorMessage = "Cycle code cannot exceed 50 characters")]
        [Display(Name = "Cycle Code")]
        public string CycleCode { get; set; } = string.Empty;

        /// <summary>
        /// Cycle name/description
        /// </summary>
        [Required(ErrorMessage = "Cycle name is required")]
        [MaxLength(200, ErrorMessage = "Cycle name cannot exceed 200 characters")]
        [Display(Name = "Cycle Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cannabis strain being cultivated in this cycle
        /// </summary>
        /// <remarks>
        /// Critical for:
        /// - Medical cannabis tracking (specific strains for conditions)
        /// - Genetic consistency
        /// - Customer preferences
        /// - SAHPRA reporting
        /// </remarks>
        [Required(ErrorMessage = "Strain name is required")]
        [MaxLength(100, ErrorMessage = "Strain name cannot exceed 100 characters")]
        [Display(Name = "Strain Name")]
        public string StrainName { get; set; } = string.Empty;

        // ============================================================
        // DATES & TIMELINE
        // ============================================================

        /// <summary>
        /// Date when planting/germination began
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Start date required for cultivation tracking
        /// Used to calculate growth duration and expected harvest date
        /// </remarks>
        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Planned harvest date (estimated)
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Production planning
        /// - Resource scheduling
        /// - Customer pre-orders
        /// </remarks>
        [Display(Name = "Planned Harvest Date")]
        public DateTime? PlannedHarvestDate { get; set; }

        /// <summary>
        /// Actual harvest completion date
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Actual harvest date required for reporting
        /// Null until harvest is complete
        /// </remarks>
        [Display(Name = "Actual Harvest Date")]
        public DateTime? ActualHarvestDate { get; set; }

        /// <summary>
        /// Date when cycle was completed/closed
        /// </summary>
        /// <remarks>
        /// Cycle is complete when all plants harvested and processed
        /// Used to mark cycle as finished in reporting
        /// </remarks>
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        // ============================================================
        // GROW ROOM REFERENCE
        // ============================================================

        /// <summary>
        /// Primary grow room used for this cycle
        /// </summary>
        /// <remarks>
        /// Plants may move between rooms during lifecycle
        /// This tracks the main flowering room
        /// </remarks>
        [Display(Name = "Grow Room")]
        public int? GrowRoomId { get; set; }

        /// <summary>
        /// Navigation property to GrowRoom
        /// </summary>
        [ForeignKey(nameof(GrowRoomId))]
        public virtual GrowRoom? GrowRoom { get; set; }

        // ============================================================
        // PLANT COUNTS & YIELD (SAHPRA Reporting)
        // ============================================================

        /// <summary>
        /// Total number of plants started in this cycle
        /// </summary>
        /// <remarks>
        /// DALRRD Compliance: Must not exceed licensed plant count
        /// Used for plant count reporting
        /// </remarks>
        [Range(0, 10000, ErrorMessage = "Plant count must be between 0 and 10,000")]
        [Display(Name = "Total Plants Started")]
        public int TotalPlantsStarted { get; set; } = 0;

        /// <summary>
        /// Number of plants successfully harvested
        /// </summary>
        /// <remarks>
        /// SAHPRA Reporting: Actual harvested plant count required
        /// May be less than started due to culling (males, hermaphrodites, disease)
        /// </remarks>
        [Range(0, 10000, ErrorMessage = "Harvested count must be between 0 and 10,000")]
        [Display(Name = "Plants Harvested")]
        public int PlantsHarvested { get; set; } = 0;

        /// <summary>
        /// Total wet weight yield in grams (immediately after harvest)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Harvest weight must be recorded
        /// Wet weight typically 70-80% moisture
        /// Used to calculate expected dry weight
        /// </remarks>
        [Range(0, 1000000, ErrorMessage = "Wet weight must be between 0 and 1,000,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Wet Weight (g)")]
        public decimal? TotalWetWeightGrams { get; set; }

        /// <summary>
        /// Total dry weight yield in grams (after drying/curing)
        /// </summary>
        /// <remarks>
        /// SAHPRA Compliance: Final dry weight required for inventory
        /// Dry weight typically 20-30% of wet weight
        /// This is the saleable product weight
        /// Critical for SARS tax calculations and inventory valuation
        /// </remarks>
        [Range(0, 1000000, ErrorMessage = "Dry weight must be between 0 and 1,000,000 grams")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Dry Weight (g)")]
        public decimal? TotalDryWeightGrams { get; set; }

        // ============================================================
        // STATUS & NOTES
        // ============================================================

        /// <summary>
        /// Whether this cycle is currently active
        /// </summary>
        /// <remarks>
        /// Active = plants still growing
        /// Inactive = harvest complete or cycle abandoned
        /// </remarks>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Notes about this grow cycle (issues, observations, genetics)
        /// </summary>
        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of plants in this grow cycle
        /// </summary>
        /// <remarks>
        /// Enables seed-to-sale traceability
        /// Links individual plants to the cultivation cycle
        /// </remarks>
        public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

        /// <summary>
        /// Collection of harvest batches from this cycle
        /// </summary>
        /// <remarks>
        /// Links cultivation to production/processing
        /// One cycle may produce multiple harvest batches
        /// </remarks>
        public virtual ICollection<HarvestBatch> HarvestBatches { get; set; } = new List<HarvestBatch>();
    }
}
