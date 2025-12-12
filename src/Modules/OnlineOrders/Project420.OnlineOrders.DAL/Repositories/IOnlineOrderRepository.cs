using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for OnlineOrder entity.
/// Cannabis Act compliant Click & Collect orders.
/// </summary>
public interface IOnlineOrderRepository : IRepository<OnlineOrder>
{
    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<OnlineOrder?> GetByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    Task<IEnumerable<OnlineOrder>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<OnlineOrder>> GetByStatusAsync(OnlineOrderStatus status);

    /// <summary>
    /// Get orders by date range
    /// </summary>
    Task<IEnumerable<OnlineOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get orders ready for pickup (status = ReadyForPickup)
    /// </summary>
    Task<IEnumerable<OnlineOrder>> GetReadyForPickupAsync(int pickupLocationId);

    /// <summary>
    /// Get orders pending age verification at pickup
    /// </summary>
    Task<IEnumerable<OnlineOrder>> GetPendingAgeVerificationAsync();

    /// <summary>
    /// Get order with all related entities (items, payments, status history)
    /// </summary>
    Task<OnlineOrder?> GetOrderWithDetailsAsync(int orderId);
}
