using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.DAL;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Retail.POS.DAL.Repositories;

/// <summary>
/// Repository implementation for payment reconciliation operations
/// </summary>
/// <remarks>
/// Enterprise Patterns Applied:
/// - Repository pattern (abstracts data access)
/// - Async/await for all I/O operations
/// - Business logic in methods (variance calculations, reconciliation)
/// - Comprehensive error handling
/// </remarks>
public class PaymentRepository : IPaymentRepository
{
    private readonly PosDbContext _context;

    public PaymentRepository(PosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ========================================
    // PAYMENT SUMMARY & REPORTING
    // ========================================

    /// <inheritdoc/>
    public async Task<PaymentSummary> GetPaymentSummaryAsync(
        DateTime periodStart,
        DateTime periodEnd,
        int? userId = null)
    {
        if (periodEnd < periodStart)
            throw new ArgumentException("End date must be >= start date", nameof(periodEnd));

        // Get all payments in period
        var query = _context.Payments
            .Include(p => p.TransactionHeader)
            .Where(p => p.PaymentDate >= periodStart && p.PaymentDate <= periodEnd);

        // Filter by user if specified
        if (userId.HasValue)
        {
            var userIdString = userId.Value.ToString();
            query = query.Where(p => p.TransactionHeader.CreatedBy == userIdString);
        }

        var payments = await query.ToListAsync();

        // Build summary
        var summary = new PaymentSummary
        {
            SummaryDate = DateTime.Now,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,

            // Totals by method
            TotalCash = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount),
            TotalCard = payments.Where(p => p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount),
            TotalEFT = payments.Where(p => p.PaymentMethod == PaymentMethod.EFT).Sum(p => p.Amount),
            TotalMobilePayment = payments.Where(p => p.PaymentMethod == PaymentMethod.MobilePayment).Sum(p => p.Amount),
            TotalOnAccount = payments.Where(p => p.PaymentMethod == PaymentMethod.OnAccount).Sum(p => p.Amount),
            TotalVoucher = payments.Where(p => p.PaymentMethod == PaymentMethod.Voucher).Sum(p => p.Amount),

            // Transaction count
            TransactionCount = payments.Select(p => p.TransactionHeaderId).Distinct().Count(),

            // Counts by method
            CashTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Select(p => p.TransactionHeaderId).Distinct().Count(),
            CardTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.Card).Select(p => p.TransactionHeaderId).Distinct().Count(),
            EFTTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.EFT).Select(p => p.TransactionHeaderId).Distinct().Count(),
            MobilePaymentTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.MobilePayment).Select(p => p.TransactionHeaderId).Distinct().Count(),
            OnAccountTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.OnAccount).Select(p => p.TransactionHeaderId).Distinct().Count(),
            VoucherTransactionCount = payments.Where(p => p.PaymentMethod == PaymentMethod.Voucher).Select(p => p.TransactionHeaderId).Distinct().Count(),

            // User info (if filtered)
            GeneratedByUserId = userId
        };

        return summary;
    }

    /// <inheritdoc/>
    public async Task<List<Payment>> GetPaymentsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        PaymentMethod? paymentMethod = null)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be >= start date", nameof(endDate));

        var query = _context.Payments
            .Include(p => p.TransactionHeader)
                .ThenInclude(t => t.TransactionDetails)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate);

        // Filter by payment method if specified
        if (paymentMethod.HasValue)
            query = query.Where(p => p.PaymentMethod == paymentMethod.Value);

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Payment>> GetPaymentsByUserAsync(
        int userId,
        DateTime shiftStart,
        DateTime shiftEnd)
    {
        if (shiftEnd < shiftStart)
            throw new ArgumentException("Shift end must be >= shift start", nameof(shiftEnd));

        var userIdString = userId.ToString();

        return await _context.Payments
            .Include(p => p.TransactionHeader)
                .ThenInclude(t => t.TransactionDetails)
            .Where(p => p.PaymentDate >= shiftStart
                     && p.PaymentDate <= shiftEnd
                     && p.TransactionHeader.CreatedBy == userIdString)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    // ========================================
    // CASH DRAWER RECONCILIATION
    // ========================================

    /// <inheritdoc/>
    public async Task<CashDrawerReconciliation> GetCashDrawerReconciliationAsync(
        int userId,
        DateTime shiftStart,
        DateTime shiftEnd,
        decimal openingFloat)
    {
        if (shiftEnd < shiftStart)
            throw new ArgumentException("Shift end must be >= shift start", nameof(shiftEnd));

        var userIdString = userId.ToString();

        // Get all cash payments for this user during shift
        var cashPayments = await _context.Payments
            .Include(p => p.TransactionHeader)
            .Where(p => p.PaymentMethod == PaymentMethod.Cash
                     && p.PaymentDate >= shiftStart
                     && p.PaymentDate <= shiftEnd
                     && p.TransactionHeader.CreatedBy == userIdString
                     && p.TransactionHeader.Status == TransactionStatus.Completed)
            .ToListAsync();

        // Calculate cash sales (positive amounts = sales)
        var cashSales = cashPayments
            .Where(p => p.Amount > 0 && p.TransactionHeader.TransactionType == TransactionType.Sale)
            .Sum(p => p.Amount);

        // Calculate cash paid out (refunds = negative/positive amounts depending on implementation)
        var cashPaidOut = cashPayments
            .Where(p => p.TransactionHeader.TransactionType == TransactionType.Refund)
            .Sum(p => Math.Abs(p.Amount));

        // Get transaction counts
        var transactionCount = cashPayments
            .Where(p => p.Amount > 0 && p.TransactionHeader.TransactionType == TransactionType.Sale)
            .Select(p => p.TransactionHeaderId)
            .Distinct()
            .Count();

        var refundCount = cashPayments
            .Where(p => p.TransactionHeader.TransactionType == TransactionType.Refund)
            .Select(p => p.TransactionHeaderId)
            .Distinct()
            .Count();

        // Build reconciliation
        var reconciliation = new CashDrawerReconciliation
        {
            ReconciliationDate = DateTime.Now,
            ShiftStart = shiftStart,
            ShiftEnd = shiftEnd,

            // Expected cash
            OpeningFloat = openingFloat,
            CashSales = cashSales,
            CashPaidOut = cashPaidOut,

            // User tracking
            CashierId = userId,
            CashierName = $"User {userId}", // TODO: Get actual user name from User table

            // Transaction counts
            TransactionCount = transactionCount,
            RefundCount = refundCount
        };

        return reconciliation;
    }

    /// <inheritdoc/>
    public async Task<int> RecordCashDrawerReconciliationAsync(CashDrawerReconciliation reconciliation)
    {
        if (reconciliation == null)
            throw new ArgumentNullException(nameof(reconciliation));

        // Validate required fields
        if (reconciliation.ActualCash == 0)
            throw new ArgumentException("Actual cash must be set", nameof(reconciliation));

        // TODO: Create CashDrawerReconciliation entity and save to database
        // For now, this is a placeholder that would save to a reconciliation table

        // In a real implementation, you would:
        // 1. Create a new entity
        // 2. Save to _context.CashDrawerReconciliations
        // 3. Return the generated ID

        // Placeholder implementation
        await Task.CompletedTask;
        return 1; // Return placeholder ID
    }

    /// <inheritdoc/>
    public async Task<List<CashDrawerReconciliation>> GetReconciliationHistoryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be >= start date", nameof(endDate));

        // TODO: Query from CashDrawerReconciliation table when entity exists
        // For now, return empty list as placeholder

        await Task.CompletedTask;
        return new List<CashDrawerReconciliation>();
    }

    // ========================================
    // VARIANCE ANALYSIS & ALERTS
    // ========================================

    /// <inheritdoc/>
    public async Task<List<CashDrawerReconciliation>> GetVarianceAlertsAsync(
        DateTime startDate,
        DateTime endDate,
        decimal varianceThreshold = 10.00m)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be >= start date", nameof(endDate));

        // TODO: Query from CashDrawerReconciliation table when entity exists
        // Filter for reconciliations where AbsoluteVariance > varianceThreshold

        await Task.CompletedTask;
        return new List<CashDrawerReconciliation>();
    }

    /// <inheritdoc/>
    public async Task<CashierVarianceSummary> GetCashierVarianceSummaryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be >= start date", nameof(endDate));

        // TODO: Query from CashDrawerReconciliation table when entity exists
        // Calculate summary statistics for this cashier

        // Placeholder implementation
        await Task.CompletedTask;

        return new CashierVarianceSummary
        {
            CashierId = userId,
            CashierName = $"User {userId}",
            TotalReconciliations = 0,
            BalancedReconciliations = 0,
            ReconciliationsWithinTolerance = 0,
            ReconciliationsRequiringReview = 0,
            TotalVariance = 0,
            AverageVariance = 0,
            LargestVariance = 0
        };
    }
}
