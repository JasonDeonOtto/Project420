using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository implementation for StockTransfer entity operations.
/// SAHPRA Compliance: Stock transfers between locations for traceability.
/// </summary>
public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(InventoryDbContext context) : base(context)
    {
    }
}
