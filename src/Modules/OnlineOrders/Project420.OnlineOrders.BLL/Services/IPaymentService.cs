using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service interface for payment processing business logic.
/// Integrates with SA payment providers: Yoco, PayFast, Ozow.
/// SARS compliance: Tax calculations and receipt generation.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a payment for an order.
    /// Generates payment URL for customer to complete payment.
    /// </summary>
    /// <param name="dto">Payment request data</param>
    /// <returns>Payment response with payment URL</returns>
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto);

    /// <summary>
    /// Gets all payment transactions for an order.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of payment transactions</returns>
    Task<IEnumerable<PaymentResponseDto>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets a payment transaction by provider reference.
    /// </summary>
    /// <param name="providerReference">Payment provider reference</param>
    /// <returns>Payment transaction details</returns>
    Task<PaymentResponseDto?> GetByReferenceAsync(string providerReference);

    /// <summary>
    /// Processes a refund for a payment.
    /// </summary>
    /// <param name="paymentId">Payment transaction ID</param>
    /// <param name="amount">Refund amount</param>
    /// <returns>Refund transaction details</returns>
    Task<PaymentResponseDto> RefundPaymentAsync(int paymentId, decimal amount);
}
