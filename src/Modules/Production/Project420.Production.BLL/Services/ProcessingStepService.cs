using Project420.Production.DAL.Repositories;

namespace Project420.Production.BLL.Services;

public class ProcessingStepService : IProcessingStepService
{
    private readonly IProcessingStepRepository _repository;

    public ProcessingStepService(IProcessingStepRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateProcessingStepAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateProcessingStepAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetProcessingStepByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllProcessingStepsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateProcessingStepAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetProcessingStepsByBatchAsync(int productionBatchId)
    {
        var steps = await _repository.FindAsync(s => s.ProductionBatchId == productionBatchId);
        return steps.Cast<object>();
    }
}
