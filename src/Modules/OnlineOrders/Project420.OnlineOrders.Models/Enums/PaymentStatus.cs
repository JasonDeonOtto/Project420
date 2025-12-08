namespace Project420.OnlineOrders.Models.Enums;

/// <summary>
/// Payment transaction status
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment initiated, awaiting completion
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment processing
    /// </summary>
    Processing = 5,

    /// <summary>
    /// Payment successful
    /// </summary>
    Success = 10,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 20,

    /// <summary>
    /// Payment cancelled by customer
    /// </summary>
    Cancelled = 30,

    /// <summary>
    /// Payment refunded
    /// </summary>
    Refunded = 40,

    /// <summary>
    /// Partial refund processed
    /// </summary>
    PartiallyRefunded = 45
}
