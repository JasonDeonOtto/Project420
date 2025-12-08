using Project420.Production.DAL.Repositories;

namespace Project420.Production.BLL.Services;

public class LabTestService : ILabTestService
{
    private readonly ILabTestRepository _repository;

    public LabTestService(ILabTestRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateLabTestAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateLabTestAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetLabTestByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllLabTestsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateLabTestAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetLabTestsByBatchAsync(int productionBatchId)
    {
        var tests = await _repository.FindAsync(t => t.ProductionBatchId == productionBatchId);
        return tests.Cast<object>();
    }

    public async Task<object?> GetLabTestByCertificateNumberAsync(string certificateNumber)
    {
        var tests = await _repository.FindAsync(t => t.COANumber == certificateNumber);
        return tests.FirstOrDefault();
    }

    public async Task<IEnumerable<object>> GetFailedLabTestsAsync()
    {
        // LabTest entity doesn't have TestPassed property - will be determined by business logic
        // For now, return empty - will implement with DTOs
        return Enumerable.Empty<object>();
    }
}
