using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Batch type codes for the 16-digit batch numbering system.
/// These codes form the TT component of the batch number format: SSTTYYYYMMDDNNNN
/// </summary>
/// <remarks>
/// Batch Number Format: SSTTYYYYMMDDNNNN (16 digits)
/// - SS: Site ID (01-99)
/// - TT: Batch Type (this enum, 2 digits)
/// - YYYYMMDD: Date
/// - NNNN: Daily sequence per site/type
///
/// Cannabis Compliance:
/// - Each batch type serves a specific business purpose
/// - Type codes enable instant visual identification
/// - Critical for SAHPRA seed-to-sale traceability
/// - Supports audit trail and compliance reporting
///
/// Example: 0110202512060001
/// - Site: 01
/// - Type: 10 (Production)
/// - Date: 2025-12-06
/// - Sequence: 0001
/// </remarks>
public enum BatchType
{
    /// <summary>
    /// Production batch - manufacturing, growing, or processing operations.
    /// Used when creating new inventory from raw materials or cultivation.
    /// </summary>
    [Description("Production")]
    Production = 10,

    /// <summary>
    /// Transfer batch - inter-location stock movements.
    /// Used when moving stock between sites or warehouses.
    /// </summary>
    [Description("Transfer")]
    Transfer = 20,

    /// <summary>
    /// Stock take batch - physical inventory counts.
    /// Used to track stock count operations and reconciliation.
    /// </summary>
    [Description("Stock Take")]
    StockTake = 30,

    /// <summary>
    /// Adjustment batch - inventory corrections and adjustments.
    /// Used for manual corrections to stock levels (with documented reasons).
    /// </summary>
    [Description("Adjustment")]
    Adjustment = 40,

    /// <summary>
    /// Return to supplier batch - goods returned to vendors.
    /// Used when returning defective or excess stock to suppliers.
    /// </summary>
    [Description("Return to Supplier")]
    ReturnToSupplier = 50,

    /// <summary>
    /// Destruction/Waste batch - product disposal operations.
    /// SAHPRA requires documentation of all cannabis waste/destruction.
    /// </summary>
    [Description("Destruction/Waste")]
    Destruction = 60,

    /// <summary>
    /// Customer return batch - goods returned by customers.
    /// Used for refunds and customer return processing.
    /// </summary>
    [Description("Customer Return")]
    CustomerReturn = 70,

    /// <summary>
    /// Quarantine batch - stock held for quality or compliance review.
    /// Used for isolation pending QC, lab testing, or investigation.
    /// </summary>
    [Description("Quarantine")]
    Quarantine = 80,

    /// <summary>
    /// Reserved for future use.
    /// Placeholder for additional batch types as business needs evolve.
    /// </summary>
    [Description("Reserved")]
    Reserved = 90
}
