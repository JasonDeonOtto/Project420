using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for OnlineOrder entity.
/// Handles Click & Collect orders with Cannabis Act compliance.
/// </summary>
public class OnlineOrderRepository : Repository<OnlineOrder>, IOnlineOrderRepository
{
    public OnlineOrderRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<OnlineOrder?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<OnlineOrder>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OnlineOrder>> GetByStatusAsync(OnlineOrderStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .OrderBy(o => o.PreferredPickupDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OnlineOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OnlineOrder>> GetReadyForPickupAsync(int pickupLocationId)
    {
        return await _dbSet
            .Where(o => o.PickupLocationId == pickupLocationId
                     && o.Status == OnlineOrderStatus.ReadyForPickup)
            .OrderBy(o => o.PreferredPickupDate)
            .ThenBy(o => o.PreferredPickupTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<OnlineOrder>> GetPendingAgeVerificationAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OnlineOrderStatus.ReadyForPickup
                     && !o.AgeVerifiedAtPickup)
            .OrderBy(o => o.PreferredPickupDate)
            .ToListAsync();
    }

    public async Task<OnlineOrder?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.PaymentTransactions)
            .Include(o => o.StatusHistory)
            .Include(o => o.PickupConfirmation)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
