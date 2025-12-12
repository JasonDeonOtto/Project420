using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for OrderStatusHistory entity.
/// Provides immutable audit trail for order status changes.
/// </summary>
public class OrderStatusHistoryRepository : Repository<OrderStatusHistory>, IOrderStatusHistoryRepository
{
    public OrderStatusHistoryRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<OrderStatusHistory?> GetLatestStatusAsync(int orderId)
    {
        return await _dbSet
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetByNewStatusAsync(OnlineOrderStatus status)
    {
        return await _dbSet
            .Where(h => h.NewStatus == status)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}
