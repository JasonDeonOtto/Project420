using Project420.Cultivation.DAL.Repositories;

namespace Project420.Cultivation.BLL.Services;

public class HarvestBatchService : IHarvestBatchService
{
    private readonly IHarvestBatchRepository _repository;

    public HarvestBatchService(IHarvestBatchRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateHarvestBatchAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateHarvestBatchAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetHarvestBatchByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllHarvestBatchesAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateHarvestBatchAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetHarvestBatchesByGrowCycleAsync(int growCycleId)
    {
        var batches = await _repository.FindAsync(b => b.GrowCycleId == growCycleId);
        return batches.Cast<object>();
    }

    public async Task<object?> GetHarvestBatchByBatchNumberAsync(string batchNumber)
    {
        var batches = await _repository.FindAsync(b => b.BatchNumber == batchNumber);
        return batches.FirstOrDefault();
    }

    public async Task RecordLabTestResultsAsync(int batchId, string thcPercentage, string cbdPercentage, DateTime testDate, string certificateNumber, bool passed)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Harvest batch {batchId} not found");

        batch.THCPercentage = thcPercentage;
        batch.CBDPercentage = cbdPercentage;
        batch.LabTestDate = testDate;
        batch.LabTestCertificateNumber = certificateNumber;
        batch.LabTestPassed = passed;

        await _repository.UpdateAsync(batch);
    }

    public async Task UpdateProcessingStatusAsync(int batchId, string newStatus)
    {
        var batch = await _repository.GetByIdAsync(batchId);
        if (batch == null) throw new InvalidOperationException($"Harvest batch {batchId} not found");

        batch.ProcessingStatus = newStatus;
        await _repository.UpdateAsync(batch);
    }
}
