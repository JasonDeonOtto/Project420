using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.Models.Entities
{
    /// <summary>
    /// Represents a physical growing location/room for cannabis cultivation
    /// </summary>
    /// <remarks>
    /// SAHPRA GMP Compliance Requirements:
    /// - All growing locations must be identified and tracked
    /// - Environmental conditions must be monitored and recorded
    /// - Security measures must be in place
    /// - Rooms must be segregated by growth stage to prevent contamination
    ///
    /// POPIA Compliance:
    /// - Inherits from AuditableEntity for full audit trail
    /// - All room changes tracked (who, what, when)
    /// </remarks>
    public class GrowRoom : AuditableEntity
    {
        // ============================================================
        // BASIC INFORMATION
        // ============================================================

        /// <summary>
        /// Room code/identifier (e.g., "VEG-01", "FLOWER-A", "MOTHER-ROOM")
        /// </summary>
        [Required(ErrorMessage = "Room code is required")]
        [MaxLength(50, ErrorMessage = "Room code cannot exceed 50 characters")]
        [Display(Name = "Room Code")]
        public string RoomCode { get; set; } = string.Empty;

        /// <summary>
        /// Room name/description
        /// </summary>
        [Required(ErrorMessage = "Room name is required")]
        [MaxLength(200, ErrorMessage = "Room name cannot exceed 200 characters")]
        [Display(Name = "Room Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of grow room (Mother, Clone, Vegetative, Flowering, Drying)
        /// </summary>
        [Required]
        [Display(Name = "Room Type")]
        public GrowRoomType RoomType { get; set; }

        /// <summary>
        /// Room size in square meters
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Capacity planning
        /// - Plant density calculations
        /// - Compliance reporting (SAHPRA requires facility layout)
        /// </remarks>
        [Range(0.01, 10000.00, ErrorMessage = "Room size must be between 0.01 and 10,000 square meters")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Room Size (m²)")]
        public decimal? RoomSizeSquareMeters { get; set; }

        /// <summary>
        /// Maximum plant capacity for this room
        /// </summary>
        /// <remarks>
        /// DALRRD Hemp Permit Compliance:
        /// - Must not exceed licensed plant count
        /// - Must track plant density for GMP compliance
        /// </remarks>
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000 plants")]
        [Display(Name = "Max Capacity")]
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Whether this room is currently active/operational
        /// </summary>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // ENVIRONMENTAL MONITORING (GMP Compliance)
        // ============================================================

        /// <summary>
        /// Target temperature range in Celsius (e.g., "22-26°C")
        /// </summary>
        /// <remarks>
        /// SAHPRA GMP: Environmental conditions must be controlled and recorded
        /// Optimal cannabis growth: 22-28°C (vegetative), 20-26°C (flowering)
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Target Temperature")]
        public string? TargetTemperature { get; set; }

        /// <summary>
        /// Target humidity range as percentage (e.g., "50-60%")
        /// </summary>
        /// <remarks>
        /// SAHPRA GMP: Humidity must be controlled to prevent mold/mildew
        /// Optimal: 70-80% (seedling), 50-70% (veg), 40-50% (flowering)
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Target Humidity")]
        public string? TargetHumidity { get; set; }

        /// <summary>
        /// Light cycle schedule (e.g., "18/6", "12/12")
        /// </summary>
        /// <remarks>
        /// Vegetative: 18 hours light, 6 hours dark (18/6)
        /// Flowering: 12 hours light, 12 hours dark (12/12)
        /// Mother plants: 18/6 to keep in vegetative state
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Light Cycle")]
        public string? LightCycle { get; set; }

        // ============================================================
        // LOCATION & FACILITY
        // ============================================================

        /// <summary>
        /// Building or facility location
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        /// <summary>
        /// Notes about security, equipment, or special considerations
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of plants currently in this room
        /// </summary>
        /// <remarks>
        /// EF Core will automatically populate this collection
        /// Used for tracking current room occupancy and plant count
        /// </remarks>
        public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

        /// <summary>
        /// Collection of grow cycles that used this room
        /// </summary>
        public virtual ICollection<GrowCycle> GrowCycles { get; set; } = new List<GrowCycle>();
    }
}
