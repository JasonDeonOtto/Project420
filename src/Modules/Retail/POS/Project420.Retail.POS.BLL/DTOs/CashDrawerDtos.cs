using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs;

// ========================================
// CASH DRAWER SESSION DTOs
// ========================================

/// <summary>
/// DTO for cash drawer session information
/// </summary>
public class CashDrawerSessionDto
{
    /// <summary>
    /// Unique session ID
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Cashier user ID
    /// </summary>
    public int CashierId { get; set; }

    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Session opened date/time
    /// </summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>
    /// Session closed date/time (if closed)
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Session status (Open, Closed, Reconciled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Opening float amount
    /// </summary>
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// Expected cash on hand (calculated)
    /// </summary>
    public decimal ExpectedCash { get; set; }

    /// <summary>
    /// Actual cash counted (at close)
    /// </summary>
    public decimal? ActualCash { get; set; }

    /// <summary>
    /// Variance (actual - expected)
    /// </summary>
    public decimal? Variance { get; set; }

    /// <summary>
    /// Number of transactions in session
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Cash movements during session
    /// </summary>
    public List<CashMovementDto> CashMovements { get; set; } = new();

    /// <summary>
    /// Denomination breakdown (at open or close)
    /// </summary>
    public DenominationBreakdownDto? DenominationBreakdown { get; set; }
}

/// <summary>
/// Request DTO for opening a cash drawer
/// </summary>
public class CashDrawerOpenRequestDto
{
    /// <summary>
    /// Cashier opening the drawer
    /// </summary>
    public int CashierId { get; set; }

    /// <summary>
    /// Opening float amount
    /// </summary>
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// Denomination breakdown of opening float
    /// </summary>
    public DenominationBreakdownDto DenominationBreakdown { get; set; } = new();

    /// <summary>
    /// Manager who verified the float (if applicable)
    /// </summary>
    public int? VerifiedByManagerId { get; set; }

    /// <summary>
    /// Notes about opening (optional)
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for closing and reconciling a cash drawer
/// </summary>
public class CashDrawerCloseRequestDto
{
    /// <summary>
    /// Session ID to close
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Actual cash counted
    /// </summary>
    public decimal ActualCash { get; set; }

    /// <summary>
    /// Denomination breakdown of counted cash
    /// </summary>
    public DenominationBreakdownDto DenominationBreakdown { get; set; } = new();

    /// <summary>
    /// Cashier closing the drawer
    /// </summary>
    public int ClosedByCashierId { get; set; }

    /// <summary>
    /// Manager who approved reconciliation (if variance requires approval)
    /// </summary>
    public int? ApprovedByManagerId { get; set; }

    /// <summary>
    /// Reason for variance (if any)
    /// </summary>
    public string? VarianceReason { get; set; }

    /// <summary>
    /// Notes about closing (optional)
    /// </summary>
    public string? Notes { get; set; }
}

// ========================================
// CASH MOVEMENT DTOs
// ========================================

/// <summary>
/// DTO for cash movement (drop, loan, etc.)
/// </summary>
public class CashMovementDto
{
    /// <summary>
    /// Movement ID
    /// </summary>
    public int MovementId { get; set; }

    /// <summary>
    /// Movement date/time
    /// </summary>
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// Movement type (Drop, Loan, PettyCash, Correction)
    /// </summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// Amount (positive = add to drawer, negative = remove from drawer)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Reason for movement
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// User who performed the movement
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Manager who approved the movement (if required)
    /// </summary>
    public string? ApprovedBy { get; set; }
}

/// <summary>
/// Request DTO for recording a cash movement
/// </summary>
public class CashMovementRequestDto
{
    /// <summary>
    /// Movement type
    /// </summary>
    public CashMovementType MovementType { get; set; }

    /// <summary>
    /// Amount (positive = add, negative = remove)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Reason for movement
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// User performing the movement
    /// </summary>
    public int PerformedByUserId { get; set; }

    /// <summary>
    /// Manager approving the movement (if required)
    /// </summary>
    public int? ApprovedByManagerId { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Cash movement types
/// </summary>
public enum CashMovementType
{
    /// <summary>
    /// Cash drop to safe (remove excess cash)
    /// </summary>
    Drop,

    /// <summary>
    /// Cash loan from safe (add cash when running low)
    /// </summary>
    Loan,

    /// <summary>
    /// Petty cash payment (remove for expenses)
    /// </summary>
    PettyCash,

    /// <summary>
    /// Correction (adjust for errors)
    /// </summary>
    Correction
}

// ========================================
// RECONCILIATION DTOs
// ========================================

/// <summary>
/// DTO for expected reconciliation amounts
/// </summary>
public class ReconciliationExpectedDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Opening float
    /// </summary>
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// Cash sales total
    /// </summary>
    public decimal CashSales { get; set; }

    /// <summary>
    /// Cash refunds total
    /// </summary>
    public decimal CashRefunds { get; set; }

    /// <summary>
    /// Cash movements (net)
    /// </summary>
    public decimal CashMovements { get; set; }

    /// <summary>
    /// Expected cash on hand
    /// </summary>
    public decimal ExpectedCash { get; set; }

    /// <summary>
    /// Card payment totals
    /// </summary>
    public decimal CardPayments { get; set; }

    /// <summary>
    /// On-account payment totals
    /// </summary>
    public decimal OnAccountPayments { get; set; }

    /// <summary>
    /// Transaction count by payment method
    /// </summary>
    public Dictionary<string, int> TransactionCountByMethod { get; set; } = new();

    /// <summary>
    /// Amount totals by payment method
    /// </summary>
    public Dictionary<string, decimal> AmountTotalsByMethod { get; set; } = new();
}

/// <summary>
/// DTO for reconciliation result
/// </summary>
public class ReconciliationResultDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Reconciliation successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Reconciliation date/time
    /// </summary>
    public DateTime ReconciledAt { get; set; }

    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Opening float
    /// </summary>
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// Expected cash
    /// </summary>
    public decimal ExpectedCash { get; set; }

