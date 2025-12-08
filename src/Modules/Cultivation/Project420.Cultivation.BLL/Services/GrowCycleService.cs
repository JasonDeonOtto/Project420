using Project420.Cultivation.DAL.Repositories;

namespace Project420.Cultivation.BLL.Services;

public class GrowCycleService : IGrowCycleService
{
    private readonly IGrowCycleRepository _repository;

    public GrowCycleService(IGrowCycleRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateGrowCycleAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateGrowCycleAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetGrowCycleByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllGrowCyclesAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateGrowCycleAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetActiveGrowCyclesAsync()
    {
        var cycles = await _repository.FindAsync(c => c.IsActive);
        return cycles.Cast<object>();
    }

    public async Task RecordHarvestCompletionAsync(int cycleId, DateTime harvestDate, decimal totalWetWeight, decimal totalDryWeight)
    {
        var cycle = await _repository.GetByIdAsync(cycleId);
        if (cycle == null) throw new InvalidOperationException($"Grow cycle {cycleId} not found");

        cycle.ActualHarvestDate = harvestDate;
        cycle.TotalWetWeightGrams = totalWetWeight;
        cycle.TotalDryWeightGrams = totalDryWeight;
        cycle.EndDate = harvestDate;
        cycle.IsActive = false;

        await _repository.UpdateAsync(cycle);
    }
}
