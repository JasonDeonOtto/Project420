namespace Project420.Inventory.BLL.Services;

public interface IStockAdjustmentService
{
    Task<int> CreateStockAdjustmentAsync(object dto);
    Task UpdateStockAdjustmentAsync(object dto);
    Task<object?> GetStockAdjustmentByIdAsync(int id);
    Task<IEnumerable<object>> GetAllStockAdjustmentsAsync();
    Task DeactivateStockAdjustmentAsync(int id);
    Task<IEnumerable<object>> GetStockAdjustmentsByProductAsync(int productId);
    Task<IEnumerable<object>> GetStockAdjustmentsByReasonAsync(string reason);
    Task<IEnumerable<object>> GetPendingAdjustmentsAsync();
    Task ApproveAdjustmentAsync(int adjustmentId, string approvedBy);
}
