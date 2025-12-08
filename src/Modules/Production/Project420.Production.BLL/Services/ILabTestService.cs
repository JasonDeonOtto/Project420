namespace Project420.Production.BLL.Services;

public interface ILabTestService
{
    Task<int> CreateLabTestAsync(object dto);
    Task UpdateLabTestAsync(object dto);
    Task<object?> GetLabTestByIdAsync(int id);
    Task<IEnumerable<object>> GetAllLabTestsAsync();
    Task DeactivateLabTestAsync(int id);
    Task<IEnumerable<object>> GetLabTestsByBatchAsync(int productionBatchId);
    Task<object?> GetLabTestByCertificateNumberAsync(string certificateNumber);
    Task<IEnumerable<object>> GetFailedLabTestsAsync();
}
