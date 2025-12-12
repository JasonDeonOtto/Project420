using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for OrderStatusHistory entity.
/// Audit trail for order status transitions.
/// </summary>
public interface IOrderStatusHistoryRepository : IRepository<OrderStatusHistory>
{
    /// <summary>
    /// Get status history for a specific order
    /// </summary>
    Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get latest status for an order
    /// </summary>
    Task<OrderStatusHistory?> GetLatestStatusAsync(int orderId);

    /// <summary>
    /// Get orders that transitioned to a specific status
    /// </summary>
    Task<IEnumerable<OrderStatusHistory>> GetByNewStatusAsync(OnlineOrderStatus status);
}
