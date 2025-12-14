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
