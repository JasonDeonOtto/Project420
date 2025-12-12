using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository implementation for PaymentTransaction entity.
/// Handles online payment processing with Yoco/PayFast/Ozow integration.
/// </summary>
public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(OnlineOrdersDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync();
    }

    public async Task<PaymentTransaction?> GetByTransactionReferenceAsync(string transactionReference)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ProviderTransactionId == transactionReference || p.ProviderReference == transactionReference);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByProviderAsync(PaymentProvider provider)
    {
        return await _dbSet
            .Where(p => p.Provider == provider)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentTransaction>> GetFailedPaymentsAsync()
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Failed)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync();
    }
}
