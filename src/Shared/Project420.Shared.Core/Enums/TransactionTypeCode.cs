namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the type code prefix for transaction number generation.
/// Used to generate unique transaction numbers across all transaction types.
/// </summary>
/// <remarks>
/// Transaction Number Format: {TYPE}-{YYYYMMDD}-{XXX}
/// Examples:
/// - SALE-20251205-001 (POS Sale)
/// - GRV-20251205-001 (Goods Received Note)
/// - RTS-20251205-001 (Return to Supplier)
/// - INV-20251205-001 (Sales Invoice)
/// - CRN-20251205-001 (Credit Note)
/// - ADJ-20251205-001 (Stock Adjustment)
/// - PAY-20251205-001 (Payment)
/// </remarks>
public enum TransactionTypeCode
{
    /// <summary>
    /// Point of Sale transaction (retail sale).
    /// Example: SALE-20251205-001
    /// </summary>
    SALE = 1,

    /// <summary>
    /// Goods Received Note (stock received from supplier).
    /// Example: GRV-20251205-001
    /// </summary>
    GRV = 2,

    /// <summary>
    /// Return to Supplier (defective/unwanted stock returned).
    /// Example: RTS-20251205-001
    /// </summary>
    RTS = 3,

    /// <summary>
    /// Sales Invoice (formal invoice for account customer).
    /// Example: INV-20251205-001
    /// </summary>
    INV = 4,

    /// <summary>
    /// Credit Note (refund/credit to customer).
    /// Example: CRN-20251205-001
    /// </summary>
    CRN = 5,

    /// <summary>
    /// Stock Adjustment (internal stock correction).
    /// Example: ADJ-20251205-001
    /// </summary>
    ADJ = 6,

    /// <summary>
    /// Payment (customer payment received).
    /// Example: PAY-20251205-001
    /// </summary>
    PAY = 7,

    /// <summary>
    /// Quote (sales quotation, non-binding).
    /// Example: QTE-20251205-001
    /// </summary>
    QTE = 8,

    /// <summary>
    /// Layby (deposit payment for future collection).
    /// Example: LAY-20251205-001
    /// </summary>
    LAY = 9,

    /// <summary>
    /// Transfer (stock transfer between locations).
    /// Example: TRF-20251205-001
    /// </summary>
    TRF = 10,

    /// <summary>
    /// Online Order (customer web order).
    /// Example: ORD-20251208-001
    /// </summary>
    ORD = 11
}
