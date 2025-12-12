using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for CustomerAccount entity.
/// POPIA compliant with consent tracking and age verification.
/// </summary>
public class CustomerAccountRepository : Repository<CustomerAccount>, ICustomerAccountRepository
{
    public CustomerAccountRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<CustomerAccount?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<CustomerAccount?> GetByIdNumberAsync(string idNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.IdNumber == idNumber);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(c => c.Email == email);
    }

    public async Task<IEnumerable<CustomerAccount>> GetPendingAgeVerificationAsync()
    {
        return await _dbSet
            .Where(c => !c.AgeVerified && c.IsActive)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomerAccount>> GetCustomersWithActiveOrdersAsync()
    {
        return await _dbSet
            .Include(c => c.Orders)
            .Where(c => c.Orders.Any(o =>
                o.Status == OnlineOrderStatus.PendingPayment ||
                o.Status == OnlineOrderStatus.PaymentReceived ||
                o.Status == OnlineOrderStatus.ReadyForPickup))
            .ToListAsync();
    }

    public async Task UpdateLastLoginAsync(int customerId)
    {
        var customer = await GetByIdAsync(customerId);
        if (customer != null)
        {
            customer.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
