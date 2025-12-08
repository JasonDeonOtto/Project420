namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service interface for stock movement business logic.
/// SARS Compliance: Stock movement tracking for tax reconciliation.
/// </summary>
public interface IStockMovementService
{
    Task<int> CreateStockMovementAsync(object dto);
    Task UpdateStockMovementAsync(object dto);
    Task<object?> GetStockMovementByIdAsync(int id);
    Task<IEnumerable<object>> GetAllStockMovementsAsync();
    Task DeactivateStockMovementAsync(int id);
    Task<IEnumerable<object>> GetStockMovementsByProductAsync(int productId);
    Task<IEnumerable<object>> GetStockMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<object>> GetStockMovementsByTypeAsync(string movementType);
}
