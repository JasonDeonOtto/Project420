using Project420.Production.DAL.Repositories;

namespace Project420.Production.BLL.Services;

public class ProductionBatchService : IProductionBatchService
{
    private readonly IProductionBatchRepository _repository;

    public ProductionBatchService(IProductionBatchRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateProductionBatchAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateProductionBatchAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetProductionBatchByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllProductionBatchesAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateProductionBatchAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<object?> GetProductionBatchByBatchNumberAsync(string batchNumber)
    {
        var batches = await _repository.FindAsync(b => b.BatchNumber == batchNumber);
        return batches.FirstOrDefault();
    }

    public async Task<IEnumerable<object>> GetProductionBatchesByHarvestBatchAsync(string harvestBatchNumber)
    {
        var batches = await _repository.FindAsync(b => b.HarvestBatchNumber == harvestBatchNumber);
        return batches.Cast<object>();
    }

    public async Task UpdateWeightAsync(int batchId, decimal currentWeight, decimal? wasteWeight = null)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Production batch {batchId} not found");

        batch.CurrentWeightGrams = currentWeight;
        if (wasteWeight.HasValue)
        {
            batch.WasteWeightGrams = (batch.WasteWeightGrams ?? 0) + wasteWeight.Value;
        }

        await _repository.UpdateAsync(batch);
    }

    public async Task UpdateStatusAsync(int batchId, string newStatus)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Production batch {batchId} not found");

        batch.Status = newStatus;
        if (newStatus == "Completed")
        {
            batch.CompletionDate = DateTime.UtcNow;
            batch.FinalWeightGrams = batch.CurrentWeightGrams;
        }

        await _repository.UpdateAsync(batch);
    }

    public async Task RecordQualityControlAsync(int batchId, bool passed)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Production batch {batchId} not found");

        batch.QualityControlPassed = passed;
        await _repository.UpdateAsync(batch);
    }

    public async Task RecordLabTestAsync(int batchId, bool passed, string thcPercentage, string cbdPercentage)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Production batch {batchId} not found");

        batch.LabTestPassed = passed;
        batch.THCPercentage = thcPercentage;
        batch.CBDPercentage = cbdPercentage;

        await _repository.UpdateAsync(batch);
    }

    public async Task RecordPackagingAsync(int batchId, int unitsPackaged, string packageSize, DateTime packagingDate)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Production batch {batchId} not found");

        batch.UnitsPackaged = unitsPackaged;
        batch.PackageSize = packageSize;
        batch.PackagingDate = packagingDate;

        await _repository.UpdateAsync(batch);
    }
}
