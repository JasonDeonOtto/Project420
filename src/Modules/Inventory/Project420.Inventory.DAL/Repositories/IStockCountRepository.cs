using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository interface for StockCount entity operations.
/// SAHPRA/SARS Compliance: Physical stock count verification.
/// </summary>
public interface IStockCountRepository : IRepository<StockCount>
{
}
