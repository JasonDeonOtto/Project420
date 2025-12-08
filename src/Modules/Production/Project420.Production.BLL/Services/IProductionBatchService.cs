namespace Project420.Production.BLL.Services;

/// <summary>
/// Service interface for production batch business logic.
/// SAHPRA GMP Compliance: Production batch tracking from harvest to retail.
/// </summary>
public interface IProductionBatchService
{
    Task<int> CreateProductionBatchAsync(object dto);
    Task UpdateProductionBatchAsync(object dto);
    Task<object?> GetProductionBatchByIdAsync(int id);
    Task<IEnumerable<object>> GetAllProductionBatchesAsync();
    Task DeactivateProductionBatchAsync(int id);
    Task<object?> GetProductionBatchByBatchNumberAsync(string batchNumber);
    Task<IEnumerable<object>> GetProductionBatchesByHarvestBatchAsync(string harvestBatchNumber);
    Task UpdateWeightAsync(int batchId, decimal currentWeight, decimal? wasteWeight = null);
    Task UpdateStatusAsync(int batchId, string newStatus);
    Task RecordQualityControlAsync(int batchId, bool passed);
    Task RecordLabTestAsync(int batchId, bool passed, string thcPercentage, string cbdPercentage);
    Task RecordPackagingAsync(int batchId, int unitsPackaged, string packageSize, DateTime packagingDate);
}
