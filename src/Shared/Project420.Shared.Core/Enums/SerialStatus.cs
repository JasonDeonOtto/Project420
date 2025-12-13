using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Lifecycle status of a serial number for unit-level tracking.
/// Tracks the state of individual units from creation to final disposition.
/// </summary>
/// <remarks>
/// Cannabis Compliance:
/// - SAHPRA requires tracking of every unit's status
/// - Critical for seed-to-sale traceability
/// - Enables recall management and quality control
/// - Supports audit trail for regulatory compliance
///
/// State Flow:
/// Created → Assigned → InStock → Sold/Transferred/Destroyed
///
/// Important Rules:
/// - Serial numbers are immutable once created
/// - Status changes create audit trail entries
/// - Destroyed status requires documented reason (SAHPRA)
/// </remarks>
public enum SerialStatus
{
    /// <summary>
    /// Serial number generated but not yet assigned to physical product.
    /// Initial state after generation during production planning.
    /// </summary>
    [Description("Created")]
    Created = 1,

    /// <summary>
    /// Serial number assigned to a specific product unit during packaging.
    /// Product has serial label applied but not yet in sellable inventory.
    /// </summary>
    [Description("Assigned")]
    Assigned = 2,

    /// <summary>
    /// Product is in sellable inventory, ready for sale or distribution.
    /// Standard state for retail-ready products.
    /// </summary>
    [Description("In Stock")]
    InStock = 3,

    /// <summary>
    /// Product sold to end customer.
    /// Final state for retail sales - serial number retired for this product.
    /// </summary>
    [Description("Sold")]
    Sold = 4,

    /// <summary>
    /// Product transferred to another location or facility.
    /// Intermediate state - serial retains identity but location changes.
    /// </summary>
    [Description("Transferred")]
    Transferred = 5,

    /// <summary>
    /// Product destroyed or disposed of (waste, expired, damaged).
    /// SAHPRA requires documented destruction with witness for cannabis.
    /// </summary>
    [Description("Destroyed")]
    Destroyed = 6,

    /// <summary>
    /// Product quarantined pending QC, investigation, or lab testing.
    /// Cannot be sold until released from quarantine.
    /// </summary>
    [Description("Quarantined")]
    Quarantined = 7,

    /// <summary>
    /// Product recalled due to quality or safety concerns.
    /// Triggers recall management workflow.
    /// </summary>
    [Description("Recalled")]
    Recalled = 8,

    /// <summary>
    /// Product returned by customer.
    /// May transition to Destroyed, InStock (if resellable), or Quarantined.
    /// </summary>
    [Description("Returned")]
    Returned = 9
}
