using Project420.Inventory.DAL.Repositories.Common;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.DAL.Repositories;

/// <summary>
/// Repository interface for StockMovement entity operations.
/// SARS Compliance: Stock movement tracking for tax reconciliation.
/// </summary>
public interface IStockMovementRepository : IRepository<StockMovement>
{
}
