using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for Plant entity operations.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public interface IPlantRepository : IRepository<Plant>
{
    /// <summary>
    /// Gets all active plants (IsActive = true, not harvested or destroyed).
    /// </summary>
    /// <returns>Collection of active plants</returns>
    /// <remarks>
    /// SAHPRA/DALRRD: Active plants are those currently in cultivation.
    /// Excludes harvested and destroyed plants.
    /// </remarks>
    Task<IEnumerable<Plant>> GetActivePlantsAsync();

    /// <summary>
    /// Gets a plant by its unique plant tag.
    /// </summary>
    /// <param name="plantTag">Plant tag identifier (e.g., barcode, RFID, QR code)</param>
    /// <returns>Plant if found, null otherwise</returns>
    /// <remarks>
    /// SAHPRA Compliance: Each plant MUST have unique identifier for tracking.
    /// </remarks>
    Task<Plant?> GetByPlantTagAsync(string plantTag);

    /// <summary>
    /// Gets all plants in a specific grow cycle.
    /// </summary>
    /// <param name="growCycleId">GrowCycle primary key</param>
    /// <returns>Collection of plants in the specified cycle</returns>
    /// <remarks>
    /// Used for cycle-level reporting and batch traceability.
    /// </remarks>
    Task<IEnumerable<Plant>> GetByGrowCycleIdAsync(int growCycleId);

    /// <summary>
    /// Gets all plants currently in a specific grow room.
    /// </summary>
    /// <param name="growRoomId">GrowRoom primary key</param>
    /// <returns>Collection of plants in the specified room</returns>
    /// <remarks>
    /// GMP Compliance: Used for room capacity tracking and plant location.
    /// </remarks>
    Task<IEnumerable<Plant>> GetByGrowRoomIdAsync(int growRoomId);

    /// <summary>
    /// Gets plants filtered by current growth stage.
    /// </summary>
    /// <param name="stage">Plant growth stage (Seed, Seedling, Vegetative, Flowering, Harvested, Destroyed)</param>
    /// <returns>Collection of plants at the specified stage</returns>
    /// <remarks>
    /// SAHPRA: Used for stage-based compliance reporting.
    /// </remarks>
    Task<IEnumerable<Plant>> GetByStageAsync(PlantStage stage);

    /// <summary>
    /// Gets a plant by ID with related data eagerly loaded.
    /// </summary>
    /// <param name="id">Plant primary key</param>
    /// <returns>Plant with GrowCycle, GrowRoom, HarvestBatch, and MotherPlant navigation properties loaded</returns>
    /// <remarks>
    /// Includes: GrowCycle, CurrentGrowRoom, HarvestBatch, MotherPlant
    /// Use for complete plant history and traceability view.
    /// </remarks>
    Task<Plant?> GetByIdWithRelatedDataAsync(int id);

    /// <summary>
    /// Gets plants ready for harvest (Flowering stage, active).
    /// </summary>
    /// <returns>Collection of plants ready for harvest</returns>
    /// <remarks>
    /// SAHPRA/GMP: Used for harvest planning and scheduling.
    /// Returns plants in Flowering stage that are ready to harvest.
    /// </remarks>
    Task<IEnumerable<Plant>> GetPlantsReadyForHarvestAsync();

    /// <summary>
    /// Gets plants that need to be destroyed (Male or Hermaphrodite).
    /// </summary>
    /// <returns>Collection of plants requiring destruction</returns>
    /// <remarks>
    /// DALRRD: Male plants must be destroyed (unless breeding).
    /// Hermaphrodites must be destroyed to prevent pollination.
    /// Returns plants with PlantSex = "Male" or "Hermaphrodite" that haven't been destroyed yet.
    /// </remarks>
    Task<IEnumerable<Plant>> GetPlantsForDestructionAsync();

    /// <summary>
    /// Checks if a plant tag is unique (not used by another plant).
    /// </summary>
    /// <param name="plantTag">Plant tag to validate</param>
    /// <param name="excludeId">Optional: Exclude this plant ID from check (for updates)</param>
    /// <returns>True if unique, false if already exists</returns>
    /// <remarks>
    /// SAHPRA: Plant tags MUST be unique for traceability.
    /// Used for validation before creating/updating plants.
    /// </remarks>
    Task<bool> IsPlantTagUniqueAsync(string plantTag, int? excludeId = null);

    /// <summary>
    /// Gets plants filtered by strain name.
    /// </summary>
    /// <param name="strainName">Cannabis strain name</param>
    /// <returns>Collection of plants of the specified strain</returns>
    /// <remarks>
    /// Used for strain-specific tracking and genetics management.
    /// </remarks>
    Task<IEnumerable<Plant>> GetPlantsByStrainAsync(string strainName);
}
