using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service implementation for POS payment reconciliation and cash drawer management
    /// </summary>
    public class PaymentReconciliationService : IPaymentReconciliationService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITransactionRepository _transactionRepository;

        // Business rule constants
        private const decimal ACCEPTABLE_VARIANCE = 10.00m;
        private const decimal MANAGER_APPROVAL_VARIANCE = 50.00m;
        private const decimal LARGE_CASH_MOVEMENT_THRESHOLD = 500.00m;

        // In-memory cache for active sessions (in production, use database or distributed cache)
        private static readonly Dictionary<int, CashDrawerSessionDto> _activeSessions = new();

        public PaymentReconciliationService(
            IPaymentRepository paymentRepository,
            ITransactionRepository transactionRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        /// <inheritdoc/>
        public async Task<CashDrawerSessionDto> OpenCashDrawerAsync(CashDrawerOpenRequestDto request)
        {
            // Validate request
            if (request.CashierId <= 0)
                throw new ArgumentException("Invalid cashier ID", nameof(request));

            if (request.OpeningFloat < 0)
                throw new ArgumentException("Opening float cannot be negative", nameof(request));

            // Check if cashier already has an open session
            var existingSession = await GetActiveCashDrawerSessionAsync(request.CashierId);
            if (existingSession != null)
            {
                throw new InvalidOperationException($"Cashier already has an open cash drawer session (Session ID: {existingSession.SessionId})");
            }

            // Validate denomination breakdown matches opening float
            var calculatedTotal = request.DenominationBreakdown.CalculateTotal();
            if (Math.Abs(calculatedTotal - request.OpeningFloat) > 0.01m)
            {
                throw new ArgumentException($"Denomination breakdown total ({calculatedTotal:C2}) does not match opening float ({request.OpeningFloat:C2})");
            }

            // Create new session
            var sessionId = GenerateSessionId();
            var session = new CashDrawerSessionDto
            {
                SessionId = sessionId,
                CashierId = request.CashierId,
                CashierName = $"Cashier {request.CashierId}", // TODO: Get from user repository
                OpenedAt = DateTime.UtcNow,
                Status = "Open",
                OpeningFloat = request.OpeningFloat,
                ExpectedCash = request.OpeningFloat,
                TransactionCount = 0,
                DenominationBreakdown = request.DenominationBreakdown
            };

            // Store in cache (in production, save to database)
            _activeSessions[request.CashierId] = session;

            return session;
        }

        /// <inheritdoc/>
        public async Task<CashDrawerSessionDto> RecordCashMovementAsync(int sessionId, CashMovementRequestDto request)
        {
            // Find session
            var session = _activeSessions.Values.FirstOrDefault(s => s.SessionId == sessionId);
            if (session == null)
                throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));

            if (session.Status != "Open")
                throw new InvalidOperationException($"Cannot record movement - session is {session.Status}");

            // Validate movement
            if (request.Amount == 0)
                throw new ArgumentException("Movement amount cannot be zero", nameof(request));

            // Check manager approval requirement
            if (Math.Abs(request.Amount) > LARGE_CASH_MOVEMENT_THRESHOLD && !request.ApprovedByManagerId.HasValue)
            {
                throw new InvalidOperationException($"Manager approval required for cash movements exceeding R{LARGE_CASH_MOVEMENT_THRESHOLD:N2}");
            }

            // Create movement record
            var movement = new CashMovementDto
            {
                MovementId = session.CashMovements.Count + 1,
                MovementDate = DateTime.UtcNow,
                MovementType = request.MovementType.ToString(),
                Amount = request.Amount,
                Reason = request.Reason,
                PerformedBy = $"User {request.PerformedByUserId}",
                ApprovedBy = request.ApprovedByManagerId.HasValue ? $"Manager {request.ApprovedByManagerId}" : null
            };

            session.CashMovements.Add(movement);

            // Update expected cash
            session.ExpectedCash += request.Amount;

            return session;
        }

        /// <inheritdoc/>
        public async Task<CashDrawerSessionDto?> GetActiveCashDrawerSessionAsync(int cashierId)
        {
            if (_activeSessions.TryGetValue(cashierId, out var session) && session.Status == "Open")
            {
                // Recalculate expected cash based on transactions
                var expected = await CalculateExpectedAmountsAsync(session.SessionId);
                session.ExpectedCash = expected.ExpectedCash;
                session.TransactionCount = expected.TransactionCountByMethod.Values.Sum();
                return session;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<ReconciliationExpectedDto> CalculateExpectedAmountsAsync(int sessionId)
        {
            // Find session
            var session = _activeSessions.Values.FirstOrDefault(s => s.SessionId == sessionId);
            if (session == null)
                throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));

            // Get payment summary for session period
            var paymentSummary = await _paymentRepository.GetPaymentSummaryAsync(
                session.OpenedAt,
                DateTime.UtcNow,
                session.CashierId);

            // Get transactions for session
            var transactions = await _transactionRepository.GetByDateRangeAsync(
                session.OpenedAt,
                DateTime.UtcNow);

            // Filter transactions by cashier (assuming CreatedBy contains cashier ID)
            var cashierTransactions = transactions
                .Where(t => t.CreatedBy == session.CashierId.ToString())
                .ToList();

            // Calculate cash sales and refunds
            var cashSales = cashierTransactions
                .Where(t => t.TotalAmount > 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash))
                .Sum(t => t.TotalAmount);

            var cashRefunds = Math.Abs(cashierTransactions
                .Where(t => t.TotalAmount < 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash))
                .Sum(t => t.TotalAmount));

            // Calculate cash movements
            var cashMovements = session.CashMovements.Sum(m => m.Amount);

            // Calculate expected cash
            var expectedCash = session.OpeningFloat + cashSales - cashRefunds + cashMovements;

            // Build result
            var result = new ReconciliationExpectedDto
            {
                SessionId = sessionId,
                OpeningFloat = session.OpeningFloat,
                CashSales = cashSales,
                CashRefunds = cashRefunds,
                CashMovements = cashMovements,
                ExpectedCash = expectedCash,
                CardPayments = cashierTransactions
                    .Where(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card))
                    .Sum(t => Math.Abs(t.TotalAmount)),
                OnAccountPayments = cashierTransactions
                    .Where(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.OnAccount))
                    .Sum(t => Math.Abs(t.TotalAmount))
            };

            // Transaction counts by method
            result.TransactionCountByMethod = new Dictionary<string, int>
            {
                ["Cash"] = cashierTransactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash)),
                ["Card"] = cashierTransactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card)),
                ["OnAccount"] = cashierTransactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.OnAccount))
            };

            // Amount totals by method
            result.AmountTotalsByMethod = new Dictionary<string, decimal>
            {
                ["Cash"] = expectedCash,
                ["Card"] = result.CardPayments,
                ["OnAccount"] = result.OnAccountPayments
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<ReconciliationResultDto> ReconcileCashDrawerAsync(CashDrawerCloseRequestDto request)
        {
            // Find session
            var session = _activeSessions.Values.FirstOrDefault(s => s.SessionId == request.SessionId);
            if (session == null)
            {
                return new ReconciliationResultDto
                {
                    Success = false,
                    ValidationMessages = new List<string> { $"Session {request.SessionId} not found" }
                };
            }

            if (session.Status != "Open")
            {
                return new ReconciliationResultDto
                {
                    Success = false,
                    ValidationMessages = new List<string> { $"Session is already {session.Status}" }
                };
            }

            // Validate denomination breakdown
            var calculatedTotal = request.DenominationBreakdown.CalculateTotal();
            if (Math.Abs(calculatedTotal - request.ActualCash) > 0.01m)
            {
                return new ReconciliationResultDto
                {
                    Success = false,
                    ValidationMessages = new List<string>
                    {
                        $"Denomination breakdown total ({calculatedTotal:C2}) does not match actual cash ({request.ActualCash:C2})"
                    }
                };
            }

            // Get expected amounts
            var expected = await CalculateExpectedAmountsAsync(request.SessionId);

            // Calculate variance
            var variance = request.ActualCash - expected.ExpectedCash;
            var variancePercentage = expected.ExpectedCash > 0
                ? (variance / expected.ExpectedCash) * 100
                : 0;

            // Determine variance status
            string varianceStatus;
            bool requiresApproval = false;

            if (Math.Abs(variance) <= ACCEPTABLE_VARIANCE)
            {
                varianceStatus = "Acceptable";
            }
            else if (Math.Abs(variance) <= MANAGER_APPROVAL_VARIANCE)
            {
                varianceStatus = "RequiresApproval";
                requiresApproval = true;

                if (string.IsNullOrWhiteSpace(request.VarianceReason))
                {
                    return new ReconciliationResultDto
                    {
                        Success = false,
                        ValidationMessages = new List<string>
                        {
                            $"Variance of {variance:C2} requires explanation"
                        }
                    };
                }
            }
            else
            {
                varianceStatus = "RequiresApproval";
                requiresApproval = true;

                if (!request.ApprovedByManagerId.HasValue)
                {
                    return new ReconciliationResultDto
                    {
                        Success = false,
                        ValidationMessages = new List<string>
                        {
                            $"Variance of {variance:C2} exceeds threshold - manager approval required"
                        }
                    };
                }

                if (string.IsNullOrWhiteSpace(request.VarianceReason))
                {
                    return new ReconciliationResultDto
                    {
                        Success = false,
                        ValidationMessages = new List<string>
                        {
                            $"Variance of {variance:C2} requires explanation"
                        }
                    };
                }

                varianceStatus = "Approved";
            }

            // Update session
            session.ClosedAt = DateTime.UtcNow;
            session.Status = "Reconciled";
            session.ActualCash = request.ActualCash;
            session.Variance = variance;
            session.DenominationBreakdown = request.DenominationBreakdown;

            // Build result
            var result = new ReconciliationResultDto
            {
                SessionId = request.SessionId,
                Success = true,
                ReconciledAt = DateTime.UtcNow,
                CashierName = session.CashierName,
                OpeningFloat = session.OpeningFloat,
                ExpectedCash = expected.ExpectedCash,
                ActualCash = request.ActualCash,
                Variance = variance,
                VariancePercentage = variancePercentage,
                VarianceStatus = varianceStatus,
                VarianceReason = request.VarianceReason,
                ApprovedByManager = request.ApprovedByManagerId.HasValue ? $"Manager {request.ApprovedByManagerId}" : null,
                TransactionCount = expected.TransactionCountByMethod.Values.Sum(),
                PaymentMethodTotals = expected.AmountTotalsByMethod,
                DenominationBreakdown = request.DenominationBreakdown
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<ReconciliationResultDto>> GetReconciliationHistoryAsync(
            DateTime startDate,
            DateTime endDate,
            int? cashierId = null)
        {
            // Get closed sessions from cache (in production, query database)
            var closedSessions = _activeSessions.Values
                .Where(s => s.Status == "Reconciled" &&
                           s.ClosedAt.HasValue &&
                           s.ClosedAt.Value >= startDate &&
                           s.ClosedAt.Value <= endDate &&
                           (!cashierId.HasValue || s.CashierId == cashierId.Value))
                .OrderByDescending(s => s.ClosedAt)
                .ToList();

            // Map to ReconciliationResultDto
            var results = closedSessions.Select(s => new ReconciliationResultDto
            {
                SessionId = s.SessionId,
                Success = true,
                ReconciledAt = s.ClosedAt!.Value,
                CashierName = s.CashierName,
                OpeningFloat = s.OpeningFloat,
                ExpectedCash = s.ExpectedCash,
                ActualCash = s.ActualCash ?? 0,
                Variance = s.Variance ?? 0,
                VariancePercentage = s.ExpectedCash > 0 ? ((s.Variance ?? 0) / s.ExpectedCash) * 100 : 0,
                VarianceStatus = Math.Abs(s.Variance ?? 0) <= ACCEPTABLE_VARIANCE ? "Acceptable" : "Approved",
                TransactionCount = s.TransactionCount,
                DenominationBreakdown = s.DenominationBreakdown
            }).ToList();

            return results;
        }

        /// <inheritdoc/>
        public async Task<PaymentMethodBreakdownDto> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate)
        {
            // Get payment summary
            var paymentSummary = await _paymentRepository.GetPaymentSummaryAsync(startDate, endDate);

            // Get transactions for refund analysis
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // Build payment method totals
            var paymentMethodTotals = new List<PaymentMethodTotal>();

            // Cash
            var cashSales = transactions.Where(t => t.TotalAmount > 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash)).Sum(t => t.TotalAmount);
            var cashRefunds = Math.Abs(transactions.Where(t => t.TotalAmount < 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash)).Sum(t => t.TotalAmount));
            var cashCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash));

            paymentMethodTotals.Add(new PaymentMethodTotal
            {
                PaymentMethod = "Cash",
                SalesAmount = cashSales,
                RefundAmount = cashRefunds,
                NetAmount = cashSales - cashRefunds,
                TransactionCount = cashCount,
                AverageTransactionValue = cashCount > 0 ? cashSales / cashCount : 0
            });

            // Card
            var cardSales = transactions.Where(t => t.TotalAmount > 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card)).Sum(t => t.TotalAmount);
            var cardRefunds = Math.Abs(transactions.Where(t => t.TotalAmount < 0 && t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card)).Sum(t => t.TotalAmount));
            var cardCount = transactions.Count(t => t.Payments.Any(p => p.PaymentMethod == PaymentMethod.Card));

            paymentMethodTotals.Add(new PaymentMethodTotal
            {
                PaymentMethod = "Card",
                SalesAmount = cardSales,
                RefundAmount = cardRefunds,
                NetAmount = cardSales - cardRefunds,
                TransactionCount = cardCount,
                AverageTransactionValue = cardCount > 0 ? cardSales / cardCount : 0
            });

            // Calculate grand total and percentages
            var grandTotal = paymentMethodTotals.Sum(p => p.NetAmount);
            foreach (var total in paymentMethodTotals)
            {
                total.PercentageOfTotal = grandTotal > 0 ? (total.NetAmount / grandTotal) * 100 : 0;
            }

            return new PaymentMethodBreakdownDto
            {
                StartDate = startDate,
                EndDate = endDate,
                PaymentMethodTotals = paymentMethodTotals,
                GrandTotal = grandTotal,
                TotalTransactionCount = transactions.Count
            };
        }

        /// <inheritdoc/>
        public async Task<ReconciliationValidationDto> ValidateReconciliationAsync(int sessionId)
        {
            var validation = new ReconciliationValidationDto
            {
                CanReconcile = true
            };

            // Find session
            var session = _activeSessions.Values.FirstOrDefault(s => s.SessionId == sessionId);
            if (session == null)
            {
                validation.CanReconcile = false;
                validation.Errors.Add($"Session {sessionId} not found");
                return validation;
            }

            if (session.Status != "Open")
            {
                validation.CanReconcile = false;
                validation.Errors.Add($"Session is already {session.Status}");
                return validation;
            }

            // Calculate session duration
            var duration = DateTime.UtcNow - session.OpenedAt;
            validation.SessionDurationHours = (decimal)duration.TotalHours;

            // Get expected amounts
            var expected = await CalculateExpectedAmountsAsync(sessionId);
            validation.ExpectedCash = expected.ExpectedCash;
            validation.TransactionCount = expected.TransactionCountByMethod.Values.Sum();

            // Add warnings
            if (validation.SessionDurationHours > 12)
            {
                validation.Warnings.Add($"Session has been open for {validation.SessionDurationHours:N1} hours - unusually long");
            }

            if (validation.TransactionCount > 500)
            {
                validation.Warnings.Add($"High transaction count ({validation.TransactionCount}) - verify all transactions processed");
            }

            if (validation.ExpectedCash > 10000)
            {
                validation.Warnings.Add($"Large cash amount expected ({validation.ExpectedCash:C2}) - verify count carefully");
                validation.RequiresManagerApproval = true;
            }

            return validation;
        }

        /// <inheritdoc/>
        public async Task<DenominationWorksheetDto> GenerateDenominationWorksheetAsync(int sessionId)
        {
            // Get expected amounts
            var expected = await CalculateExpectedAmountsAsync(sessionId);

            // Build worksheet
            var worksheet = new DenominationWorksheetDto
            {
                SessionId = sessionId,
                ExpectedTotal = expected.ExpectedCash,
                DenominationLines = new List<DenominationLineDto>
                {
                    new DenominationLineDto { DenominationName = "R200 notes", UnitValue = 200.00m },
                    new DenominationLineDto { DenominationName = "R100 notes", UnitValue = 100.00m },
                    new DenominationLineDto { DenominationName = "R50 notes", UnitValue = 50.00m },
                    new DenominationLineDto { DenominationName = "R20 notes", UnitValue = 20.00m },
                    new DenominationLineDto { DenominationName = "R10 notes", UnitValue = 10.00m },
                    new DenominationLineDto { DenominationName = "R5 coins", UnitValue = 5.00m },
                    new DenominationLineDto { DenominationName = "R2 coins", UnitValue = 2.00m },
                    new DenominationLineDto { DenominationName = "R1 coins", UnitValue = 1.00m },
                    new DenominationLineDto { DenominationName = "R0.50 coins", UnitValue = 0.50m },
                    new DenominationLineDto { DenominationName = "R0.20 coins", UnitValue = 0.20m },
                    new DenominationLineDto { DenominationName = "R0.10 coins", UnitValue = 0.10m },
                    new DenominationLineDto { DenominationName = "R0.05 coins", UnitValue = 0.05m }
                }
            };

            return worksheet;
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        private static int _sessionIdCounter = 0;
        private int GenerateSessionId()
        {
            return System.Threading.Interlocked.Increment(ref _sessionIdCounter);
        }

        /// <summary>
        /// Clears all static session state. FOR TESTING ONLY.
        /// This method is internal to allow test assemblies to reset state between tests.
        /// </summary>
        internal static void ResetSessionStateForTesting()
        {
            _activeSessions.Clear();
            _sessionIdCounter = 0;
        }
    }
}
