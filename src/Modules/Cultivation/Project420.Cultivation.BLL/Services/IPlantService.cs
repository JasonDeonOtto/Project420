namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service interface for plant business logic.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public interface IPlantService
{
    /// <summary>
    /// Creates a new plant record with validation and compliance checks.
    /// </summary>
    /// <param name="dto">Plant creation data</param>
    /// <returns>Created plant with generated ID</returns>
    Task<int> CreatePlantAsync(object dto); // Will replace 'object' with proper DTO later

    /// <summary>
    /// Updates an existing plant record.
    /// </summary>
    /// <param name="dto">Plant update data</param>
    Task UpdatePlantAsync(object dto);

    /// <summary>
    /// Gets a plant by ID.
    /// </summary>
    /// <param name="id">Plant ID</param>
    /// <returns>Plant if found, null otherwise</returns>
    Task<object?> GetPlantByIdAsync(int id);

    /// <summary>
    /// Gets all active plants.
    /// </summary>
    /// <returns>Collection of all active plants</returns>
    Task<IEnumerable<object>> GetAllPlantsAsync();

    /// <summary>
    /// Deactivates a plant (soft delete).
    /// POPIA requires data retention.
    /// </summary>
    /// <param name="id">Plant ID</param>
    Task DeactivatePlantAsync(int id);

    /// <summary>
    /// Gets plants by grow cycle.
    /// </summary>
    /// <param name="growCycleId">Grow cycle ID</param>
    /// <returns>Plants in the grow cycle</returns>
    Task<IEnumerable<object>> GetPlantsByGrowCycleAsync(int growCycleId);

    /// <summary>
    /// Gets plants by current stage.
    /// </summary>
    /// <param name="stage">Plant stage</param>
    /// <returns>Plants in specified stage</returns>
    Task<IEnumerable<object>> GetPlantsByStageAsync(string stage);

    /// <summary>
    /// Records plant harvest.
    /// SAHPRA Compliance: Harvest date and weight must be recorded.
    /// </summary>
    /// <param name="plantId">Plant ID</param>
    /// <param name="harvestDate">Harvest date</param>
    /// <param name="wetWeightGrams">Wet weight in grams</param>
    /// <param name="dryWeightGrams">Dry weight in grams (optional)</param>
    Task RecordHarvestAsync(int plantId, DateTime harvestDate, decimal wetWeightGrams, decimal? dryWeightGrams = null);

    /// <summary>
    /// Records plant destruction.
    /// SAHPRA Compliance: Plant destruction must be documented.
    /// </summary>
    /// <param name="plantId">Plant ID</param>
    /// <param name="destructionDate">Destruction date</param>
    /// <param name="reason">Reason for destruction</param>
    /// <param name="wasteWeightGrams">Waste weight in grams</param>
    Task RecordDestructionAsync(int plantId, DateTime destructionDate, string reason, decimal wasteWeightGrams);
}
