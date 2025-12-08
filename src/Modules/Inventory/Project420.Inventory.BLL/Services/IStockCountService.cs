namespace Project420.Inventory.BLL.Services;

public interface IStockCountService
{
    Task<int> CreateStockCountAsync(object dto);
    Task UpdateStockCountAsync(object dto);
    Task<object?> GetStockCountByIdAsync(int id);
    Task<IEnumerable<object>> GetAllStockCountsAsync();
    Task DeactivateStockCountAsync(int id);
    Task<IEnumerable<object>> GetStockCountsByLocationAsync(string location);
    Task<IEnumerable<object>> GetStockCountsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<object>> GetPendingStockCountsAsync();
    Task CompleteStockCountAsync(int countId, DateTime completionDate);
    Task<IEnumerable<object>> GetStockCountsWithVariancesAsync();
}
