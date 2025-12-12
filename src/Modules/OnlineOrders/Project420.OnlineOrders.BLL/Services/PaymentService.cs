using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;
using Project420.OnlineOrders.DAL.Repositories;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service for payment processing business logic.
/// Integrates with SA payment providers: Yoco, PayFast, Ozow.
/// SARS compliance: Tax calculations and receipt generation.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly IValidator<PaymentRequestDto> _paymentValidator;

    public PaymentService(
        IPaymentTransactionRepository paymentRepository,
        IOnlineOrderRepository orderRepository,
        IValidator<PaymentRequestDto> paymentValidator)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _paymentValidator = paymentValidator;
    }

    /// <summary>
    /// Processes a payment for an order.
    /// Generates payment URL for customer to complete payment.
    /// </summary>
    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _paymentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        // STEP 2: Verify order exists
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = "Order not found"
            };
        }

        // STEP 3: Check if order is already paid
        if (order.PaymentDate.HasValue)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = "Order has already been paid"
            };
        }

        // STEP 4: Create payment transaction record
        var payment = new PaymentTransaction
        {
            OrderId = dto.OrderId,
            Provider = dto.Provider,
            Amount = order.TotalAmount,
            Currency = "ZAR",
            Status = PaymentStatus.Pending,
            InitiatedAt = DateTime.UtcNow,
            CreatedBy = $"Order_{dto.OrderId}",
            ModifiedBy = $"Order_{dto.OrderId}"
        };

        var createdPayment = await _paymentRepository.AddAsync(payment);

        // STEP 5: Generate payment URL based on provider
        string paymentUrl = GeneratePaymentUrl(dto.Provider, createdPayment.Id, order, dto);

        // STEP 6: Update order with payment reference
        order.PaymentProvider = dto.Provider;
        order.PaymentReference = $"PAY_{createdPayment.Id}";
        await _orderRepository.UpdateAsync(order);

        // STEP 7: Return response
        return new PaymentResponseDto
        {
            Success = true,
            TransactionId = createdPayment.Id,
            PaymentUrl = paymentUrl,
            Provider = dto.Provider,
            Amount = order.TotalAmount,
            Currency = "ZAR",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    /// <summary>
    /// Gets all payment transactions for an order.
    /// </summary>
    public async Task<IEnumerable<PaymentResponseDto>> GetByOrderIdAsync(int orderId)
    {
        var payments = await _paymentRepository.FindAsync(p => p.OrderId == orderId);
        return payments.Select(MapToDto);
    }

    /// <summary>
    /// Gets a payment transaction by provider reference.
    /// </summary>
    public async Task<PaymentResponseDto?> GetByReferenceAsync(string providerReference)
    {
        var payments = await _paymentRepository.FindAsync(p =>
            p.ProviderReference == providerReference ||
            p.ProviderTransactionId == providerReference);
        var payment = payments.FirstOrDefault();
        return payment != null ? MapToDto(payment) : null;
    }

    /// <summary>
    /// Processes a refund for a payment.
    /// </summary>
    public async Task<PaymentResponseDto> RefundPaymentAsync(int paymentId, decimal amount)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = "Payment transaction not found"
            };
        }

        if (payment.Status != PaymentStatus.Success)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = "Can only refund successful payments"
            };
        }

        if (amount > payment.Amount)
        {
            return new PaymentResponseDto
            {
                Success = false,
                ErrorMessage = "Refund amount cannot exceed payment amount"
            };
        }

        // TODO: Implement actual refund processing with payment provider

        // Create refund transaction record
        var refund = new PaymentTransaction
        {
            OrderId = payment.OrderId,
            Provider = payment.Provider,
            Amount = -amount, // Negative amount for refund
            Currency = payment.Currency,
            Status = PaymentStatus.Refunded,
            InitiatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            CreatedBy = "System",
            ModifiedBy = "System"
        };

        var createdRefund = await _paymentRepository.AddAsync(refund);

        return new PaymentResponseDto
        {
            Success = true,
            TransactionId = createdRefund.Id,
            Provider = payment.Provider,
            Amount = amount,
            Currency = payment.Currency
        };
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Generates payment URL based on provider.
    /// TODO: Implement actual API calls to payment providers.
    /// </summary>
    private string GeneratePaymentUrl(PaymentProvider provider, int paymentId, OnlineOrder order, PaymentRequestDto dto)
    {
        return provider switch
        {
            PaymentProvider.Yoco => GenerateYocoPaymentUrl(paymentId, order, dto),
            PaymentProvider.PayFast => GeneratePayFastPaymentUrl(paymentId, order, dto),
            PaymentProvider.Ozow => GenerateOzowPaymentUrl(paymentId, order, dto),
            _ => throw new InvalidOperationException($"Unsupported payment provider: {provider}")
        };
    }

    /// <summary>
    /// Generates Yoco payment URL.
    /// TODO: Implement actual Yoco API integration.
    /// </summary>
    private string GenerateYocoPaymentUrl(int paymentId, OnlineOrder order, PaymentRequestDto dto)
    {
        // TODO: Call Yoco API to create payment
        return $"https://payments.yoco.com/pay/{paymentId}?amount={order.TotalAmount:F2}&return_url={dto.ReturnUrl}";
    }

    /// <summary>
    /// Generates PayFast payment URL.
    /// TODO: Implement actual PayFast API integration.
    /// </summary>
    private string GeneratePayFastPaymentUrl(int paymentId, OnlineOrder order, PaymentRequestDto dto)
    {
        // TODO: Call PayFast API to create payment
        return $"https://www.payfast.co.za/eng/process?amount={order.TotalAmount:F2}&item_name=Order_{order.OrderNumber}&return_url={dto.ReturnUrl}";
    }

    /// <summary>
    /// Generates Ozow payment URL.
    /// TODO: Implement actual Ozow API integration.
    /// </summary>
    private string GenerateOzowPaymentUrl(int paymentId, OnlineOrder order, PaymentRequestDto dto)
    {
        // TODO: Call Ozow API to create payment
        return $"https://pay.ozow.com/?amount={order.TotalAmount:F2}&reference={order.OrderNumber}&return_url={dto.ReturnUrl}";
    }

    /// <summary>
    /// Maps PaymentTransaction entity to PaymentResponseDto.
    /// </summary>
    private PaymentResponseDto MapToDto(PaymentTransaction payment)
    {
        return new PaymentResponseDto
        {
            Success = payment.Status == PaymentStatus.Success,
            TransactionId = payment.Id,
            Provider = payment.Provider,
            Amount = payment.Amount,
            Currency = payment.Currency,
            ErrorMessage = payment.ErrorMessage
        };
    }
}
