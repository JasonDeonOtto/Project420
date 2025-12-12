using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the type of transaction being processed.
/// Used as discriminator for unified TransactionDetails table.
/// </summary>
/// <remarks>
/// Best Practice: Enums for fixed sets of related constants
/// - ONE TransactionHeader entity with a TransactionType field
/// - Not separate Sale, Refund, Receipt entities (that was duplication)
///
/// Cannabis Compliance: All transaction types must be tracked
/// for audit purposes (SAHPRA and SARS reporting requirements)
///
/// Movement Architecture (Option A):
/// - Each transaction type generates corresponding movements in the ledger
/// - IN transactions increase SOH, OUT transactions decrease SOH
/// - All stock changes tracked for seed-to-sale traceability
/// </remarks>
public enum TransactionType
{
    // ============================================================
    // RETAIL TRANSACTIONS (POS)
    // ============================================================

    /// <summary>
    /// Standard retail sale transaction (customer purchases products)
    /// Direction: OUT (decreases SOH)
    /// </summary>
    [Description("Sale")]
    Sale = 1,

    /// <summary>
    /// Refund transaction (customer returns products for refund)
    /// Direction: IN (increases SOH)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Refunds must be tracked for inventory
    /// and must reference original sale for audit trail
    /// </remarks>
    [Description("Refund")]
    Refund = 2,

    /// <summary>
    /// Customer payment on account (paying off debt, no products sold)
    /// Direction: N/A (no stock movement)
    /// </summary>
    [Description("Account Payment")]
    AccountPayment = 3,

    /// <summary>
    /// Layby/Layaway transaction (customer reserves products, pays over time)
    /// Direction: Reserved stock (may or may not create movement depending on policy)
    /// </summary>
    [Description("Layby")]
    Layby = 4,

    /// <summary>
    /// Quote/Estimate (not a final sale, for customer reference)
    /// Direction: N/A (no stock movement until converted to sale)
    /// </summary>
    [Description("Quote")]
    Quote = 5,

    // ============================================================
    // PURCHASING/RECEIVING TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Goods Received Voucher - stock received from supplier
    /// Direction: IN (increases SOH)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Must capture batch number, supplier details,
    /// and lab test certificates for seed-to-sale traceability
    /// </remarks>
    [Description("GRV")]
    GRV = 6,

    /// <summary>
    /// Return to Supplier - damaged/incorrect stock returned
    /// Direction: OUT (decreases SOH)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Reason must be documented for SAHPRA compliance
    /// </remarks>
    [Description("RTS")]
    RTS = 7,

    // ============================================================
    // WHOLESALE TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Wholesale sale to another business (B2B)
    /// Direction: OUT (decreases SOH)
    /// </summary>
    [Description("Wholesale Sale")]
    WholesaleSale = 8,

    /// <summary>
    /// Wholesale refund/return from another business
    /// Direction: IN (increases SOH)
    /// </summary>
    [Description("Wholesale Refund")]
    WholesaleRefund = 9,

    // ============================================================
    // PRODUCTION TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Production input - raw materials consumed in production
    /// Direction: OUT (raw materials decrease)
    /// </summary>
    /// <remarks>
    /// Links ProductionBatch to consumed HarvestBatch/inventory items
    /// </remarks>
    [Description("Production Input")]
    ProductionInput = 10,

    /// <summary>
    /// Production output - finished goods from production
    /// Direction: IN (finished products increase)
    /// </summary>
    /// <remarks>
    /// Creates new inventory with new batch numbers from production
    /// </remarks>
    [Description("Production Output")]
    ProductionOutput = 11,

    // ============================================================
    // TRANSFER TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Stock transfer out - leaving this location
    /// Direction: OUT (decreases SOH at source location)
    /// </summary>
    [Description("Transfer Out")]
    TransferOut = 12,

    /// <summary>
    /// Stock transfer in - arriving at this location
    /// Direction: IN (increases SOH at destination location)
    /// </summary>
    [Description("Transfer In")]
    TransferIn = 13,

    // ============================================================
    // ADJUSTMENT TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Stock adjustment increase (found stock, correction up)
    /// Direction: IN (increases SOH)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Reason must be documented for SAHPRA/SARS compliance
    /// </remarks>
    [Description("Adjustment In")]
    AdjustmentIn = 14,

    /// <summary>
    /// Stock adjustment decrease (lost/damaged/expired, correction down)
    /// Direction: OUT (decreases SOH)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Reason must be documented for SAHPRA/SARS compliance.
    /// Waste disposal must follow prescribed procedures.
    /// </remarks>
    [Description("Adjustment Out")]
    AdjustmentOut = 15,

    /// <summary>
    /// Stocktake variance - difference found during physical count
    /// Direction: IN or OUT (depends on variance sign - positive=IN, negative=OUT)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: All variances must be investigated and documented
    /// </remarks>
    [Description("Stocktake Variance")]
    StocktakeVariance = 16
}