    /// <summary>
    /// Actual cash counted
    /// </summary>
    public decimal ActualCash { get; set; }

    /// <summary>
    /// Variance (actual - expected)
    /// </summary>
    public decimal Variance { get; set; }

    /// <summary>
    /// Variance percentage
    /// </summary>
    public decimal VariancePercentage { get; set; }

    /// <summary>
    /// Variance status (Acceptable, RequiresApproval, Approved, Rejected)
    /// </summary>
    public string VarianceStatus { get; set; } = string.Empty;

    /// <summary>
    /// Reason for variance
    /// </summary>
    public string? VarianceReason { get; set; }

    /// <summary>
    /// Manager who approved (if applicable)
    /// </summary>
    public string? ApprovedByManager { get; set; }

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Payment method breakdown
    /// </summary>
    public Dictionary<string, decimal> PaymentMethodTotals { get; set; } = new();

    /// <summary>
    /// Denomination breakdown
    /// </summary>
    public DenominationBreakdownDto? DenominationBreakdown { get; set; }

    /// <summary>
    /// Validation errors/warnings
    /// </summary>
    public List<string> ValidationMessages { get; set; } = new();
}

/// <summary>
/// DTO for reconciliation validation
/// </summary>
public class ReconciliationValidationDto
{
    /// <summary>
    /// Can proceed with reconciliation
    /// </summary>
    public bool CanReconcile { get; set; }

    /// <summary>
    /// Validation errors (blocking)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings (non-blocking)
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Session duration in hours
    /// </summary>
    public decimal SessionDurationHours { get; set; }

    /// <summary>
    /// Expected cash amount
    /// </summary>
    public decimal ExpectedCash { get; set; }

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Requires manager approval
    /// </summary>
    public bool RequiresManagerApproval { get; set; }
}

// ========================================
// DENOMINATION DTOs
// ========================================

/// <summary>
/// DTO for cash denomination breakdown
/// </summary>
public class DenominationBreakdownDto
{
    // Notes
    public int Notes200 { get; set; }
    public int Notes100 { get; set; }
    public int Notes50 { get; set; }
    public int Notes20 { get; set; }
    public int Notes10 { get; set; }

    // Coins
    public int Coins5 { get; set; }
    public int Coins2 { get; set; }
    public int Coins1 { get; set; }
    public int Coins050 { get; set; }
    public int Coins020 { get; set; }
    public int Coins010 { get; set; }
    public int Coins005 { get; set; }

    /// <summary>
    /// Calculate total from denomination counts
    /// </summary>
    public decimal CalculateTotal()
    {
        return (Notes200 * 200.00m) +
               (Notes100 * 100.00m) +
               (Notes50 * 50.00m) +
               (Notes20 * 20.00m) +
               (Notes10 * 10.00m) +
               (Coins5 * 5.00m) +
               (Coins2 * 2.00m) +
               (Coins1 * 1.00m) +
               (Coins050 * 0.50m) +
               (Coins020 * 0.20m) +
               (Coins010 * 0.10m) +
               (Coins005 * 0.05m);
    }
}

/// <summary>
/// DTO for denomination worksheet
/// </summary>
public class DenominationWorksheetDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Expected total cash
    /// </summary>
    public decimal ExpectedTotal { get; set; }

    /// <summary>
    /// Template for counting
    /// </summary>
    public List<DenominationLineDto> DenominationLines { get; set; } = new();
}

/// <summary>
/// Single line in denomination worksheet
/// </summary>
public class DenominationLineDto
{
    /// <summary>
    /// Denomination name (e.g., "R200 notes")
    /// </summary>
    public string DenominationName { get; set; } = string.Empty;

    /// <summary>
    /// Unit value (e.g., 200.00)
    /// </summary>
    public decimal UnitValue { get; set; }

    /// <summary>
    /// Count (to be filled in)
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Line total (UnitValue × Count)
    /// </summary>
    public decimal LineTotal => UnitValue * Count;
}

// ========================================
// PAYMENT METHOD BREAKDOWN DTOs
// ========================================

/// <summary>
/// DTO for payment method breakdown
/// </summary>
public class PaymentMethodBreakdownDto
{
    /// <summary>
    /// Date range start
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date range end
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Payment method totals
    /// </summary>
    public List<PaymentMethodTotal> PaymentMethodTotals { get; set; } = new();

    /// <summary>
    /// Grand total (all payment methods)
    /// </summary>
    public decimal GrandTotal { get; set; }

    /// <summary>
    /// Total transaction count
    /// </summary>
    public int TotalTransactionCount { get; set; }
}

/// <summary>
/// Payment method total details
/// </summary>
public class PaymentMethodTotal
{
    /// <summary>
    /// Payment method name
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal SalesAmount { get; set; }

    /// <summary>
    /// Total refund amount
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Net amount (sales - refunds)
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue { get; set; }

    /// <summary>
    /// Percentage of total
    /// </summary>
    public decimal PercentageOfTotal { get; set; }
}
