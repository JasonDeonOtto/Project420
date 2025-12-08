using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository implementation for StockCount entity operations.
/// SAHPRA/SARS Compliance: Physical stock count verification.
/// </summary>
public class StockCountRepository : Repository<StockCount>, IStockCountRepository
{
    public StockCountRepository(InventoryDbContext context) : base(context)
    {
    }
}
