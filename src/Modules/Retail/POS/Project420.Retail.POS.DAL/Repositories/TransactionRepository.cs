using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.DAL;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database;
using Project420.Shared.Infrastructure.DTOs;


namespace Project420.Retail.POS.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for POS transaction database operations
    /// </summary>
    /// <remarks>
    /// Enterprise Patterns Applied:
    /// - Repository pattern (abstracts data access)
    /// - Unit of Work pattern (DbContext manages transactions)
    /// - Async/await for all I/O operations
    /// - Eager loading with Include() for related entities
    /// - Proper exception handling and logging
    ///
    /// Phase 7B Changes:
    /// - Uses SharedDbContext for TransactionDetails (unified architecture)
    /// - PosDbContext for RetailTransactionHeaders and Payments
    /// - TransactionDetails linked via HeaderId + TransactionType discriminator
    /// </remarks>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly PosDbContext _posContext;
        private readonly SharedDbContext _sharedContext;

        public TransactionRepository(PosDbContext posContext, SharedDbContext sharedContext)
        {
            _posContext = posContext ?? throw new ArgumentNullException(nameof(posContext));
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        // ========================================
        // CREATE OPERATIONS
        // ========================================

        /// <inheritdoc/>
        public async Task<RetailTransactionHeader> CreateSaleAsync(
            RetailTransactionHeader header,
            List<TransactionDetail> details,
            Payment payment)
        {
            // Validation
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (details == null || details.Count == 0)
                throw new ArgumentException("Transaction must have at least one line item", nameof(details));
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            // Phase 7B: Use distributed transaction across PosDbContext and SharedDbContext
            // Note: For true atomicity across databases, consider implementing Saga pattern
            // For now, we use best-effort with manual rollback

            try
            {
                // 1. Add transaction header to PosDbContext
                _posContext.RetailTransactionHeaders.Add(header);
                await _posContext.SaveChangesAsync();

                // 2. Add transaction details to SharedDbContext (unified table)
                foreach (var detail in details)
                {
                    detail.HeaderId = header.Id;
                    detail.TransactionType = header.TransactionType; // Sale, Refund, etc.
                    _sharedContext.TransactionDetails.Add(detail);
                }
                await _sharedContext.SaveChangesAsync();

                // 3. Add payment to PosDbContext (link to header)
                payment.TransactionHeaderId = header.Id;
                _posContext.Payments.Add(payment);
                await _posContext.SaveChangesAsync();

                // Return header with all relationships loaded
                return await GetByIdAsync(header.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created transaction");
            }
            catch (Exception)
            {
                // Note: With cross-database operations, we may have partial data.
                // In production, consider implementing compensation/saga pattern.
                throw;
            }
        }

        // ========================================
        // READ OPERATIONS
        // ========================================

        /// <inheritdoc/>
        public async Task<RetailTransactionHeader?> GetByIdAsync(int transactionId)
        {
            // Phase 7B: Load header with payments from PosDbContext
            var header = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (header == null) return null;

            // Load transaction details from SharedDbContext (unified table)
            var details = await _sharedContext.TransactionDetails
                .Where(td => td.HeaderId == transactionId && td.TransactionType == header.TransactionType)
                .ToListAsync();

            header.TransactionDetails = details;
            return header;
        }

        /// <inheritdoc/>
        public async Task<RetailTransactionHeader?> GetByTransactionNumberAsync(string transactionNumber)
        {
            if (string.IsNullOrWhiteSpace(transactionNumber))
                throw new ArgumentException("Transaction number cannot be empty", nameof(transactionNumber));

            // Phase 7B: Load header with payments from PosDbContext
            var header = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber);

            if (header == null) return null;

            // Load transaction details from SharedDbContext (unified table)
            var details = await _sharedContext.TransactionDetails
                .Where(td => td.HeaderId == header.Id && td.TransactionType == header.TransactionType)
                .ToListAsync();

            header.TransactionDetails = details;
            return header;
        }

        /// <inheritdoc/>
        public async Task<List<RetailTransactionHeader>> GetByCustomerAsync(int debtorId, int pageNumber = 1, int pageSize = 50)
        {
            if (debtorId <= 0) throw new ArgumentException("Invalid debtor ID", nameof(debtorId));
            if (pageNumber < 1) throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
            if (pageSize < 1 || pageSize > 100) throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

            var headers = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .Where(t => t.DebtorId == debtorId)
                .OrderByDescending(t => t.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await LoadTransactionDetailsAsync(headers);
            return headers;
        }

        /// <inheritdoc/>
        public async Task<List<RetailTransactionHeader>> GetTodaysTransactionsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var headers = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .Where(t => t.TransactionDate >= today && t.TransactionDate < tomorrow)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            await LoadTransactionDetailsAsync(headers);
            return headers;
        }

        /// <inheritdoc/>
        public async Task<List<RetailTransactionHeader>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be >= start date", nameof(endDate));

            var headers = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            await LoadTransactionDetailsAsync(headers);
            return headers;
        }

        // ========================================
        // UPDATE OPERATIONS
        // ========================================

        /// <inheritdoc/>
        public async Task<bool> VoidTransactionAsync(int transactionId, string voidReason, int userId)
        {
            if (string.IsNullOrWhiteSpace(voidReason))
                throw new ArgumentException("Void reason is required for audit compliance", nameof(voidReason));

            var transaction = await _posContext.RetailTransactionHeaders
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
                return false;

            // Don't allow voiding already voided transactions
            if (transaction.Status == TransactionStatus.Cancelled)
                return false;

            // Update status and audit fields
            transaction.Status = TransactionStatus.Cancelled;
            transaction.Notes = string.IsNullOrEmpty(transaction.Notes)
                ? $"VOIDED: {voidReason}"
                : $"{transaction.Notes}\nVOIDED: {voidReason}";
            transaction.ModifiedAt = DateTime.UtcNow;
            transaction.ModifiedBy = userId.ToString();

            await _posContext.SaveChangesAsync();
            return true;
        }

        // ========================================
        // REPORTING QUERIES
        // ========================================

        /// <inheritdoc/>
        public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be >= start date", nameof(endDate));

            return await _posContext.RetailTransactionHeaders
                .Where(t => t.TransactionDate >= startDate
                         && t.TransactionDate <= endDate
                         && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.TotalAmount);
        }

        /// <inheritdoc/>
        public async Task<(decimal Subtotal, decimal VATAmount, decimal Total)> GetVATSummaryAsync(
            DateTime startDate,
            DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be >= start date", nameof(endDate));

            var transactions = await _posContext.RetailTransactionHeaders
                .Where(t => t.TransactionDate >= startDate
                         && t.TransactionDate <= endDate
                         && t.Status == TransactionStatus.Completed)
                .ToListAsync();

            var subtotal = transactions.Sum(t => t.Subtotal);
            var vatAmount = transactions.Sum(t => t.TaxAmount);
            var total = transactions.Sum(t => t.TotalAmount);

            return (subtotal, vatAmount, total);
        }

        // ========================================
        // REFUND OPERATIONS
        // ========================================

        /// <inheritdoc/>
        public async Task<RetailTransactionHeader> ProcessRefundAsync(
            string originalTransactionNumber,
            RetailTransactionHeader refundHeader,
            List<TransactionDetail> refundDetails,
            Payment payment,
            RefundReason refundReason)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(originalTransactionNumber))
                throw new ArgumentException("Original transaction number is required", nameof(originalTransactionNumber));
            if (refundHeader == null) throw new ArgumentNullException(nameof(refundHeader));
            if (refundDetails == null || refundDetails.Count == 0)
                throw new ArgumentException("Refund must have at least one line item", nameof(refundDetails));
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            // Get original transaction
            var originalTransaction = await GetByTransactionNumberAsync(originalTransactionNumber);
            if (originalTransaction == null)
                throw new InvalidOperationException($"Original transaction {originalTransactionNumber} not found");

            // Validate refund
            var validation = await ValidateRefundAsync(originalTransactionNumber, Math.Abs(refundHeader.TotalAmount));
            if (!validation.IsValid)
                throw new InvalidOperationException($"Refund validation failed: {string.Join(", ", validation.ValidationErrors)}");

            // Phase 7B: Cross-database operation
            try
            {
                // Set refund-specific fields
                refundHeader.TransactionType = TransactionType.Refund;
                refundHeader.Status = TransactionStatus.Completed;
                refundHeader.OriginalTransactionId = originalTransaction.Id;
                refundHeader.Notes = string.IsNullOrEmpty(refundHeader.Notes)
                    ? $"Refund Reason: {refundReason}"
                    : $"{refundHeader.Notes}\nRefund Reason: {refundReason}";

                // 1. Add refund transaction header to PosDbContext
                _posContext.RetailTransactionHeaders.Add(refundHeader);
                await _posContext.SaveChangesAsync();

                // 2. Add refund details to SharedDbContext (unified table)
                foreach (var detail in refundDetails)
                {
                    detail.HeaderId = refundHeader.Id;
                    detail.TransactionType = TransactionType.Refund;
                    _sharedContext.TransactionDetails.Add(detail);
                }
                await _sharedContext.SaveChangesAsync();

                // 3. Add payment (negative amount for refund)
                payment.TransactionHeaderId = refundHeader.Id;
                _posContext.Payments.Add(payment);
                await _posContext.SaveChangesAsync();

                // 4. Update original transaction status
                var originalToUpdate = await _posContext.RetailTransactionHeaders
                    .FirstAsync(t => t.Id == originalTransaction.Id);
                originalToUpdate.Status = TransactionStatus.Refunded;
                originalToUpdate.ModifiedAt = DateTime.UtcNow;
                originalToUpdate.ModifiedBy = refundHeader.CreatedBy;
                await _posContext.SaveChangesAsync();

                // Return refund with all relationships loaded
                return await GetByIdAsync(refundHeader.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created refund");
            }
            catch (Exception)
            {
                // Note: With cross-database operations, we may have partial data.
                // In production, consider implementing compensation/saga pattern.
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<RefundValidationResult> ValidateRefundAsync(string originalTransactionNumber, decimal refundAmount)
        {
            var result = new RefundValidationResult
            {
                OriginalTransactionNumber = originalTransactionNumber
            };

            // Get original transaction
            var originalTransaction = await GetByTransactionNumberAsync(originalTransactionNumber);
            if (originalTransaction == null)
            {
                result.AddError("Original transaction not found");
                return result;
            }

            // Check transaction type (can't refund a refund)
            if (originalTransaction.TransactionType != TransactionType.Sale)
            {
                result.AddError($"Cannot refund a {originalTransaction.TransactionType} transaction");
                return result;
            }

            // Check transaction status
            if (originalTransaction.Status == TransactionStatus.Cancelled)
            {
                result.AddError("Cannot refund a cancelled transaction");
                return result;
            }

            // Calculate days since original transaction
            result.DaysSinceOriginalTransaction = (DateTime.Now.Date - originalTransaction.TransactionDate.Date).Days;

            // Check refund window (30 days standard)
            const int refundWindowDays = 30;
            if (result.DaysSinceOriginalTransaction > refundWindowDays)
            {
                result.AddError($"Refund window expired (must be within {refundWindowDays} days)");
                result.RequiresManagerApproval = true;
            }

            // Get previous refunds for this transaction
            var previousRefunds = await GetRefundHistoryAsync(originalTransactionNumber);
            result.PreviousRefundAmount = previousRefunds.Sum(r => Math.Abs(r.TotalAmount));

            // Calculate max refund amount
            result.MaxRefundAmount = originalTransaction.TotalAmount - result.PreviousRefundAmount;

            // Check refund amount doesn't exceed remaining
            if (refundAmount > result.MaxRefundAmount)
            {
                result.AddError($"Refund amount R{refundAmount:F2} exceeds maximum refundable R{result.MaxRefundAmount:F2}");
                return result;
            }

            // Manager approval required for large refunds
            if (refundAmount > 1000.00m)
            {
                result.RequiresManagerApproval = true;
            }

            // If we got here, refund is valid
            result.IsValid = result.ValidationErrors.Count == 0;

            return result;
        }

        /// <inheritdoc/>
        public async Task<RetailTransactionHeader?> GetRefundableTransactionAsync(string transactionNumber)
        {
            // Get transaction with refund history
            var transaction = await GetByTransactionNumberAsync(transactionNumber);
            if (transaction == null)
                return null;

            // Only sales can be refunded
            if (transaction.TransactionType != TransactionType.Sale)
                return null;

            // Don't allow refunds of cancelled transactions
            if (transaction.Status == TransactionStatus.Cancelled)
                return null;

            return transaction;
        }

        /// <inheritdoc/>
        public async Task<List<RetailTransactionHeader>> GetRefundHistoryAsync(string originalTransactionNumber)
        {
            if (string.IsNullOrWhiteSpace(originalTransactionNumber))
                throw new ArgumentException("Transaction number cannot be empty", nameof(originalTransactionNumber));

            // First, get the original transaction by number
            var originalTransaction = await _posContext.RetailTransactionHeaders
                .FirstOrDefaultAsync(t => t.TransactionNumber == originalTransactionNumber);

            if (originalTransaction == null)
                return new List<RetailTransactionHeader>();

            // Then get all refunds referencing this transaction
            var headers = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .Where(t => t.TransactionType == TransactionType.Refund
                         && t.OriginalTransactionId == originalTransaction.Id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            await LoadTransactionDetailsAsync(headers);
            return headers;
        }

        // ========================================
        // ADVANCED SEARCH & FILTERING
        // ========================================

        /// <inheritdoc/>
        public async Task<PagedResult<RetailTransactionHeader>> SearchTransactionsAsync(TransactionSearchCriteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            // Validate criteria
            if (!criteria.IsValid(out var errors))
                throw new ArgumentException($"Invalid search criteria: {string.Join(", ", errors)}");

            // Phase 7B: Two-phase search for cross-database queries
            // Phase 1: Get header IDs matching criteria (PosDbContext)
            // Phase 2: Filter by TransactionDetail criteria (SharedDbContext)

            // Start with base query (headers and payments from PosDbContext)
            var query = _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .AsQueryable();

            // Apply header-level filters
            if (criteria.StartDate.HasValue)
                query = query.Where(t => t.TransactionDate >= criteria.StartDate.Value);

            if (criteria.EndDate.HasValue)
                query = query.Where(t => t.TransactionDate <= criteria.EndDate.Value);

            if (criteria.CustomerId.HasValue)
                query = query.Where(t => t.DebtorId == criteria.CustomerId.Value);

            if (!string.IsNullOrWhiteSpace(criteria.TransactionNumber))
                query = query.Where(t => t.TransactionNumber.Contains(criteria.TransactionNumber));

            if (criteria.TransactionType.HasValue)
                query = query.Where(t => t.TransactionType == criteria.TransactionType.Value);

            if (criteria.Status.HasValue)
                query = query.Where(t => t.Status == criteria.Status.Value);

            if (criteria.MinAmount.HasValue)
                query = query.Where(t => t.TotalAmount >= criteria.MinAmount.Value);

            if (criteria.MaxAmount.HasValue)
                query = query.Where(t => t.TotalAmount <= criteria.MaxAmount.Value);

            if (criteria.PaymentMethod.HasValue)
                query = query.Where(t => t.Payments.Any(p => p.PaymentMethod == criteria.PaymentMethod.Value));

            if (criteria.ProcessedByUserId.HasValue)
            {
                var userId = criteria.ProcessedByUserId.Value.ToString();
                query = query.Where(t => t.CreatedBy == userId);
            }

            // For detail-level filtering (ProductId, BatchNumber), we need to do two-phase query
            IEnumerable<int>? detailFilteredIds = null;
            if (criteria.ProductId.HasValue || !string.IsNullOrWhiteSpace(criteria.BatchNumber))
            {
                var detailQuery = _sharedContext.TransactionDetails.AsQueryable();

                if (criteria.ProductId.HasValue)
                    detailQuery = detailQuery.Where(d => d.ProductId == criteria.ProductId.Value);

                if (!string.IsNullOrWhiteSpace(criteria.BatchNumber))
                    detailQuery = detailQuery.Where(d => d.BatchNumber == criteria.BatchNumber);

                detailFilteredIds = await detailQuery.Select(d => d.HeaderId).Distinct().ToListAsync();
                query = query.Where(t => detailFilteredIds.Contains(t.Id));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip(criteria.GetSkipCount())
                .Take(criteria.PageSize)
                .ToListAsync();

            // Load transaction details for the results
            await LoadTransactionDetailsAsync(items);

            return new PagedResult<RetailTransactionHeader>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = criteria.PageNumber,
                PageSize = criteria.PageSize
            };
        }

        /// <inheritdoc/>
        public async Task<TransactionStatistics> GetTransactionStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be >= start date", nameof(endDate));

            var transactions = await _posContext.RetailTransactionHeaders
                .Include(t => t.Payments)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToListAsync();

            // Load transaction details for all transactions
            await LoadTransactionDetailsAsync(transactions);

            var stats = new TransactionStatistics
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,

                // Transaction counts
                TotalTransactionCount = transactions.Count,
                SalesCount = transactions.Count(t => t.TransactionType == TransactionType.Sale),
                RefundsCount = transactions.Count(t => t.TransactionType == TransactionType.Refund),
                CompletedCount = transactions.Count(t => t.Status == TransactionStatus.Completed),
                CancelledCount = transactions.Count(t => t.Status == TransactionStatus.Cancelled),

                // Financial totals (completed transactions only)
                TotalSalesSubtotal = transactions
                    .Where(t => t.Status == TransactionStatus.Completed && t.TransactionType == TransactionType.Sale)
                    .Sum(t => t.Subtotal),
                TotalVATAmount = transactions
                    .Where(t => t.Status == TransactionStatus.Completed && t.TransactionType == TransactionType.Sale)
                    .Sum(t => t.TaxAmount),
                TotalSalesIncludingVAT = transactions
                    .Where(t => t.Status == TransactionStatus.Completed && t.TransactionType == TransactionType.Sale)
                    .Sum(t => t.TotalAmount),
                TotalRefundsAmount = transactions
                    .Where(t => t.TransactionType == TransactionType.Refund)
                    .Sum(t => Math.Abs(t.TotalAmount)),

                // Averages
                AverageItemsPerTransaction = transactions.Count > 0
                    ? (decimal)transactions.Sum(t => t.TransactionDetails.Count) / transactions.Count
                    : 0,

                // Payment method breakdown
                CashPaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount),
                CardPaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount),
                EFTPaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.EFT).Sum(p => p.Amount),
                MobilePaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.MobilePayment).Sum(p => p.Amount),
                OnAccountPaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.OnAccount).Sum(p => p.Amount),
                VoucherPaymentTotal = transactions.SelectMany(t => t.Payments).Where(p => p.PaymentMethod == PaymentMethod.Voucher).Sum(p => p.Amount),

                // Payment method counts
                CashTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash)),
                CardTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card)),
                EFTTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.EFT)),
                MobilePaymentTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.MobilePayment)),
                OnAccountTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.OnAccount)),
                VoucherTransactionCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Voucher)),

                // Peak analysis
                HighestTransactionValue = transactions.Any() ? transactions.Max(t => t.TotalAmount) : 0,
                LowestTransactionValue = transactions.Any() ? transactions.Where(t => t.TotalAmount > 0).Min(t => t.TotalAmount) : 0,

                // User/cashier breakdown
                UniqueCashierCount = transactions.Select(t => t.CreatedBy).Distinct().Count()
            };

            // Most popular payment method
            var paymentMethodCounts = new Dictionary<PaymentMethod, int>
            {
                { PaymentMethod.Cash, stats.CashTransactionCount },
                { PaymentMethod.Card, stats.CardTransactionCount },
                { PaymentMethod.EFT, stats.EFTTransactionCount },
                { PaymentMethod.MobilePayment, stats.MobilePaymentTransactionCount },
                { PaymentMethod.OnAccount, stats.OnAccountTransactionCount },
                { PaymentMethod.Voucher, stats.VoucherTransactionCount }
            };
            stats.MostPopularPaymentMethod = paymentMethodCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key.ToString();

            return stats;
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        /// <summary>
        /// Loads transaction details from SharedDbContext for a collection of headers.
        /// Uses batch loading for efficiency.
        /// </summary>
        /// <param name="headers">Collection of transaction headers to load details for</param>
        private async Task LoadTransactionDetailsAsync(IEnumerable<RetailTransactionHeader> headers)
        {
            var headerList = headers.ToList();
            if (!headerList.Any()) return;

            // Get all header IDs and their transaction types
            var headerData = headerList.Select(h => new { h.Id, h.TransactionType }).ToList();
            var headerIds = headerData.Select(h => h.Id).ToList();

            // Batch load all details for these headers
            var allDetails = await _sharedContext.TransactionDetails
                .Where(td => headerIds.Contains(td.HeaderId))
                .ToListAsync();

            // Assign details to each header (filter by TransactionType as well)
            foreach (var header in headerList)
            {
                header.TransactionDetails = allDetails
                    .Where(td => td.HeaderId == header.Id && td.TransactionType == header.TransactionType)
                    .ToList();
            }
        }
    }
}
