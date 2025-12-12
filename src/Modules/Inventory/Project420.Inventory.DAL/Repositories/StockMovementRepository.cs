using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository implementation for StockMovement entity operations.
/// SARS Compliance: Stock movement tracking for tax reconciliation.
/// </summary>
public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(InventoryDbContext context) : base(context)
    {
    }


}
