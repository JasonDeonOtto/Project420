using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the reason for a refund/return transaction
/// </summary>
/// <remarks>
/// Cannabis Act Compliance:
/// - All refunds must be tracked with reason for audit purposes
/// - Defective products must be recorded for waste tracking (SAHPRA reporting)
/// - Compliance issues require immediate escalation
///
/// SARS VAT Compliance:
/// - Refunds affect VAT calculations (must reverse original VAT)
/// - Audit trail required for all financial adjustments
///
/// Best Practice:
/// - Use enums for fixed sets of business reasons
/// - Clear descriptions for reporting and audit logs
/// </remarks>
public enum RefundReason
{
    /// <summary>
    /// Customer changed their mind (no defect)
    /// </summary>
    /// <remarks>
    /// Standard retail return within return period.
    /// Product condition must be verified before accepting.
    /// </remarks>
    [Description("Customer Request - Changed Mind")]
    CustomerRequest = 1,

    /// <summary>
    /// Product was damaged, defective, or quality issue
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Must be recorded as waste for SAHPRA reporting.
    /// Product must be disposed of according to Cannabis Act regulations.
    /// </remarks>
    [Description("Defective/Damaged Product")]
    DefectiveProduct = 2,

    /// <summary>
    /// Wrong product was sold to customer
    /// </summary>
    /// <remarks>
    /// Staff error - requires retraining/review.
    /// Customer should receive correct product immediately.
    /// </remarks>
    [Description("Wrong Product Sold")]
    WrongProduct = 3,

    /// <summary>
    /// Price error or overcharge
    /// </summary>
    /// <remarks>
    /// Pricing system error or manual entry mistake.
    /// Requires immediate correction to maintain customer trust.
    /// </remarks>
    [Description("Price Error/Overcharge")]
    PriceError = 4,

    /// <summary>
    /// Manager override or discretionary refund
    /// </summary>
    /// <remarks>
    /// Requires manager approval and notes.
    /// Used for customer service exceptions.
    /// </remarks>
    [Description("Manager Override")]
    ManagerOverride = 5,

    /// <summary>
    /// Cannabis Act compliance issue (age verification, license, etc.)
    /// </summary>
    /// <remarks>
    /// CRITICAL: Sale should not have occurred.
    /// Examples:
    /// - Age verification was missed
    /// - Medical license was invalid/expired
    /// - Possession limits were exceeded
    /// Requires immediate reporting and process review.
    /// </remarks>
    [Description("Compliance Issue - Cannabis Act")]
    ComplianceIssue = 6,

    /// <summary>
    /// Duplicate transaction (same sale processed twice)
    /// </summary>
    /// <remarks>
    /// System or human error - payment was charged multiple times.
    /// Requires immediate refund and system investigation.
    /// </remarks>
    [Description("Duplicate Transaction")]
    DuplicateTransaction = 7,

    /// <summary>
    /// Product expired or approaching expiry
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Expired products must be disposed as waste.
    /// Should not have been sold - requires inventory process review.
    /// </remarks>
    [Description("Expired Product")]
    ExpiredProduct = 8,

    /// <summary>
    /// Other reason (requires detailed notes)
    /// </summary>
    /// <remarks>
    /// Catch-all for unusual situations.
    /// MUST include detailed notes in refund transaction.
    /// </remarks>
    [Description("Other - See Notes")]
    Other = 99
}
