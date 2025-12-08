using Project420.Inventory.DAL.Repositories;

namespace Project420.Inventory.BLL.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IStockAdjustmentRepository _repository;

    public StockAdjustmentService(IStockAdjustmentRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateStockAdjustmentAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateStockAdjustmentAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetStockAdjustmentByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllStockAdjustmentsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateStockAdjustmentAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetStockAdjustmentsByProductAsync(int productId)
    {
        // StockAdjustment uses denormalized data (ProductSKU), not ProductId FK
        // Will implement with DTOs
        return Enumerable.Empty<object>();
    }

    public async Task<IEnumerable<object>> GetStockAdjustmentsByReasonAsync(string reason)
    {
        var adjustments = await _repository.FindAsync(a => a.Reason != null && a.Reason.Contains(reason));
        return adjustments.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetPendingAdjustmentsAsync()
    {
        // Entity doesn't have ApprovedDate - will implement with DTOs
        return Enumerable.Empty<object>();
    }

    public async Task ApproveAdjustmentAsync(int adjustmentId, string approvedBy)
    {
        // Entity doesn't have approval workflow properties
        // Will implement with DTOs when full approval workflow is designed
        throw new NotImplementedException("Approval workflow will be implemented with DTOs");
    }
}
