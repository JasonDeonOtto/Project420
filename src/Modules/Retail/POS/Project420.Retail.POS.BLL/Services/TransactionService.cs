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
    /// </remarks>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IVATCalculationService _vatService;
        private readonly ITransactionNumberGeneratorService _transactionNumberService;
        private readonly IMovementService _movementService;

        public TransactionService(
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

                // 2. Calculate line items with VAT (Phase 7B: Using unified TransactionDetail)
                var details = new List<TransactionDetail>();
                foreach (var cartItem in request.CartItems)
                {
                    var vatBreakdown = _vatService.CalculateLineItem(
                        unitPriceInclVAT: cartItem.UnitPriceInclVAT,
                        quantity: cartItem.Quantity);

                    var detail = new TransactionDetail
                    {
                        ProductId = cartItem.ProductId,
                        ProductSKU = cartItem.ProductSku,
                        ProductName = cartItem.ProductName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPriceInclVAT,
                        DiscountAmount = 0,
                        VATAmount = vatBreakdown.TaxAmount,
                        LineTotal = vatBreakdown.Total,
                        CostPrice = cartItem.CostPrice,
                        BatchNumber = cartItem.BatchNumber,
                        // HeaderId and TransactionType set by repository
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.ProcessedBy.ToString()
                    };

                    details.Add(detail);

                    // Update cart item with calculated values (for receipt)
                    cartItem.LineSubtotal = vatBreakdown.Subtotal;
                    cartItem.LineVATAmount = vatBreakdown.TaxAmount;
                    cartItem.LineTotal = vatBreakdown.Total;
                }

                // 3. Calculate header totals (aggregate from details)
                // Note: TransactionDetail uses VATAmount and LineTotal (not Subtotal/TaxAmount)
                var vatAmount = details.Sum(d => d.VATAmount);
                var total = details.Sum(d => d.LineTotal);
                var subtotal = total - vatAmount;

                // 4. Apply discounts if any
                decimal discountAmount = 0;
                if (request.DiscountPercentage.HasValue)
                {
                    discountAmount = total * (request.DiscountPercentage.Value / 100);
                }
                else if (request.DiscountAmount.HasValue)
                {
                    discountAmount = request.DiscountAmount.Value;
                }

                var finalTotal = total - discountAmount;

                // 5. Create transaction header
                var header = new RetailTransactionHeader
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = DateTime.UtcNow,
                    DebtorId = request.DebtorId,
                    CustomerName = request.CustomerName,
                    PricelistId = request.PricelistId,
                    Subtotal = subtotal,
                    TaxAmount = vatAmount,
                    DiscountAmount = discountAmount,
                    TotalAmount = finalTotal,
                    Status = TransactionStatus.Completed,
                    ProcessedBy = request.ProcessedBy.ToString(),
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ProcessedBy.ToString()
                };

                // 6. Create payment
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

                // 7. Save to database (atomic transaction)
                var savedTransaction = await _transactionRepository.CreateSaleAsync(
                    header, details, payment);

                // 8. Generate movements for SOH calculation (Phase 7B - Movement Architecture)
                // This creates Movement records in the ledger for each TransactionDetail
                // SOH is calculated from SUM(IN) - SUM(OUT) movements
                await _movementService.GenerateMovementsAsync(TransactionType.Sale, savedTransaction.Id);

                // 9. Calculate change (for cash payments)
                decimal? changeDue = null;
                if (request.PaymentMethod == PaymentMethod.Cash && request.AmountTendered.HasValue)
                {
                    changeDue = request.AmountTendered.Value - finalTotal;
                }

                // 10. Build checkout result
                return new CheckoutResultDto
                {
                    Success = true,
                    TransactionId = savedTransaction.Id,
                    TransactionNumber = transactionNumber,
                    TransactionDate = savedTransaction.TransactionDate,
                    Subtotal = subtotal,
                    VATAmount = vatAmount,
                    DiscountAmount = discountAmount,
                    TotalAmount = finalTotal,
                    AmountTendered = request.AmountTendered,
                    ChangeDue = changeDue,
                    CustomerName = request.CustomerName,
                    PaymentMethod = request.PaymentMethod.ToString(),
                    ItemCount = request.CartItems.Count,
                    ProcessedBy = request.ProcessedBy.ToString(),
                    LineItems = request.CartItems,
                    AgeVerificationDate = request.AgeVerificationDate, // Stored in DTO for receipt display
                    BatchNumbers = string.Join(", ", request.CartItems
                        .Where(c => !string.IsNullOrEmpty(c.BatchNumber))
                        .Select(c => c.BatchNumber)
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

            // Map to CheckoutResultDto
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
                // Phase 7B: TransactionDetail uses different property names
                LineItems = transaction.TransactionDetails.Select(d => new CartItemDto
                {
                    ProductId = d.ProductId,
                    ProductSku = d.ProductSKU,
                    ProductName = d.ProductName,
                    UnitPriceInclVAT = d.UnitPrice,
                    Quantity = (int)d.Quantity, // TransactionDetail uses decimal for quantity
                    BatchNumber = d.BatchNumber,
                    CostPrice = d.CostPrice ?? 0,
                    LineSubtotal = d.LineTotal - d.VATAmount, // Calculate subtotal from LineTotal - VAT
                    LineVATAmount = d.VATAmount,
                    LineTotal = d.LineTotal
                }).ToList(),
                AgeVerificationDate = null, // Age verification stored in Debtor entity
                BatchNumbers = string.Join(", ", transaction.TransactionDetails
                    .Where(d => !string.IsNullOrEmpty(d.BatchNumber))
                    .Select(d => d.BatchNumber)
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
