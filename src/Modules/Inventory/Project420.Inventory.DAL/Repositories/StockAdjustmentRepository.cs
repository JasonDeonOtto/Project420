using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository implementation for StockAdjustment entity operations.
/// SARS Compliance: Stock adjustments for shrinkage/waste tracking.
/// </summary>
public class StockAdjustmentRepository : Repository<StockAdjustment>, IStockAdjustmentRepository
{
    public StockAdjustmentRepository(InventoryDbContext context) : base(context)
    {
    }
}
