namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service interface for grow cycle business logic.
/// SAHPRA/DALRRD Compliance: Cultivation cycle tracking for reporting.
/// </summary>
public interface IGrowCycleService
{
    Task<int> CreateGrowCycleAsync(object dto);
    Task UpdateGrowCycleAsync(object dto);
    Task<object?> GetGrowCycleByIdAsync(int id);
    Task<IEnumerable<object>> GetAllGrowCyclesAsync();
    Task DeactivateGrowCycleAsync(int id);
    Task<IEnumerable<object>> GetActiveGrowCyclesAsync();
    Task RecordHarvestCompletionAsync(int cycleId, DateTime harvestDate, decimal totalWetWeight, decimal totalDryWeight);
}
