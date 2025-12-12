using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for PickupConfirmation entity.
/// Cannabis Act compliant age verification at pickup.
/// </summary>
public interface IPickupConfirmationRepository : IRepository<PickupConfirmation>
{
    /// <summary>
    /// Get pickup confirmation by order ID
    /// </summary>
    Task<PickupConfirmation?> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get pickups by staff member ID
    /// </summary>
    Task<IEnumerable<PickupConfirmation>> GetByStaffMemberIdAsync(int staffMemberId);

    /// <summary>
    /// Get pickups by date range
    /// </summary>
    Task<IEnumerable<PickupConfirmation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
