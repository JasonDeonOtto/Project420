namespace Project420.Inventory.BLL.Services;

public interface IStockTransferService
{
    Task<int> CreateStockTransferAsync(object dto);
    Task UpdateStockTransferAsync(object dto);
    Task<object?> GetStockTransferByIdAsync(int id);
    Task<IEnumerable<object>> GetAllStockTransfersAsync();
    Task DeactivateStockTransferAsync(int id);
    Task<IEnumerable<object>> GetStockTransfersByProductAsync(int productId);
    Task<IEnumerable<object>> GetStockTransfersByLocationAsync(string fromLocation, string toLocation);
    Task<IEnumerable<object>> GetPendingTransfersAsync();
    Task ApproveTransferAsync(int transferId, string approvedBy);
    Task CompleteTransferAsync(int transferId, DateTime completionDate);
}
