using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for PaymentTransaction entity.
/// Online payment processing with provider integration.
/// </summary>
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    /// <summary>
    /// Get all payment transactions for an order
    /// </summary>
    Task<IEnumerable<PaymentTransaction>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get payment by transaction reference from provider
    /// </summary>
    Task<PaymentTransaction?> GetByTransactionReferenceAsync(string transactionReference);

    /// <summary>
    /// Get payments by status
    /// </summary>
    Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status);

    /// <summary>
    /// Get payments by provider
    /// </summary>
    Task<IEnumerable<PaymentTransaction>> GetByProviderAsync(PaymentProvider provider);

    /// <summary>
    /// Get failed payments requiring investigation
    /// </summary>
    Task<IEnumerable<PaymentTransaction>> GetFailedPaymentsAsync();
}
