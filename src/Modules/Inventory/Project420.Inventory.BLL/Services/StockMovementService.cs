using Project420.Inventory.DAL.Repositories;

namespace Project420.Inventory.BLL.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;

    public StockMovementService(IStockMovementRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateStockMovementAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateStockMovementAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetStockMovementByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllStockMovementsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateStockMovementAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetStockMovementsByProductAsync(int productId)
    {
        // StockMovement uses denormalized data (ProductSKU), not ProductId FK
        // Will implement with DTOs
        return Enumerable.Empty<object>();
    }

    public async Task<IEnumerable<object>> GetStockMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var movements = await _repository.FindAsync(m => m.MovementDate >= startDate && m.MovementDate <= endDate);
        return movements.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetStockMovementsByTypeAsync(string movementType)
    {
        var movements = await _repository.FindAsync(m => m.MovementType.ToString() == movementType);
        return movements.Cast<object>();
    }
}
