namespace Project420.OnlineOrders.Models.Enums;

/// <summary>
/// Online order status workflow
/// </summary>
public enum OnlineOrderStatus
{
    /// <summary>
    /// Order is being created (cart)
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Order created, awaiting payment
    /// </summary>
    PendingPayment = 10,

    /// <summary>
    /// Payment received, processing order
    /// </summary>
    PaymentReceived = 20,

    /// <summary>
    /// Order ready for customer pickup
    /// </summary>
    ReadyForPickup = 30,

    /// <summary>
    /// Order completed and picked up
    /// </summary>
    Completed = 40,

    /// <summary>
    /// Order cancelled by customer or system
    /// </summary>
    Cancelled = 90,

    /// <summary>
    /// Order refunded
    /// </summary>
    Refunded = 95,

    /// <summary>
    /// Order expired (not picked up in time)
    /// </summary>
    Expired = 99
}
