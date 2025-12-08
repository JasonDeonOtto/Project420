using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository interface for StockTransfer entity operations.
/// SAHPRA Compliance: Stock transfers between locations for traceability.
/// </summary>
public interface IStockTransferRepository : IRepository<StockTransfer>
{
}
