namespace Project420.Production.BLL.Services;

public interface IProcessingStepService
{
    Task<int> CreateProcessingStepAsync(object dto);
    Task UpdateProcessingStepAsync(object dto);
    Task<object?> GetProcessingStepByIdAsync(int id);
    Task<IEnumerable<object>> GetAllProcessingStepsAsync();
    Task DeactivateProcessingStepAsync(int id);
    Task<IEnumerable<object>> GetProcessingStepsByBatchAsync(int productionBatchId);
}
