using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for OnlineOrderItem entity.
/// Handles order line items with cannabis compliance tracking.
/// </summary>
public class OnlineOrderItemRepository : Repository<OnlineOrderItem>, IOnlineOrderItemRepository
{
    public OnlineOrderItemRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OnlineOrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .Where(i => i.OrderId == orderId)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<OnlineOrderItem>> GetByProductSkuAsync(string productSku)
    {
        return await _dbSet
            .Where(i => i.ProductCode == productSku)
            .ToListAsync();
    }
}
