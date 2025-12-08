using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the method of payment used for a transaction
/// </summary>
/// <remarks>
/// Best Practice: Separate Payment entity from Transaction entity
/// - Transaction = What they're buying (invoice/receipt)
/// - Payment = How they're paying (can be multiple payments per transaction)
///
/// Example: R200 sale paid with R100 cash + R100 card = 1 Transaction, 2 Payments
///
/// Cannabis Compliance: All payment methods must be tracked for:
/// - SARS tax reporting (VAT201)
/// - Cash handling compliance
/// - Financial audit trail
/// </remarks>
public enum PaymentMethod
{
    /// <summary>
    /// Cash payment (physical money)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Cash transactions over R25,000 must be reported
    /// to the Financial Intelligence Centre (FIC) for anti-money laundering
    /// </remarks>
    [Description("Cash")]
    Cash = 1,

    /// <summary>
    /// Card payment (credit or debit card)
    /// </summary>
    [Description("Card")]
    Card = 2,

    /// <summary>
    /// Electronic Funds Transfer (bank transfer)
    /// </summary>
    [Description("EFT")]
    EFT = 3,

    /// <summary>
    /// Mobile payment (e.g., SnapScan, Zapper, Apple Pay, Google Pay)
    /// </summary>
    [Description("Mobile Payment")]
    MobilePayment = 4,

    /// <summary>
    /// Payment on account (customer has credit terms)
    /// </summary>
    /// <remarks>
    /// This doesn't collect money now, adds to customer's debt
    /// </remarks>
    [Description("On Account")]
    OnAccount = 5,

    /// <summary>
    /// Voucher or gift card redemption
    /// </summary>
    [Description("Voucher")]
    Voucher = 6,

    /// <summary>
    /// Loyalty points redemption
    /// </summary>
    /// <remarks>
    /// Customer loyalty program points used as payment tender.
    /// Points are converted to Rand value at redemption time.
    /// </remarks>
    [Description("Loyalty Points")]
    LoyaltyPoints = 7
}
