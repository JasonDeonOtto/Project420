namespace Project420.Production.BLL.Services;

public interface IQualityControlService
{
    Task<int> CreateQualityControlAsync(object dto);
    Task UpdateQualityControlAsync(object dto);
    Task<object?> GetQualityControlByIdAsync(int id);
    Task<IEnumerable<object>> GetAllQualityControlsAsync();
    Task DeactivateQualityControlAsync(int id);
    Task<IEnumerable<object>> GetQualityControlsByBatchAsync(int productionBatchId);
    Task<IEnumerable<object>> GetFailedQualityControlsAsync();
}
