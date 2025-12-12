using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for PickupConfirmation entity.
/// Handles Cannabis Act compliant age verification and ID checks at pickup.
/// </summary>
public class PickupConfirmationRepository : Repository<PickupConfirmation>, IPickupConfirmationRepository
{
    public PickupConfirmationRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<PickupConfirmation?> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<IEnumerable<PickupConfirmation>> GetByStaffMemberIdAsync(int staffMemberId)
    {
        return await _dbSet
            .Where(p => p.VerifiedByStaffId == staffMemberId)
            .OrderByDescending(p => p.PickupDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PickupConfirmation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(p => p.PickupDate >= startDate && p.PickupDate <= endDate)
            .OrderByDescending(p => p.PickupDate)
            .ToListAsync();
    }
}
