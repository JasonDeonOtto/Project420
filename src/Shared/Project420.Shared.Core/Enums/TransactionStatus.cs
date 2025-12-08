using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the current status of a transaction in the POS system
/// </summary>
/// <remarks>
/// Best Practice: Use enums instead of magic strings for:
/// - Type safety (compiler catches errors)
/// - IntelliSense support
/// - Database efficiency (stores as integers)
/// - Easy to extend and maintain
/// </remarks>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction is being created but not yet finalized
    /// </summary>
    [Description("Pending")]
    Pending = 1,

    /// <summary>
    /// Transaction has been completed and payment received
    /// </summary>
    [Description("Completed")]
    Completed = 2,

    /// <summary>
    /// Transaction was cancelled before completion
    /// </summary>
    [Description("Cancelled")]
    Cancelled = 3,

    /// <summary>
    /// Transaction is on hold awaiting approval or payment
    /// </summary>
    [Description("On Hold")]
    OnHold = 4,

    /// <summary>
    /// Transaction has been fully or partially refunded
    /// </summary>
    [Description("Refunded")]
    Refunded = 5
}
