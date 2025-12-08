using Project420.Inventory.DAL.Repositories;

namespace Project420.Inventory.BLL.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _repository;

    public StockTransferService(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateStockTransferAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateStockTransferAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetStockTransferByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllStockTransfersAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateStockTransferAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetStockTransfersByProductAsync(int productId)
    {
        // StockTransfer uses denormalized data (ProductSKU), not ProductId FK
        // Will implement with DTOs
        return Enumerable.Empty<object>();
    }

    public async Task<IEnumerable<object>> GetStockTransfersByLocationAsync(string fromLocation, string toLocation)
    {
        var transfers = await _repository.FindAsync(t =>
            (string.IsNullOrEmpty(fromLocation) || t.FromLocation == fromLocation) &&
            (string.IsNullOrEmpty(toLocation) || t.ToLocation == toLocation));
        return transfers.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetPendingTransfersAsync()
    {
        var pending = await _repository.FindAsync(t => t.Status == "Pending" || t.Status == "In Transit");
        return pending.Cast<object>();
    }

    public async Task ApproveTransferAsync(int transferId, string approvedBy)
    {
        var transfer = await _repository.GetByIdAsync(transferId);
        if (transfer == null) throw new InvalidOperationException($"Stock transfer {transferId} not found");

        transfer.AuthorizedBy = approvedBy; // Entity uses AuthorizedBy, not ApprovedBy
        transfer.Status = "Approved";

        await _repository.UpdateAsync(transfer);
    }

    public async Task CompleteTransferAsync(int transferId, DateTime completionDate)
    {
        var transfer = await _repository.GetByIdAsync(transferId);
        if (transfer == null) throw new InvalidOperationException($"Stock transfer {transferId} not found");

        // Entity doesn't have CompletedDate - just update status
        transfer.Status = "Received";

        await _repository.UpdateAsync(transfer);
    }
}
