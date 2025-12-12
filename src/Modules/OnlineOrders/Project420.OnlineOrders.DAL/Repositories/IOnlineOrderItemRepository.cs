using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for OnlineOrderItem entity.
/// Order line items with product details.
/// </summary>
public interface IOnlineOrderItemRepository : IRepository<OnlineOrderItem>
{
    /// <summary>
    /// Get all items for a specific order
    /// </summary>
    Task<IEnumerable<OnlineOrderItem>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get items by product SKU
    /// </summary>
    Task<IEnumerable<OnlineOrderItem>> GetByProductSkuAsync(string productSku);
}
