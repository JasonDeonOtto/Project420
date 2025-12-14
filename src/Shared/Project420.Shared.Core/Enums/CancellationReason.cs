using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Reasons for transaction cancellation (void)
/// </summary>
/// <remarks>
/// Transaction cancellation is distinct from refunds:
/// - Cancellation: Void the entire transaction (before customer leaves)
/// - Refund: Return products and refund payment (after customer has left)
///
/// SAHPRA/Cannabis Act Compliance:
/// - All cancellations must be logged with reason for audit trail
/// - Manager approval required for completed transactions
/// - Stock movements are reversed automatically (soft delete)
///
/// Phase 9.6: Transaction Cancellation Workflow
/// </remarks>
public enum CancellationReason
{
    /// <summary>
    /// Customer changed their mind before completing sale
    /// </summary>
    [Description("Customer Changed Mind")]
    CustomerRequest = 1,

    /// <summary>
    /// Cashier made an error (wrong items, wrong quantity)
    /// </summary>
    [Description("Cashier Error - Wrong Item/Quantity")]
    CashierError = 2,

    /// <summary>
    /// Payment was declined or failed
    /// </summary>
    [Description("Payment Declined/Failed")]
    PaymentFailed = 3,

    /// <summary>
    /// Price or promotion error discovered
    /// </summary>
    [Description("Price Error")]
    PriceError = 4,

    /// <summary>
    /// Customer failed age verification after sale started
    /// </summary>
    /// <remarks>
    /// Cannabis Act requirement: Must verify 18+ before sale.
    /// If verification fails mid-transaction, must cancel.
    /// </remarks>
    [Description("Age Verification Failed")]
    AgeVerificationFailed = 5,

    /// <summary>
    /// Product out of stock or unavailable
    /// </summary>
    [Description("Product Unavailable")]
    ProductUnavailable = 6,

    /// <summary>
    /// Transaction created in error (duplicate, test, etc.)
    /// </summary>
    [Description("Duplicate/Test Transaction")]
    DuplicateTransaction = 7,

    /// <summary>
    /// System or technical error requiring cancellation
    /// </summary>
    [Description("System/Technical Error")]
    SystemError = 8,

    /// <summary>
    /// Manager-initiated cancellation with override
    /// </summary>
    /// <remarks>
    /// Requires manager PIN and explicit reason in notes.
    /// Used for exceptional circumstances not covered by other reasons.
    /// </remarks>
    [Description("Manager Override")]
    ManagerOverride = 9,

    /// <summary>
    /// Compliance issue discovered (e.g., possession limit)
    /// </summary>
    /// <remarks>
    /// Cannabis Act compliance: Cannot sell if customer exceeds
    /// possession limits or other regulatory violations.
    /// </remarks>
    [Description("Compliance Issue")]
    ComplianceIssue = 10,

    /// <summary>
    /// Fraud suspected or detected
    /// </summary>
    /// <remarks>
    /// Requires manager override and incident report.
    /// </remarks>
    [Description("Fraud Suspected")]
    FraudSuspected = 11
}
