using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service implementation for advanced POS transaction search and filtering
    /// </summary>
    public class TransactionSearchService : ITransactionSearchService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionSearchService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        /// <inheritdoc/>
        public async Task<TransactionSearchResultDto> SearchTransactionsAsync(TransactionSearchCriteriaDto criteria)
        {
            // Validate criteria
            if (criteria.PageNumber < 1)
                criteria.PageNumber = 1;

            if (criteria.PageSize < 1)
                criteria.PageSize = 50;

            if (criteria.PageSize > 500)
                criteria.PageSize = 500;

            // Get date range (default to last 30 days if not specified)
            var startDate = criteria.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = criteria.EndDate ?? DateTime.UtcNow;

            // Get transactions
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // Apply filters
            var filteredTransactions = ApplyFilters(transactions, criteria);

            // Apply sorting
            var sortedTransactions = ApplySorting(filteredTransactions, criteria);

            // Calculate pagination
            var totalCount = sortedTransactions.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)criteria.PageSize);
            var skip = (criteria.PageNumber - 1) * criteria.PageSize;

            // Get page
            var pageTransactions = sortedTransactions.Skip(skip).Take(criteria.PageSize).ToList();

            // Map to DTOs
            var transactionDtos = pageTransactions.Select(MapToSummaryDto).ToList();

            // Calculate summary
            var summary = new SearchSummaryDto
            {
                TotalSales = filteredTransactions.Where(t => t.TotalAmount > 0).Sum(t => t.TotalAmount),
                TotalRefunds = Math.Abs(filteredTransactions.Where(t => t.TotalAmount < 0).Sum(t => t.TotalAmount)),
                TransactionCount = totalCount,
                UniqueCustomerCount = filteredTransactions.Select(t => t.CustomerName).Distinct().Count()
            };
            summary.NetAmount = summary.TotalSales - summary.TotalRefunds;
            summary.AverageTransactionValue = totalCount > 0 ? summary.TotalSales / totalCount : 0;

            return new TransactionSearchResultDto
            {
                Transactions = transactionDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = criteria.PageNumber,
                PageSize = criteria.PageSize,
                Summary = summary
            };
        }

        /// <inheritdoc/>
        public async Task<TransactionDetailDto?> GetTransactionByNumberAsync(string transactionNumber)
        {
            var transaction = await _transactionRepository.GetByTransactionNumberAsync(transactionNumber);
            if (transaction == null)
                return null;

            return MapToDetailDto(transaction);
        }

        /// <inheritdoc/>
        public async Task<TransactionSearchResultDto> SearchTransactionsByCustomerAsync(
            string? customerName = null,
            int? debtorId = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var criteria = new TransactionSearchCriteriaDto
            {
                CustomerName = customerName,
                DebtorId = debtorId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortField = "TransactionDate",
                SortDirection = "Descending"
            };

            return await SearchTransactionsAsync(criteria);
        }

        /// <inheritdoc/>
        public async Task<TransactionSearchResultDto> SearchTransactionsByProductAsync(
            int? productId = null,
            string? productSku = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var criteria = new TransactionSearchCriteriaDto
            {
                ProductId = productId,
                ProductSku = productSku,
                StartDate = startDate,
                EndDate = endDate,
                SortField = "TransactionDate",
                SortDirection = "Descending"
            };

            return await SearchTransactionsAsync(criteria);
        }

        /// <inheritdoc/>
        public async Task<BatchTraceabilityDto> SearchTransactionsByBatchNumberAsync(string batchNumber)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
                throw new ArgumentException("Batch number is required", nameof(batchNumber));

            // Search transactions containing this batch number
            var criteria = new TransactionSearchCriteriaDto
            {
                BatchNumber = batchNumber,
                SortField = "TransactionDate",
                SortDirection = "Ascending"
            };

            var searchResult = await SearchTransactionsAsync(criteria);

            // Get transactions for detailed analysis
            var startDate = DateTime.UtcNow.AddYears(-1);
            var endDate = DateTime.UtcNow;
            var allTransactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // Filter transactions with this batch
            var batchTransactions = allTransactions
                .Where(t => t.TransactionDetails.Any(d => d.BatchNumber == batchNumber))
                .ToList();

            // Extract product info (from first occurrence)
            var firstDetail = batchTransactions
                .SelectMany(t => t.TransactionDetails)
                .FirstOrDefault(d => d.BatchNumber == batchNumber);

            ProductInfoDto? productInfo = null;
            if (firstDetail != null)
            {
                productInfo = new ProductInfoDto
                {
                    ProductId = firstDetail.ProductId,
                    ProductSku = firstDetail.ProductSKU ?? string.Empty,
                    ProductName = firstDetail.ProductName ?? string.Empty
                };
            }

            // Build batch transactions
            var batchTransactionDtos = batchTransactions.Select(t =>
            {
                var batchDetails = t.TransactionDetails.Where(d => d.BatchNumber == batchNumber).ToList();
                var quantity = batchDetails.Sum(d => Math.Abs(d.Quantity));
                var amount = batchDetails.Sum(d => Math.Abs(d.Total));

                return new BatchTransactionDto
                {
                    TransactionNumber = t.TransactionNumber,
                    TransactionDate = t.TransactionDate,
                    CustomerName = t.CustomerName ?? "Unknown",
                    QuantitySold = quantity,
                    Amount = amount,
                    CashierName = t.ProcessedBy ?? "Unknown"
                };
            }).ToList();

            // Calculate totals
            var totalQuantity = batchTransactionDtos.Sum(t => t.QuantitySold);
            var totalAmount = batchTransactionDtos.Sum(t => t.Amount);
            var uniqueCustomers = batchTransactionDtos.Select(t => t.CustomerName).Distinct().Count();

            return new BatchTraceabilityDto
            {
                BatchNumber = batchNumber,
                ProductInfo = productInfo,
                Transactions = batchTransactionDtos,
                TotalQuantitySold = totalQuantity,
                TotalSalesAmount = totalAmount,
                FirstSaleDate = batchTransactionDtos.Any() ? batchTransactionDtos.Min(t => t.TransactionDate) : (DateTime?)null,
                LastSaleDate = batchTransactionDtos.Any() ? batchTransactionDtos.Max(t => t.TransactionDate) : (DateTime?)null,
                UniqueCustomerCount = uniqueCustomers
            };
        }

        /// <inheritdoc/>
        public async Task<TransactionStatisticsDto> GetTransactionStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            string? groupBy = null)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            var stats = new TransactionStatisticsDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTransactionCount = transactions.Count,
                TotalSalesAmount = transactions.Where(t => t.TotalAmount > 0).Sum(t => t.TotalAmount),
                TotalRefundAmount = Math.Abs(transactions.Where(t => t.TotalAmount < 0).Sum(t => t.TotalAmount))
            };

            stats.NetSales = stats.TotalSalesAmount - stats.TotalRefundAmount;
            stats.AverageTransactionValue = stats.TotalTransactionCount > 0
                ? stats.TotalSalesAmount / stats.TotalTransactionCount
                : 0;

            var totalItems = transactions.SelectMany(t => t.TransactionDetails).Sum(d => Math.Abs(d.Quantity));
            stats.AverageItemsPerTransaction = stats.TotalTransactionCount > 0
                ? (decimal)totalItems / stats.TotalTransactionCount
                : 0;

            // Payment method statistics
            var paymentMethods = transactions
                .SelectMany(t => t.Payments)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodStats
                {
                    PaymentMethod = g.Key.ToString(),
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(p => Math.Abs(p.Amount)),
                    AverageValue = g.Count() > 0 ? g.Sum(p => Math.Abs(p.Amount)) / g.Count() : 0
                })
                .ToList();

            var totalPaymentAmount = paymentMethods.Sum(p => p.TotalAmount);
            foreach (var pm in paymentMethods)
            {
                pm.PercentageOfTotal = totalPaymentAmount > 0 ? (pm.TotalAmount / totalPaymentAmount) * 100 : 0;
                stats.PaymentMethodStatistics[pm.PaymentMethod] = pm;
            }

            // Grouping (if requested)
            if (!string.IsNullOrWhiteSpace(groupBy))
            {
                if (groupBy.Equals("Cashier", StringComparison.OrdinalIgnoreCase))
                {
                    stats.CashierStatistics = CalculateCashierStatistics(transactions);
                }
                else if (groupBy.Equals("Day", StringComparison.OrdinalIgnoreCase) ||
                         groupBy.Equals("Week", StringComparison.OrdinalIgnoreCase) ||
                         groupBy.Equals("Month", StringComparison.OrdinalIgnoreCase))
                {
                    stats.PeriodStatistics = CalculatePeriodStatistics(transactions, groupBy);
                }
            }

            return stats;
        }

        /// <inheritdoc/>
        public async Task<List<TransactionSummaryDto>> GetRecentTransactionsAsync(int count = 20, bool includeVoided = false)
        {
            if (count < 1)
                count = 20;

            if (count > 100)
                count = 100;

            // Get recent transactions (last 24 hours by default, extend if needed)
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-1);

            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // Filter voided if requested
            if (!includeVoided)
            {
                transactions = transactions.Where(t => t.Status != TransactionStatus.Cancelled).ToList();
            }

            // Sort by date descending and take count
            var recentTransactions = transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToList();

            return recentTransactions.Select(MapToSummaryDto).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<SuspiciousTransactionDto>> SearchSuspiciousTransactionsAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
            var suspicious = new List<SuspiciousTransactionDto>();

            // Group by cashier to detect patterns
            var transactionsByCashier = transactions.GroupBy(t => t.ProcessedBy);

            foreach (var cashierGroup in transactionsByCashier)
            {
                var cashierTransactions = cashierGroup.OrderBy(t => t.TransactionDate).ToList();

                // Detect potential duplicates (same amount, same items, within 5 minutes)
                for (int i = 0; i < cashierTransactions.Count - 1; i++)
                {
                    var current = cashierTransactions[i];
                    var next = cashierTransactions[i + 1];

                    if (Math.Abs(current.TotalAmount - next.TotalAmount) < 0.01m &&
                        (next.TransactionDate - current.TransactionDate).TotalMinutes <= 5)
                    {
                        var suspicion = new SuspiciousTransactionDto
                        {
                            TransactionNumber = current.TransactionNumber,
                            TransactionDate = current.TransactionDate,
                            SuspicionReasons = new List<string>
                            {
                                "Potential duplicate - same amount within 5 minutes"
                            },
                            RiskLevel = "Medium",
                            CashierName = current.ProcessedBy ?? "Unknown",
                            CustomerName = current.CustomerName ?? "Unknown",
                            TotalAmount = current.TotalAmount,
                            RelatedTransactions = new List<string> { next.TransactionNumber }
                        };

                        suspicious.Add(suspicion);
                    }
                }

                // Detect excessive voids
                var voidCount = cashierTransactions.Count(t => t.Status == TransactionStatus.Cancelled);
                var voidPercentage = cashierTransactions.Count > 0
                    ? (decimal)voidCount / cashierTransactions.Count * 100
                    : 0;

                if (voidPercentage > 20 && cashierTransactions.Count >= 10)
                {
                    var voidedTransactions = cashierTransactions
                        .Where(t => t.Status == TransactionStatus.Cancelled)
                        .ToList();

                    foreach (var voidedTx in voidedTransactions)
                    {
                        suspicious.Add(new SuspiciousTransactionDto
                        {
                            TransactionNumber = voidedTx.TransactionNumber,
                            TransactionDate = voidedTx.TransactionDate,
                            SuspicionReasons = new List<string>
                            {
                                $"Cashier has high void rate ({voidPercentage:N1}% of transactions)"
                            },
                            RiskLevel = "High",
                            CashierName = voidedTx.ProcessedBy ?? "Unknown",
                            CustomerName = voidedTx.CustomerName ?? "Unknown",
                            TotalAmount = voidedTx.TotalAmount
                        });
                    }
                }
            }

            // Detect large refunds without corresponding sale
            var refunds = transactions.Where(t => t.TotalAmount < 0 && Math.Abs(t.TotalAmount) > 500).ToList();
            foreach (var refund in refunds)
            {
                suspicious.Add(new SuspiciousTransactionDto
                {
                    TransactionNumber = refund.TransactionNumber,
                    TransactionDate = refund.TransactionDate,
                    SuspicionReasons = new List<string>
                    {
                        $"Large refund amount ({Math.Abs(refund.TotalAmount):C2}) - verify authorization"
                    },
                    RiskLevel = "Medium",
                    CashierName = refund.ProcessedBy ?? "Unknown",
                    CustomerName = refund.CustomerName ?? "Unknown",
                    TotalAmount = refund.TotalAmount
                });
            }

            return suspicious.Distinct().ToList();
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        private IQueryable<Models.Entities.POSTransactionHeader> ApplyFilters(
            List<Models.Entities.POSTransactionHeader> transactions,
            TransactionSearchCriteriaDto criteria)
        {
            var query = transactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.TransactionNumber))
            {
                query = query.Where(t => t.TransactionNumber.Contains(criteria.TransactionNumber));
            }

            if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
            {
                query = query.Where(t => t.CustomerName != null &&
                                        t.CustomerName.Contains(criteria.CustomerName, StringComparison.OrdinalIgnoreCase));
            }

            if (criteria.DebtorId.HasValue)
            {
                query = query.Where(t => t.DebtorId == criteria.DebtorId.Value);
            }

            if (criteria.Status.HasValue)
            {
                query = query.Where(t => t.Status == criteria.Status.Value);
            }

            if (criteria.MinAmount.HasValue)
            {
                query = query.Where(t => Math.Abs(t.TotalAmount) >= criteria.MinAmount.Value);
            }

            if (criteria.MaxAmount.HasValue)
            {
                query = query.Where(t => Math.Abs(t.TotalAmount) <= criteria.MaxAmount.Value);
            }

            if (criteria.PaymentMethod.HasValue)
            {
                query = query.Where(t => t.Payments.Any(p => p.PaymentMethod == criteria.PaymentMethod.Value));
            }

            if (criteria.ProductId.HasValue)
            {
                query = query.Where(t => t.TransactionDetails.Any(d => d.ProductId == criteria.ProductId.Value));
            }

            if (!string.IsNullOrWhiteSpace(criteria.ProductSku))
            {
                query = query.Where(t => t.TransactionDetails.Any(d => d.ProductSKU == criteria.ProductSku));
            }

            if (!string.IsNullOrWhiteSpace(criteria.BatchNumber))
            {
                query = query.Where(t => t.TransactionDetails.Any(d => d.BatchNumber == criteria.BatchNumber));
            }

            if (criteria.CashierId.HasValue)
            {
                query = query.Where(t => t.ProcessedBy == criteria.CashierId.Value.ToString());
            }

            return query;
        }

        private IOrderedQueryable<Models.Entities.POSTransactionHeader> ApplySorting(
            IQueryable<Models.Entities.POSTransactionHeader> query,
            TransactionSearchCriteriaDto criteria)
        {
            var sortDescending = criteria.SortDirection.Equals("Descending", StringComparison.OrdinalIgnoreCase);

            return criteria.SortField.ToLowerInvariant() switch
            {
                "transactionnumber" => sortDescending
                    ? query.OrderByDescending(t => t.TransactionNumber)
                    : query.OrderBy(t => t.TransactionNumber),
                "totalamount" => sortDescending
                    ? query.OrderByDescending(t => t.TotalAmount)
                    : query.OrderBy(t => t.TotalAmount),
                "customername" => sortDescending
                    ? query.OrderByDescending(t => t.CustomerName)
                    : query.OrderBy(t => t.CustomerName),
                _ => sortDescending
                    ? query.OrderByDescending(t => t.TransactionDate)
                    : query.OrderBy(t => t.TransactionDate)
            };
        }

        private TransactionSummaryDto MapToSummaryDto(Models.Entities.POSTransactionHeader transaction)
        {
            return new TransactionSummaryDto
            {
                TransactionId = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TotalAmount >= 0 ? "Sale" : "Refund",
                Status = transaction.Status.ToString(),
                CustomerName = transaction.CustomerName ?? "Walk-In Customer",
                TotalAmount = Math.Abs(transaction.TotalAmount),
                PaymentMethod = transaction.Payments.FirstOrDefault()?.PaymentMethod.ToString() ?? "Unknown",
                CashierName = transaction.ProcessedBy ?? "Unknown",
                ItemCount = transaction.TransactionDetails.Sum(d => Math.Abs(d.Quantity))
            };
        }

        private TransactionDetailDto MapToDetailDto(Models.Entities.POSTransactionHeader transaction)
        {
            return new TransactionDetailDto
            {
                Header = MapToSummaryDto(transaction),
                LineItems = transaction.TransactionDetails.Select(d => new TransactionLineItemDto
                {
                    ProductId = d.ProductId,
                    ProductSku = d.ProductSKU ?? string.Empty,
                    ProductName = d.ProductName ?? string.Empty,
                    Quantity = Math.Abs(d.Quantity),
                    UnitPrice = d.UnitPrice,
                    Subtotal = Math.Abs(d.Subtotal),
                    VATAmount = Math.Abs(d.TaxAmount),
                    Total = Math.Abs(d.Total),
                    BatchNumber = d.BatchNumber
                }).ToList(),
                Payments = transaction.Payments.Select(p => new PaymentDetailDto
                {
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Amount = Math.Abs(p.Amount),
                    PaymentReference = p.PaymentReference,
                    PaymentDate = p.PaymentDate
                }).ToList(),
                Subtotal = Math.Abs(transaction.Subtotal),
                VATAmount = Math.Abs(transaction.TaxAmount),
                DiscountAmount = transaction.DiscountAmount,
                TotalAmount = Math.Abs(transaction.TotalAmount),
                Notes = transaction.Notes
            };
        }

        private Dictionary<string, CashierStats> CalculateCashierStatistics(List<Models.Entities.POSTransactionHeader> transactions)
        {
            return transactions
                .Where(t => t.TotalAmount > 0) // Only sales
                .GroupBy(t => t.ProcessedBy)
                .ToDictionary(
                    g => g.Key ?? "Unknown",
                    g => new CashierStats
                    {
                        CashierName = g.Key ?? "Unknown",
                        TransactionCount = g.Count(),
                        TotalSales = g.Sum(t => t.TotalAmount),
                        AverageTransactionValue = g.Count() > 0 ? g.Sum(t => t.TotalAmount) / g.Count() : 0
                    }
                );
        }

        private List<PeriodStats> CalculatePeriodStatistics(List<Models.Entities.POSTransactionHeader> transactions, string groupBy)
        {
            var stats = new List<PeriodStats>();

            if (groupBy.Equals("Day", StringComparison.OrdinalIgnoreCase))
            {
                stats = transactions
                    .Where(t => t.TotalAmount > 0)
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new PeriodStats
                    {
                        PeriodStart = g.Key,
                        PeriodEnd = g.Key.AddDays(1).AddTicks(-1),
                        PeriodLabel = g.Key.ToString("yyyy-MM-dd"),
                        TransactionCount = g.Count(),
                        TotalSales = g.Sum(t => t.TotalAmount),
                        AverageTransactionValue = g.Count() > 0 ? g.Sum(t => t.TotalAmount) / g.Count() : 0
                    })
                    .OrderBy(p => p.PeriodStart)
                    .ToList();
            }
            // Add Week and Month grouping logic here if needed

            return stats;
        }
    }
}
