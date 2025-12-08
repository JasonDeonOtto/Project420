namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service interface for harvest batch business logic.
/// SAHPRA Compliance: Batch tracking for seed-to-sale traceability.
/// </summary>
public interface IHarvestBatchService
{
    Task<int> CreateHarvestBatchAsync(object dto);
    Task UpdateHarvestBatchAsync(object dto);
    Task<object?> GetHarvestBatchByIdAsync(int id);
    Task<IEnumerable<object>> GetAllHarvestBatchesAsync();
    Task DeactivateHarvestBatchAsync(int id);
    Task<IEnumerable<object>> GetHarvestBatchesByGrowCycleAsync(int growCycleId);
    Task<object?> GetHarvestBatchByBatchNumberAsync(string batchNumber);
    Task RecordLabTestResultsAsync(int batchId, string thcPercentage, string cbdPercentage, DateTime testDate, string certificateNumber, bool passed);
    Task UpdateProcessingStatusAsync(int batchId, string newStatus);
}
