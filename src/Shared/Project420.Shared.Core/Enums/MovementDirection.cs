using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Direction of inventory movement for SOH (Stock on Hand) calculation.
/// </summary>
/// <remarks>
/// Movement Architecture (Option A):
/// - SOH is calculated as SUM(IN) - SUM(OUT) from the Movement ledger
/// - SOH is NEVER stored directly - always calculated from movements
/// - This ensures perfect audit trail and reconciliation capability
///
/// Design Decision:
/// - Using explicit direction instead of signed quantity improves clarity
/// - Makes aggregation queries simpler and more efficient
/// - Reduces risk of sign errors in calculations
/// </remarks>
public enum MovementDirection
{
    /// <summary>
    /// Stock coming IN to inventory (increases SOH).
    /// Examples: GRV, Refund, Production Output, Transfer In, Adjustment In
    /// </summary>
    [Description("IN")]
    In = 1,

    /// <summary>
    /// Stock going OUT of inventory (decreases SOH).
    /// Examples: Sale, RTS, Production Input, Transfer Out, Adjustment Out
    /// </summary>
    [Description("OUT")]
    Out = 2
}
