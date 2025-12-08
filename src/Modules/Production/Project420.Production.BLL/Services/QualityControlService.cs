using Project420.Production.DAL.Repositories;

namespace Project420.Production.BLL.Services;

public class QualityControlService : IQualityControlService
{
    private readonly IQualityControlRepository _repository;

    public QualityControlService(IQualityControlRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateQualityControlAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateQualityControlAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetQualityControlByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllQualityControlsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateQualityControlAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetQualityControlsByBatchAsync(int productionBatchId)
    {
        var qcs = await _repository.FindAsync(q => q.ProductionBatchId == productionBatchId);
        return qcs.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetFailedQualityControlsAsync()
    {
        var failed = await _repository.FindAsync(q => q.Passed == false);
        return failed.Cast<object>();
    }
}
