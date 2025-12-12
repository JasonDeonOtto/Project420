using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;
using Project420.Shared.Infrastructure.DTOs;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service implementation for POS refund business logic
    /// </summary>
    /// <remarks>
    /// Phase 7B: Integrated with MovementService for unified transaction architecture.
    /// After each refund is processed, movements are generated automatically.
    /// Refunds generate IN movements (products returned to stock).
    /// </remarks>
    public class RefundService : IRefundService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IVATCalculationService _vatService;
        private readonly ITransactionNumberGeneratorService _transactionNumberService;
        private readonly IMovementService _movementService;

        // Business rule constants
        private const int STANDARD_REFUND_WINDOW_DAYS = 30;
        private const decimal MANAGER_APPROVAL_THRESHOLD = 1000.00m;

        public RefundService(
            ITransactionRepository transactionRepository,
            IVATCalculationService vatService,
            ITransactionNumberGeneratorService transactionNumberService,
            IMovementService movementService)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _vatService = vatService ?? throw new ArgumentNullException(nameof(vatService));
            _transactionNumberService = transactionNumberService ?? throw new ArgumentNullException(nameof(transactionNumberService));
            _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));
        }

        /// <inheritdoc/>
        public async Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request)
        {
            // Validate request
            var validationErrors = await ValidateRefundRequestAsync(request);
            if (validationErrors.Any())
            {
                return new RefundResultDto
                {
                    Success = false,
                    ErrorMessages = validationErrors
                };
            }

            try
            {
                // 1. Get original transaction
                var originalTransaction = await _transactionRepository.GetByTransactionNumberAsync(
                    request.OriginalTransactionNumber);

                if (originalTransaction == null)
                {
                    return new RefundResultDto
                    {
                        Success = false,
                        ErrorMessages = new List<string> { $"Original transaction '{request.OriginalTransactionNumber}' not found" }
                    };
                }

                // 2. Generate refund transaction number
                var refundTransactionNumber = await _transactionNumberService.GenerateAsync(
                    TransactionTypeCode.CRN); // Credit Note for refunds

                // 3. Calculate line items with VAT reversal (Phase 7B: Using unified TransactionDetail)
                var refundDetails = new List<TransactionDetail>();
                foreach (var refundItem in request.RefundItems)
                {
                    var vatBreakdown = _vatService.CalculateLineItem(
                        unitPriceInclVAT: refundItem.UnitPriceInclVAT,
                        quantity: refundItem.QuantityToRefund);

                    var detail = new TransactionDetail
                    {
                        ProductId = refundItem.ProductId,
                        ProductSKU = refundItem.ProductSku,
                        ProductName = refundItem.ProductName,
                        Quantity = -refundItem.QuantityToRefund, // Negative for refund
                        UnitPrice = refundItem.UnitPriceInclVAT,
                        DiscountAmount = 0,
                        VATAmount = -vatBreakdown.TaxAmount, // Negative for refund
                        LineTotal = -vatBreakdown.Total, // Negative for refund
                        CostPrice = refundItem.CostPrice,
                        BatchNumber = refundItem.BatchNumber,
                        // HeaderId and TransactionType set by repository
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.ProcessedByUserId.ToString()
                    };

                    refundDetails.Add(detail);

                    // Update refund item with calculated values (for receipt)
                    refundItem.RefundSubtotal = vatBreakdown.Subtotal;
                    refundItem.RefundVATAmount = vatBreakdown.TaxAmount;
                    refundItem.RefundTotal = vatBreakdown.Total;
                }

                // 4. Calculate header totals (aggregate from details)
                // Note: TransactionDetail uses VATAmount and LineTotal (not Subtotal/TaxAmount)
                var vatAmount = Math.Abs(refundDetails.Sum(d => d.VATAmount));
                var total = Math.Abs(refundDetails.Sum(d => d.LineTotal));
                var subtotal = total - vatAmount;

                // 5. Create refund transaction header
                var refundHeader = new RetailTransactionHeader
                {
                    TransactionNumber = refundTransactionNumber,
                    TransactionDate = DateTime.UtcNow,
                    DebtorId = originalTransaction.DebtorId,
                    CustomerName = request.CustomerName ?? originalTransaction.CustomerName,
                    PricelistId = originalTransaction.PricelistId,
                    Subtotal = -subtotal, // Negative for refund
                    TaxAmount = -vatAmount, // Negative for refund
                    DiscountAmount = 0,
                    TotalAmount = -total, // Negative for refund
                    Status = TransactionStatus.Completed,
                    ProcessedBy = request.ProcessedByUserId.ToString(),
                    Notes = $"Refund for {request.OriginalTransactionNumber} - Reason: {request.RefundReason}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ProcessedByUserId.ToString()
                };

                // 6. Create refund payment (negative amount)
                var payment = new Payment
                {
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = request.RefundPaymentMethod,
                    Amount = -total, // Negative for refund
                    PaymentReference = $"Refund for {request.OriginalTransactionNumber}",
                    Notes = $"Refund payment for {refundTransactionNumber}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ProcessedByUserId.ToString()
                };

                // 7. Save to database (atomic transaction)
                var savedRefund = await _transactionRepository.ProcessRefundAsync(
                    request.OriginalTransactionNumber,
                    refundHeader,
                    refundDetails,
                    payment,
                    request.RefundReason);

                // 8. Generate movements for SOH calculation (Phase 7B - Movement Architecture)
                // Refunds generate IN movements (products returned to stock)
                // SOH is calculated from SUM(IN) - SUM(OUT) movements
                await _movementService.GenerateMovementsAsync(TransactionType.Refund, savedRefund.Id);

                // 9. Build refund result
                return new RefundResultDto
                {
                    Success = true,
                    RefundTransactionNumber = refundTransactionNumber,
                    OriginalTransactionNumber = request.OriginalTransactionNumber,
                    RefundDate = savedRefund.TransactionDate,
                    RefundSubtotal = subtotal,
                    RefundVATAmount = vatAmount,
                    RefundTotalAmount = total,
                    RefundedItems = request.RefundItems,
                    RefundPaymentMethod = request.RefundPaymentMethod.ToString(),
                    CustomerName = request.CustomerName ?? originalTransaction.CustomerName,
                    ProcessedByUserName = $"User {request.ProcessedByUserId}",
                    ApprovedByManagerName = request.ManagerApprovalUserId.HasValue
                        ? $"Manager {request.ManagerApprovalUserId}"
                        : null,
                    RefundReason = request.RefundReason.ToString(),
                    Notes = request.Notes
                };
            }
            catch (Exception ex)
            {
                return new RefundResultDto
                {
                    Success = false,
                    ErrorMessages = new List<string> { $"Refund processing failed: {ex.Message}" }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RefundEligibilityDto> ValidateRefundEligibilityAsync(string transactionNumber)
        {
            var eligibility = new RefundEligibilityDto
            {
                OriginalTransactionNumber = transactionNumber
            };

            try
            {
                // 1. Get original transaction
                var transaction = await _transactionRepository.GetByTransactionNumberAsync(transactionNumber);

                if (transaction == null)
                {
                    eligibility.IsEligible = false;
                    eligibility.IneligibilityReasons.Add("Transaction not found");
                    return eligibility;
                }

                // 2. Check if transaction is voided
                if (transaction.Status == TransactionStatus.Cancelled)
                {
                    eligibility.IsEligible = false;
                    eligibility.IneligibilityReasons.Add("Transaction has been voided/cancelled");
                    return eligibility;
                }

                // 3. Get refund history
                var refundHistory = await _transactionRepository.GetRefundHistoryAsync(transactionNumber);
                var alreadyRefunded = refundHistory.Sum(r => Math.Abs(r.TotalAmount));

                // 4. Calculate remaining refundable amount
                var originalAmount = transaction.TotalAmount;
                var remainingRefundable = originalAmount - alreadyRefunded;

                if (remainingRefundable <= 0)
                {
                    eligibility.IsEligible = false;
                    eligibility.IneligibilityReasons.Add("Transaction has been fully refunded");
                    return eligibility;
                }

                // 5. Check refund window
                var daysSince = (DateTime.UtcNow - transaction.TransactionDate).Days;
                var outsideWindow = daysSince > STANDARD_REFUND_WINDOW_DAYS;

                if (outsideWindow)
                {
                    eligibility.Warnings.Add($"Outside {STANDARD_REFUND_WINDOW_DAYS}-day refund window - manager approval required");
                    eligibility.RequiresManagerApproval = true;
                    eligibility.ManagerApprovalReason = $"Refund requested {daysSince} days after purchase";
                }

                // 6. Check if large amount
                if (remainingRefundable > MANAGER_APPROVAL_THRESHOLD)
                {
                    eligibility.RequiresManagerApproval = true;
                    eligibility.ManagerApprovalReason = eligibility.ManagerApprovalReason == null
                        ? $"Refund amount (R{remainingRefundable:N2}) exceeds threshold (R{MANAGER_APPROVAL_THRESHOLD:N2})"
                        : eligibility.ManagerApprovalReason + $" | Amount exceeds threshold";
                }

                // 7. Build refundable items list (Phase 7B: TransactionDetail uses decimal Quantity)
                eligibility.RefundableItems = transaction.TransactionDetails.Select(d =>
                {
                    var refundedQty = refundHistory
                        .SelectMany(r => r.TransactionDetails)
                        .Where(rd => rd.ProductId == d.ProductId)
                        .Sum(rd => Math.Abs(rd.Quantity));

                    return new RefundableItemDto
                    {
                        ProductId = d.ProductId,
                        ProductSku = d.ProductSKU,
                        ProductName = d.ProductName,
                        OriginalQuantity = (int)d.Quantity, // TransactionDetail uses decimal
                        AlreadyRefundedQuantity = (int)refundedQty,
                        RemainingRefundableQuantity = (int)(d.Quantity - refundedQty),
                        UnitPriceInclVAT = d.UnitPrice,
                        BatchNumber = d.BatchNumber,
                        CostPrice = d.CostPrice ?? 0
                    };
                }).Where(item => item.RemainingRefundableQuantity > 0).ToList();

                // 8. Set eligibility details
                eligibility.IsEligible = true;
                eligibility.OriginalTransactionDate = transaction.TransactionDate;
                eligibility.OriginalTotalAmount = originalAmount;
                eligibility.AlreadyRefundedAmount = alreadyRefunded;
                eligibility.RemainingRefundableAmount = remainingRefundable;
                eligibility.DaysSinceTransaction = daysSince;
                eligibility.OriginalPaymentMethod = transaction.Payments.FirstOrDefault()?.PaymentMethod.ToString();

                return eligibility;
            }
            catch (Exception ex)
            {
                eligibility.IsEligible = false;
                eligibility.IneligibilityReasons.Add($"Validation error: {ex.Message}");
                return eligibility;
            }
        }

        /// <inheritdoc/>
        public async Task<RefundResultDto?> GetRefundByTransactionNumberAsync(string refundTransactionNumber)
        {
            var refund = await _transactionRepository.GetByTransactionNumberAsync(refundTransactionNumber);

            if (refund == null || refund.TotalAmount >= 0)
                return null; // Not a refund transaction

            // Map to RefundResultDto
            return new RefundResultDto
            {
                Success = true,
                RefundTransactionNumber = refund.TransactionNumber,
                RefundDate = refund.TransactionDate,
                RefundSubtotal = Math.Abs(refund.Subtotal),
                RefundVATAmount = Math.Abs(refund.TaxAmount),
                RefundTotalAmount = Math.Abs(refund.TotalAmount),
                CustomerName = refund.CustomerName ?? "Unknown Customer",
                RefundPaymentMethod = refund.Payments.FirstOrDefault()?.PaymentMethod.ToString() ?? "Unknown",
                ProcessedByUserName = refund.ProcessedBy ?? "Unknown",
                // Phase 7B: TransactionDetail uses VATAmount and LineTotal
                RefundedItems = refund.TransactionDetails.Select(d => new RefundItemDto
                {
                    ProductId = d.ProductId,
                    ProductSku = d.ProductSKU,
                    ProductName = d.ProductName,
                    QuantityToRefund = (int)Math.Abs(d.Quantity),
                    UnitPriceInclVAT = d.UnitPrice,
                    BatchNumber = d.BatchNumber,
                    CostPrice = d.CostPrice ?? 0,
                    RefundSubtotal = Math.Abs(d.LineTotal - d.VATAmount),
                    RefundVATAmount = Math.Abs(d.VATAmount),
                    RefundTotal = Math.Abs(d.LineTotal)
                }).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<List<RefundResultDto>> GetRefundsForTransactionAsync(string originalTransactionNumber)
        {
            var refunds = await _transactionRepository.GetRefundHistoryAsync(originalTransactionNumber);

            return refunds.Select(refund => new RefundResultDto
            {
                Success = true,
                RefundTransactionNumber = refund.TransactionNumber,
                OriginalTransactionNumber = originalTransactionNumber,
                RefundDate = refund.TransactionDate,
                RefundSubtotal = Math.Abs(refund.Subtotal),
                RefundVATAmount = Math.Abs(refund.TaxAmount),
                RefundTotalAmount = Math.Abs(refund.TotalAmount),
                CustomerName = refund.CustomerName ?? "Unknown Customer",
                RefundPaymentMethod = refund.Payments.FirstOrDefault()?.PaymentMethod.ToString() ?? "Unknown",
                ProcessedByUserName = refund.ProcessedBy ?? "Unknown",
                // Phase 7B: TransactionDetail uses VATAmount and LineTotal
                RefundedItems = refund.TransactionDetails.Select(d => new RefundItemDto
                {
                    ProductId = d.ProductId,
                    ProductSku = d.ProductSKU,
                    ProductName = d.ProductName,
                    QuantityToRefund = (int)Math.Abs(d.Quantity),
                    UnitPriceInclVAT = d.UnitPrice,
                    BatchNumber = d.BatchNumber,
                    CostPrice = d.CostPrice ?? 0,
                    RefundSubtotal = Math.Abs(d.LineTotal - d.VATAmount),
                    RefundVATAmount = Math.Abs(d.VATAmount),
                    RefundTotal = Math.Abs(d.LineTotal)
                }).ToList()
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<RefundPreviewDto> CalculateRefundPreviewAsync(RefundRequestDto request)
        {
            var preview = new RefundPreviewDto();

            try
            {
                // 1. Validate eligibility
                var eligibility = await ValidateRefundEligibilityAsync(request.OriginalTransactionNumber);

                if (!eligibility.IsEligible)
                {
                    preview.IsValid = false;
                    preview.ValidationErrors = eligibility.IneligibilityReasons;
                    return preview;
                }

                // 2. Validate refund items
                var itemValidation = ValidateRefundItems(request.RefundItems, eligibility.RefundableItems);
                if (itemValidation.Any())
                {
                    preview.IsValid = false;
                    preview.ValidationErrors = itemValidation;
                    return preview;
                }

                // 3. Calculate refund amounts
                var itemBreakdown = new List<RefundPreviewItemDto>();
                decimal totalSubtotal = 0;
                decimal totalVAT = 0;
                decimal totalAmount = 0;

                foreach (var refundItem in request.RefundItems)
                {
                    var vatBreakdown = _vatService.CalculateLineItem(
                        unitPriceInclVAT: refundItem.UnitPriceInclVAT,
                        quantity: refundItem.QuantityToRefund);

                    itemBreakdown.Add(new RefundPreviewItemDto
                    {
                        ProductId = refundItem.ProductId,
                        ProductSku = refundItem.ProductSku,
                        ProductName = refundItem.ProductName,
                        QuantityToRefund = refundItem.QuantityToRefund,
                        UnitPriceInclVAT = refundItem.UnitPriceInclVAT,
                        RefundSubtotal = vatBreakdown.Subtotal,
                        RefundVATAmount = vatBreakdown.TaxAmount,
                        RefundTotal = vatBreakdown.Total,
                        BatchNumber = refundItem.BatchNumber
                    });

                    totalSubtotal += vatBreakdown.Subtotal;
                    totalVAT += vatBreakdown.TaxAmount;
                    totalAmount += vatBreakdown.Total;
                }

                // 4. Build preview
                preview.IsValid = true;
                preview.RefundSubtotal = totalSubtotal;
                preview.RefundVATAmount = totalVAT;
                preview.RefundTotalAmount = totalAmount;
                preview.ItemBreakdown = itemBreakdown;
                preview.OriginalTotalAmount = eligibility.OriginalTotalAmount;
                preview.AlreadyRefundedAmount = eligibility.AlreadyRefundedAmount;
                preview.RefundPercentage = eligibility.OriginalTotalAmount > 0
                    ? (totalAmount / eligibility.OriginalTotalAmount) * 100
                    : 0;

                // 5. Add warnings
                preview.Warnings = eligibility.Warnings;

                // 6. Check manager approval
                preview.RequiresManagerApproval = eligibility.RequiresManagerApproval || totalAmount > MANAGER_APPROVAL_THRESHOLD;
                if (totalAmount > MANAGER_APPROVAL_THRESHOLD && !eligibility.RequiresManagerApproval)
                {
                    preview.ManagerApprovalReason = $"Refund amount (R{totalAmount:N2}) exceeds threshold (R{MANAGER_APPROVAL_THRESHOLD:N2})";
                }
                else
                {
                    preview.ManagerApprovalReason = eligibility.ManagerApprovalReason;
                }

                return preview;
            }
            catch (Exception ex)
            {
                preview.IsValid = false;
                preview.ValidationErrors = new List<string> { $"Preview calculation failed: {ex.Message}" };
                return preview;
            }
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        /// <summary>
        /// Validate refund request before processing
        /// </summary>
        private async Task<List<string>> ValidateRefundRequestAsync(RefundRequestDto request)
        {
            var errors = new List<string>();

            // Original transaction number required
            if (string.IsNullOrWhiteSpace(request.OriginalTransactionNumber))
            {
                errors.Add("Original transaction number is required");
            }

            // Refund items required
            if (request.RefundItems == null || request.RefundItems.Count == 0)
            {
                errors.Add("Refund items are required - add at least one item to refund");
            }

            // Validate refund items
            if (request.RefundItems != null)
            {
                for (int i = 0; i < request.RefundItems.Count; i++)
                {
                    var item = request.RefundItems[i];

                    if (item.ProductId <= 0)
                        errors.Add($"Invalid product ID for item {i + 1}");

                    if (item.QuantityToRefund <= 0)
                        errors.Add($"Invalid refund quantity for {item.ProductName} - must be > 0");

                    if (item.UnitPriceInclVAT <= 0)
                        errors.Add($"Invalid price for {item.ProductName} - must be > 0");
                }
            }

            // Processor required
            if (request.ProcessedByUserId <= 0)
            {
                errors.Add("Processor/cashier ID is required for audit compliance");
            }

            // Validate refund reason
            if (!Enum.IsDefined(typeof(RefundReason), request.RefundReason))
            {
                errors.Add("Valid refund reason is required for compliance");
            }

            // Check if notes required for certain refund reasons
            var reasonsRequiringNotes = new[]
            {
                RefundReason.Other,
                RefundReason.ManagerOverride,
                RefundReason.ComplianceIssue
            };

            if (reasonsRequiringNotes.Contains(request.RefundReason) && string.IsNullOrWhiteSpace(request.Notes))
            {
                errors.Add($"Notes are required for refund reason: {request.RefundReason}");
            }

            // Validate eligibility (if we can)
            if (!string.IsNullOrWhiteSpace(request.OriginalTransactionNumber))
            {
                var eligibility = await ValidateRefundEligibilityAsync(request.OriginalTransactionNumber);

                if (!eligibility.IsEligible)
                {
                    errors.AddRange(eligibility.IneligibilityReasons);
                }
                else if (request.RefundItems != null)
                {
                    // Validate items against eligibility
                    var itemErrors = ValidateRefundItems(request.RefundItems, eligibility.RefundableItems);
                    errors.AddRange(itemErrors);

                    // Check manager approval requirement
                    if (eligibility.RequiresManagerApproval && !request.ManagerApprovalUserId.HasValue)
                    {
                        errors.Add($"Manager approval required: {eligibility.ManagerApprovalReason}");
                    }

                    // Check if refund amount requires manager approval
                    var refundTotal = request.RefundItems.Sum(i => i.UnitPriceInclVAT * i.QuantityToRefund);
                    if (refundTotal > MANAGER_APPROVAL_THRESHOLD && !request.ManagerApprovalUserId.HasValue)
                    {
                        errors.Add($"Manager approval required for refunds exceeding R{MANAGER_APPROVAL_THRESHOLD:N2}");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Validate refund items against refundable items
        /// </summary>
        private List<string> ValidateRefundItems(List<RefundItemDto> refundItems, List<RefundableItemDto> refundableItems)
        {
            var errors = new List<string>();

            foreach (var refundItem in refundItems)
            {
                var refundableItem = refundableItems.FirstOrDefault(r => r.ProductId == refundItem.ProductId);

                if (refundableItem == null)
                {
                    errors.Add($"Product '{refundItem.ProductName}' (ID: {refundItem.ProductId}) was not in the original transaction");
                    continue;
                }

                if (refundItem.QuantityToRefund > refundableItem.RemainingRefundableQuantity)
                {
                    errors.Add($"Refund quantity ({refundItem.QuantityToRefund}) for '{refundItem.ProductName}' " +
                              $"exceeds remaining refundable quantity ({refundableItem.RemainingRefundableQuantity})");
                }

                // Validate unit price matches original (within rounding tolerance)
                var priceDifference = Math.Abs(refundItem.UnitPriceInclVAT - refundableItem.UnitPriceInclVAT);
                if (priceDifference > 0.01m) // 1 cent tolerance
                {
                    errors.Add($"Unit price for '{refundItem.ProductName}' ({refundItem.UnitPriceInclVAT:C2}) " +
                              $"does not match original price ({refundableItem.UnitPriceInclVAT:C2})");
                }
            }

            return errors;
        }
    }
}
