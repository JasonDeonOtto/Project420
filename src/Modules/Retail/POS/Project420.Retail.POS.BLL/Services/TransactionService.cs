using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service implementation for POS transaction business logic
    /// </summary>
    /// <remarks>
    /// Phase 7B: Integrated with MovementService for unified transaction architecture.
    /// After each transaction is saved, movements are generated automatically.
    /// This ensures SOH (Stock on Hand) is always calculated from the Movement ledger.
    ///
    /// Phase 9.2: Enhanced with line-level and header-level discount support.
    /// - Line discounts: Applied to individual cart items with VAT recalculation
    /// - Header discounts: Prorated across lines with VAT recalculation
    /// - SA VAT Compliance: Discounts reduce total, then VAT is recalculated on discounted amount
    ///
    /// Phase 9.3: Enhanced with multi-tender checkout support.
    /// - Supports multiple payment methods per transaction (e.g., Cash + Card)
    /// - Creates multiple Payment records for split tenders
    /// - Change calculated from cash tenders only
    /// - FIC Act compliance for cash > R25,000
    /// </remarks>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IVATCalculationService _vatService;
        private readonly ITransactionNumberGeneratorService _transactionNumberService;
        private readonly IMovementService _movementService;
        private readonly IPOSCalculationService _calculationService;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IVATCalculationService vatService,
            ITransactionNumberGeneratorService transactionNumberService,
            IMovementService movementService,
            IPOSCalculationService calculationService)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _vatService = vatService ?? throw new ArgumentNullException(nameof(vatService));
            _transactionNumberService = transactionNumberService ?? throw new ArgumentNullException(nameof(transactionNumberService));
            _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        /// <inheritdoc/>
        public async Task<CheckoutResultDto> ProcessCheckoutAsync(CheckoutRequestDto request)
        {
            // Validate request
            var validationErrors = ValidateCheckoutRequest(request);
            if (validationErrors.Any())
            {
                return new CheckoutResultDto
                {
                    Success = false,
                    ErrorMessages = validationErrors
                };
            }

            try
            {
                // 1. Generate transaction number
                var transactionNumber = await _transactionNumberService.GenerateAsync(
                    TransactionTypeCode.SALE);

                // 2. Calculate line items with VAT and Line Discounts (Phase 9.2)
                var details = new List<TransactionDetail>();
                foreach (var cartItem in request.CartItems)
                {
                    // Calculate original line total (before discount)
                    decimal originalLineTotal = cartItem.UnitPriceInclVAT * cartItem.Quantity;

                    // Get line discount from cart item (Phase 9.2)
                    decimal lineDiscount = cartItem.DiscountAmount;

                    // If percentage discount specified, calculate the amount
                    if (lineDiscount == 0 && cartItem.DiscountPercentage.HasValue && cartItem.DiscountPercentage.Value > 0)
                    {
                        lineDiscount = _calculationService.CalculateDiscountAmount(originalLineTotal, cartItem.DiscountPercentage.Value);
                    }

                    // Calculate line totals with discount (recalculates VAT on discounted amount)
                    var (lineSubtotal, lineVat, lineTotal) = _calculationService.CalculateLineWithDiscount(originalLineTotal, lineDiscount);

                    var detail = new TransactionDetail
                    {
                        ProductId = cartItem.ProductId,
                        ProductSKU = cartItem.ProductSku,
                        ProductName = cartItem.ProductName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPriceInclVAT,
                        DiscountAmount = lineDiscount, // Phase 9.2: Store line discount
                        VATAmount = lineVat,           // VAT recalculated after discount
                        LineTotal = lineTotal,         // Total after discount
                        CostPrice = cartItem.CostPrice,
                        BatchNumber = cartItem.BatchNumber,
                        SerialNumber = cartItem.SerialNumber,
                        // HeaderId and TransactionType set by repository
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.ProcessedBy.ToString()
                    };

                    details.Add(detail);

                    // Update cart item with calculated values (for receipt)
                    cartItem.DiscountAmount = lineDiscount;
                    cartItem.LineSubtotal = lineSubtotal;
                    cartItem.LineVATAmount = lineVat;
                    cartItem.LineTotal = lineTotal;
                }

                // 3. Calculate header totals from details (includes line discounts)
                var lineDiscountTotal = details.Sum(d => d.DiscountAmount);
                var vatAmount = details.Sum(d => d.VATAmount);
                var total = details.Sum(d => d.LineTotal);
                var subtotal = total - vatAmount;

                // 4. Apply header-level discount (prorated across lines)
                // Phase 9.2: This is ADDITIONAL to any line-level discounts
                decimal headerDiscountAmount = 0;
                if (request.DiscountPercentage.HasValue && request.DiscountPercentage.Value > 0)
                {
                    headerDiscountAmount = _calculationService.CalculateDiscountAmount(total, request.DiscountPercentage.Value);
                }
                else if (request.DiscountAmount.HasValue && request.DiscountAmount.Value > 0)
                {
                    headerDiscountAmount = request.DiscountAmount.Value;
                }

                // If there's a header discount, recalculate VAT on final total
                decimal finalTotal = total;
                decimal finalVatAmount = vatAmount;
                decimal finalSubtotal = subtotal;

                if (headerDiscountAmount > 0)
                {
                    // Recalculate VAT on discounted total (SA compliance)
                    var (newSubtotal, newVat, newTotal) = _calculationService.CalculateLineWithDiscount(total, headerDiscountAmount);
                    finalTotal = newTotal;
                    finalVatAmount = newVat;
                    finalSubtotal = newSubtotal;
                }

                // Total discount = line discounts + header discount
                decimal totalDiscount = lineDiscountTotal + headerDiscountAmount;

                // 5. Create transaction header (Phase 9.2: Using recalculated VAT values)
                var header = new RetailTransactionHeader
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = DateTime.UtcNow,
                    DebtorId = request.DebtorId,
                    CustomerName = request.CustomerName,
                    PricelistId = request.PricelistId,
                    Subtotal = finalSubtotal,           // Subtotal after all discounts
                    TaxAmount = finalVatAmount,         // VAT recalculated after discounts
                    DiscountAmount = totalDiscount,     // Total of line + header discounts
                    TotalAmount = finalTotal,           // Final total after all discounts
                    Status = TransactionStatus.Completed,
                    ProcessedBy = request.ProcessedBy.ToString(),
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ProcessedBy.ToString()
                };

                // 6. Create payments (Phase 9.3: Multi-Tender Support)
                var payments = new List<Payment>();
                var paymentBreakdown = new PaymentBreakdownDto();

                if (request.IsMultiTender && request.Tenders != null)
                {
                    // Multi-tender checkout
                    foreach (var tender in request.Tenders)
                    {
                        var payment = new Payment
                        {
                            PaymentDate = DateTime.UtcNow,
                            PaymentMethod = tender.Method,
                            Amount = tender.Amount,
                            PaymentReference = tender.Reference,
                            ExternalReference = tender.AuthorizationCode,
                            MaskedCardNumber = tender.MaskedCardNumber,
                            BankOrProvider = tender.BankOrProvider,
                            IsSuccessful = tender.IsSuccessful,
                            Notes = tender.Notes ?? $"Payment for {transactionNumber}",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = request.ProcessedBy.ToString()
                        };
                        payments.Add(payment);

                        // Add to breakdown
                        paymentBreakdown.Tenders.Add(tender);
                    }
                }
                else
                {
                    // Legacy single-tender checkout (backwards compatibility)
                    var payment = new Payment
                    {
                        PaymentDate = DateTime.UtcNow,
                        PaymentMethod = request.PaymentMethod,
                        Amount = finalTotal,
                        PaymentReference = request.PaymentReference,
                        Notes = $"Payment for {transactionNumber}",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.ProcessedBy.ToString()
                    };
                    payments.Add(payment);

                    // Create single tender for breakdown
                    paymentBreakdown.Tenders.Add(new PaymentTenderDto
                    {
                        Method = request.PaymentMethod,
                        Amount = request.AmountTendered ?? finalTotal,
                        Reference = request.PaymentReference,
                        IsSuccessful = true
                    });
                }

                // Calculate totals and change for payment breakdown
                paymentBreakdown.TotalTendered = paymentBreakdown.Tenders.Sum(t => t.Amount);

                // Calculate change (only from cash tenders)
                decimal cashTendered = paymentBreakdown.Tenders
                    .Where(t => t.Method == PaymentMethod.Cash)
                    .Sum(t => t.Amount);
                decimal nonCashTendered = paymentBreakdown.Tenders
                    .Where(t => t.Method != PaymentMethod.Cash)
                    .Sum(t => t.Amount);
                decimal amountAfterNonCash = finalTotal - nonCashTendered;

                // Change is only from cash portion
                decimal changeDue = 0;
                if (cashTendered > amountAfterNonCash && amountAfterNonCash > 0)
                {
                    changeDue = cashTendered - amountAfterNonCash;
                }
                else if (cashTendered > 0 && amountAfterNonCash <= 0)
                {
                    // All paid by non-cash, cash is excess
                    changeDue = cashTendered;
                }

                paymentBreakdown.ChangeDue = changeDue;
                paymentBreakdown.OutstandingBalance = Math.Max(0, finalTotal - paymentBreakdown.TotalTendered + changeDue);

                // Determine primary payment method (largest tender)
                var primaryTender = paymentBreakdown.Tenders.OrderByDescending(t => t.Amount).FirstOrDefault();
                paymentBreakdown.PrimaryPaymentMethod = primaryTender?.Method.ToString() ?? "Unknown";

                // Generate payment method summary
                var methodNames = paymentBreakdown.Tenders.Select(t => t.Method.ToString()).Distinct();
                paymentBreakdown.PaymentMethodSummary = string.Join(" + ", methodNames);

                // 7. Save to database (atomic transaction - use multi-payment overload)
                var savedTransaction = await _transactionRepository.CreateSaleAsync(
                    header, details, payments);

                // 8. Generate movements for SOH calculation (Phase 7B - Movement Architecture)
                // This creates Movement records in the ledger for each TransactionDetail
                // SOH is calculated from SUM(IN) - SUM(OUT) movements
                await _movementService.GenerateMovementsAsync(TransactionType.Sale, savedTransaction.Id);

                // 9. Build checkout result (Phase 9.2 + 9.3: Include discount and payment breakdown)
                return new CheckoutResultDto
                {
                    Success = true,
                    TransactionId = savedTransaction.Id,
                    TransactionNumber = transactionNumber,
                    TransactionDate = savedTransaction.TransactionDate,
                    Subtotal = finalSubtotal,              // After discounts
                    VATAmount = finalVatAmount,            // Recalculated after discounts
                    DiscountAmount = totalDiscount,        // Total discounts (line + header)
                    TotalAmount = finalTotal,              // Final total after discounts
                    AmountTendered = paymentBreakdown.TotalTendered,
                    ChangeDue = changeDue > 0 ? changeDue : null,
                    PaymentBreakdown = paymentBreakdown,   // Phase 9.3: Full payment breakdown
                    CustomerName = request.CustomerName,
                    PaymentMethod = paymentBreakdown.PaymentMethodSummary,
                    ItemCount = request.CartItems.Count,
                    ProcessedBy = request.ProcessedBy.ToString(),
                    LineItems = request.CartItems,         // Includes per-line discounts
                    AgeVerificationDate = request.AgeVerificationDate, // Stored in DTO for receipt display
                    BatchNumbers = string.Join(", ", request.CartItems
                        .Where(c => !string.IsNullOrEmpty(c.BatchNumber))
                        .Select(c => c.BatchNumber)
                        .Distinct()),
                    SerialNumbers = string.Join(", ", request.CartItems
                        .Where(c => !string.IsNullOrEmpty(c.SerialNumber))
                        .Select(c => c.SerialNumber)
                        .Distinct())
                };
            }
            catch (Exception ex)
            {
                return new CheckoutResultDto
                {
                    Success = false,
                    ErrorMessages = new List<string> { $"Checkout failed: {ex.Message}" }
                };
            }
        }

        /// <inheritdoc/>
        public async Task<CheckoutResultDto?> GetTransactionByNumberAsync(string transactionNumber)
        {
            var transaction = await _transactionRepository.GetByTransactionNumberAsync(transactionNumber);
            if (transaction == null)
                return null;

            // Map to CheckoutResultDto (Phase 9.2: Include discount data)
            return new CheckoutResultDto
            {
                Success = true,
                TransactionId = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                TransactionDate = transaction.TransactionDate,
                Subtotal = transaction.Subtotal,
                VATAmount = transaction.TaxAmount,
                DiscountAmount = transaction.DiscountAmount,
                TotalAmount = transaction.TotalAmount,
                CustomerName = transaction.CustomerName ?? "Walk-In Customer",
                PaymentMethod = transaction.Payments.FirstOrDefault()?.PaymentMethod.ToString() ?? "Unknown",
                ItemCount = transaction.TransactionDetails.Count,
                ProcessedBy = transaction.ProcessedBy ?? "Unknown",
                // Phase 9.2: Include line-level discount data
                LineItems = transaction.TransactionDetails.Select(d => new CartItemDto
                {
                    ProductId = d.ProductId,
                    ProductSku = d.ProductSKU,
                    ProductName = d.ProductName,
                    UnitPriceInclVAT = d.UnitPrice,
                    Quantity = (int)d.Quantity, // TransactionDetail uses decimal for quantity
                    BatchNumber = d.BatchNumber,
                    SerialNumber = d.SerialNumber,
                    CostPrice = d.CostPrice ?? 0,
                    DiscountAmount = d.DiscountAmount,     // Line-level discount
                    LineSubtotal = d.LineTotal - d.VATAmount, // Calculate subtotal from LineTotal - VAT
                    LineVATAmount = d.VATAmount,
                    LineTotal = d.LineTotal
                }).ToList(),
                AgeVerificationDate = null, // Age verification stored in Debtor entity
                BatchNumbers = string.Join(", ", transaction.TransactionDetails
                    .Where(d => !string.IsNullOrEmpty(d.BatchNumber))
                    .Select(d => d.BatchNumber)
                    .Distinct()),
                SerialNumbers = string.Join(", ", transaction.TransactionDetails
                    .Where(d => !string.IsNullOrEmpty(d.SerialNumber))
                    .Select(d => d.SerialNumber)
                    .Distinct())
            };
        }

        /// <inheritdoc/>
        public async Task<bool> VoidTransactionAsync(string transactionNumber, string voidReason, int userId)
        {
            var transaction = await _transactionRepository.GetByTransactionNumberAsync(transactionNumber);
            if (transaction == null)
                return false;

            // Phase 7B: Reverse movements first (soft delete in Movement ledger)
            // This ensures SOH is recalculated correctly after void
            await _movementService.ReverseMovementsAsync(
                transaction.TransactionType,
                transaction.Id,
                $"Transaction voided: {voidReason}");

            return await _transactionRepository.VoidTransactionAsync(
                transaction.Id,
                voidReason,
                userId);
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        // ========================================
        // PHASE 9.6: TRANSACTION CANCELLATION
        // ========================================

        /// <inheritdoc/>
        public async Task<CancellationEligibilityDto> ValidateCancellationEligibilityAsync(string transactionNumber)
        {
            var result = new CancellationEligibilityDto
            {
                TransactionNumber = transactionNumber
            };

            // Get the transaction
            var transaction = await _transactionRepository.GetByTransactionNumberAsync(transactionNumber);
            if (transaction == null)
            {
                result.IsEligible = false;
                result.IneligibilityReasons.Add($"Transaction '{transactionNumber}' not found");
                return result;
            }

            // Populate transaction details
            result.Status = transaction.Status;
            result.TransactionDate = transaction.TransactionDate;
            result.TotalAmount = transaction.TotalAmount;
            result.MinutesSinceTransaction = (int)(DateTime.UtcNow - transaction.TransactionDate).TotalMinutes;
            result.ItemCount = transaction.TransactionDetails.Count;
            result.PaymentMethod = transaction.Payments.FirstOrDefault()?.PaymentMethod.ToString() ?? "Unknown";

            // Map items
            result.Items = transaction.TransactionDetails.Select(d => new CancellationItemDto
            {
                ProductId = d.ProductId,
                ProductSku = d.ProductSKU,
                ProductName = d.ProductName,
                Quantity = (int)d.Quantity,
                UnitPrice = d.UnitPrice,
                LineTotal = d.LineTotal,
                BatchNumber = d.BatchNumber,
                SerialNumber = d.SerialNumber
            }).ToList();

            // Check eligibility based on status
            if (transaction.Status == TransactionStatus.Cancelled)
            {
                result.IsEligible = false;
                result.IneligibilityReasons.Add("Transaction is already cancelled");
                return result;
            }

            if (transaction.Status == TransactionStatus.Refunded)
            {
                result.IsEligible = false;
                result.IneligibilityReasons.Add("Transaction has been refunded - cannot cancel a refunded transaction");
                return result;
            }

            // Check if transaction is too old (>24 hours = cannot cancel, must refund)
            if (result.MinutesSinceTransaction > 1440) // 24 hours
            {
                result.IsEligible = false;
                result.IneligibilityReasons.Add("Transaction is older than 24 hours - please use the refund process instead");
                return result;
            }

            // Transaction is eligible
            result.IsEligible = true;

            // Determine if manager approval is required
            // Manager approval required for:
            // 1. Completed transactions (payment already taken)
            // 2. Transactions > 30 minutes old
            // 3. Transactions > R1000 total
            if (transaction.Status == TransactionStatus.Completed)
            {
                result.RequiresManagerApproval = true;
                result.ManagerApprovalReason = "Manager approval required for completed transactions";
            }
            else if (result.MinutesSinceTransaction > 30)
            {
                result.RequiresManagerApproval = true;
                result.ManagerApprovalReason = "Manager approval required for transactions over 30 minutes old";
            }
            else if (transaction.TotalAmount > 1000)
            {
                result.RequiresManagerApproval = true;
                result.ManagerApprovalReason = "Manager approval required for transactions over R1,000";
            }

            // Add warnings
            if (result.MinutesSinceTransaction > 60 && result.MinutesSinceTransaction <= 1440)
            {
                result.Warnings.Add($"Transaction is {result.MinutesSinceTransaction / 60} hours old - consider using refund process instead");
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                result.Warnings.Add("Cancelling a completed transaction will reverse stock movements and require payment reversal");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<CancellationResultDto> ProcessCancellationAsync(CancellationRequestDto request)
        {
            var result = new CancellationResultDto
            {
                TransactionNumber = request.TransactionNumber
            };

            // Handle pre-payment cancellation (cart clear - no database changes needed)
            if (request.IsPrePaymentCancellation)
            {
                result.Success = true;
                result.CancellationDate = DateTime.UtcNow;
                result.Reason = request.Reason;
                result.ProcessedByUserId = request.ProcessedByUserId;
                result.MovementsReversed = 0;
                result.RequiresPaymentReversal = false;
                return result;
            }

            // For completed transactions, validate and process
            var eligibility = await ValidateCancellationEligibilityAsync(request.TransactionNumber);
            if (!eligibility.IsEligible)
            {
                result.Success = false;
                result.ErrorMessages.AddRange(eligibility.IneligibilityReasons);
                return result;
            }

            // Validate manager approval if required
            if (eligibility.RequiresManagerApproval)
            {
                if (request.ManagerApprovalUserId == null || string.IsNullOrEmpty(request.ManagerPin))
                {
                    result.Success = false;
                    result.ErrorMessages.Add("Manager approval is required for this cancellation");
                    return result;
                }

                var managerValidation = await ValidateManagerPinAsync(request.ManagerApprovalUserId.Value, request.ManagerPin);
                if (!managerValidation.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessages.Add(managerValidation.ErrorMessage ?? "Invalid manager credentials");
                    return result;
                }
            }

            try
            {
                // Get transaction for status check
                var transaction = await _transactionRepository.GetByTransactionNumberAsync(request.TransactionNumber);
                if (transaction == null)
                {
                    result.Success = false;
                    result.ErrorMessages.Add("Transaction not found");
                    return result;
                }

                // Build cancellation reason for audit
                var cancellationReason = $"Reason: {request.Reason}";
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    cancellationReason += $" | Notes: {request.Notes}";
                }
                if (request.ManagerApprovalUserId.HasValue)
                {
                    cancellationReason += $" | Manager Approved: User ID {request.ManagerApprovalUserId}";
                }
                cancellationReason += $" | Cancelled by: User ID {request.ProcessedByUserId}";

                // Reverse movements (Phase 7B - soft delete movements)
                var movementsReversed = await _movementService.ReverseMovementsAsync(
                    transaction.TransactionType,
                    transaction.Id,
                    cancellationReason);

                // Void the transaction
                var voided = await _transactionRepository.VoidTransactionAsync(
                    transaction.Id,
                    cancellationReason,
                    request.ProcessedByUserId);

                if (!voided)
                {
                    result.Success = false;
                    result.ErrorMessages.Add("Failed to void transaction");
                    return result;
                }

                // Build success result
                result.Success = true;
                result.CancellationDate = DateTime.UtcNow;
                result.MovementsReversed = movementsReversed;
                result.Reason = request.Reason;
                result.ProcessedByUserId = request.ProcessedByUserId;
                result.ManagerApprovalUserId = request.ManagerApprovalUserId;

                // Payment reversal info
                if (eligibility.Status == TransactionStatus.Completed)
                {
                    result.RequiresPaymentReversal = true;
                    result.RefundAmount = eligibility.TotalAmount;
                    result.OriginalPaymentMethod = eligibility.PaymentMethod;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessages.Add($"Cancellation failed: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<ManagerValidationDto> ValidateManagerPinAsync(int managerId, string pin)
        {
            // Simple PIN validation for PoC
            // In production, this should validate against user management system
            // For now, accept any 4+ digit PIN for manager IDs > 0

            if (managerId <= 0)
            {
                return new ManagerValidationDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid manager ID"
                };
            }

            if (string.IsNullOrEmpty(pin) || pin.Length < 4)
            {
                return new ManagerValidationDto
                {
                    IsValid = false,
                    ErrorMessage = "PIN must be at least 4 digits"
                };
            }

            // For PoC: Accept PIN "1234" for any valid manager ID
            // or any numeric PIN >= 4 digits
            if (pin == "1234" || (pin.All(char.IsDigit) && pin.Length >= 4))
            {
                return new ManagerValidationDto
                {
                    IsValid = true,
                    ManagerName = $"Manager #{managerId}" // Would lookup from user service in production
                };
            }

            return new ManagerValidationDto
            {
                IsValid = false,
                ErrorMessage = "Invalid PIN"
            };
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================

        /// <summary>
        /// Validate checkout request before processing
        /// </summary>
        private List<string> ValidateCheckoutRequest(CheckoutRequestDto request)
        {
            var errors = new List<string>();

            // Age verification (Cannabis Act requirement)
            if (!request.AgeVerified)
            {
                errors.Add("Age verification required - customer must be 18+ to purchase cannabis products");
            }

            // Cart must have items
            if (request.CartItems == null || request.CartItems.Count == 0)
            {
                errors.Add("Cart is empty - add at least one item to checkout");
            }

            // Validate cart items
            if (request.CartItems != null)
            {
                for (int i = 0; i < request.CartItems.Count; i++)
                {
                    var item = request.CartItems[i];

                    if (item.ProductId <= 0)
                        errors.Add($"Invalid product ID for item {i + 1}");

                    if (item.Quantity <= 0)
                        errors.Add($"Invalid quantity for {item.ProductName} - must be > 0");

                    if (item.UnitPriceInclVAT <= 0)
                        errors.Add($"Invalid price for {item.ProductName} - must be > 0");
                }
            }

            // Payment validation
            if (request.PaymentMethod == PaymentMethod.Cash && request.AmountTendered.HasValue)
            {
                var estimatedTotal = request.CartItems?.Sum(c => c.UnitPriceInclVAT * c.Quantity) ?? 0;
                if (request.AmountTendered.Value < estimatedTotal)
                {
                    errors.Add("Amount tendered is less than total - please provide sufficient payment");
                }
            }

            // Customer name required
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                errors.Add("Customer name is required - use 'Walk-In Customer' for anonymous sales");
            }

            // Processor required
            if (request.ProcessedBy <= 0)
            {
                errors.Add("Processor/cashier ID is required for audit compliance");
            }

            return errors;
        }
    }
}
