using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the type of transaction being processed
/// </summary>
/// <remarks>
/// Best Practice: Enums for fixed sets of related constants
/// - ONE TransactionHeader entity with a TransactionType field
/// - Not separate Sale, Refund, Receipt entities (that was duplication)
///
/// Cannabis Compliance: All transaction types must be tracked
/// for audit purposes (SAHPRA and SARS reporting requirements)
/// </remarks>
public enum TransactionType
{
    /// <summary>
    /// Standard sale transaction (customer purchases products)
    /// </summary>
    [Description("Sale")]
    Sale = 1,

    /// <summary>
    /// Refund transaction (customer returns products for refund)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Refunds must be tracked for inventory
    /// and must reference original sale for audit trail
    /// </remarks>
    [Description("Refund")]
    Refund = 2,

    /// <summary>
    /// Customer payment on account (paying off debt, no products sold)
    /// </summary>
    [Description("Account Payment")]
    AccountPayment = 3,

    /// <summary>
    /// Layby/Layaway transaction (customer reserves products, pays over time)
    /// </summary>
    [Description("Layby")]
    Layby = 4,

    /// <summary>
    /// Quote/Estimate (not a final sale, for customer reference)
    /// </summary>
    [Description("Quote")]
    Quote = 5
}
