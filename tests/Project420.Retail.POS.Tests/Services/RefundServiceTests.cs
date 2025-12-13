using FluentAssertions;
using Moq;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.BLL.Services;
using Project420.Retail.POS.Models.Entities;
using Project420.Retail.POS.Tests.Infrastructure;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Comprehensive tests for RefundService
/// Tests refund eligibility, processing, validation, and compliance
/// </summary>
public class RefundServiceTests : ServiceTestBase
{
    private readonly RefundService _service;

    public RefundServiceTests()
    {
        _service = new RefundService(
            MockTransactionRepository.Object,
            MockVATService.Object,
            MockTransactionNumberService.Object,
            MockMovementService.Object);
    }

    #region ProcessRefundAsync - Success Scenarios

    [Fact]
    public async Task ProcessRefundAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        var originalTransaction = CreateMockTransaction();
        var refundTransaction = CreateMockRefundTransaction();

        SetupMocksForSuccessfulRefund(originalTransaction, refundTransaction);

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.RefundTransactionNumber.Should().NotBeNullOrEmpty();
        result.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessRefundAsync_CalculatesRefundVATCorrectly()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        var originalTransaction = CreateMockTransaction();
        var refundTransaction = CreateMockRefundTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(100.00m, 1))
            .Returns(new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        SetupMocksForSuccessfulRefund(originalTransaction, refundTransaction);

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.RefundSubtotal.Should().Be(86.96m);
        result.RefundVATAmount.Should().Be(13.04m);
        result.RefundTotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task ProcessRefundAsync_CreatesNegativeTransactionAmounts()
    {
        // Refunds should be stored as negative transactions
        // Arrange
        var request = CreateValidRefundRequest();
        var originalTransaction = CreateMockTransaction();
        var refundTransaction = CreateMockRefundTransaction();

        RetailTransactionHeader? capturedHeader = null;

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(originalTransaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        MockTransactionRepository
            .Setup(x => x.ProcessRefundAsync(
                It.IsAny<string>(),
                It.IsAny<RetailTransactionHeader>(),
                It.IsAny<List<TransactionDetail>>(),
                It.IsAny<Payment>(),
                It.IsAny<RefundReason>()))
            .Callback<string, RetailTransactionHeader, List<TransactionDetail>, Payment, RefundReason>(
                (origNum, header, details, payment, reason) =>
                {
                    capturedHeader = header;
                })
            .ReturnsAsync(refundTransaction);

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        capturedHeader.Should().NotBeNull();
        capturedHeader!.TotalAmount.Should().BeLessThan(0, "Refund transaction should have negative total");
        capturedHeader.Subtotal.Should().BeLessThan(0, "Refund transaction should have negative subtotal");
        capturedHeader.TaxAmount.Should().BeLessThan(0, "Refund transaction should have negative VAT");
    }

    [Fact]
    public async Task ProcessRefundAsync_PartialRefund_RefundsCorrectAmount()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems[0].QuantityToRefund = 1; // Only refund 1 of 2 items

        var originalTransaction = CreateMockTransaction();
        var refundTransaction = CreateMockRefundTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(100.00m, 1))
            .Returns(new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        SetupMocksForSuccessfulRefund(originalTransaction, refundTransaction);

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.RefundTotalAmount.Should().Be(100.00m); // R100 for 1 item
    }

    #endregion

    #region ValidateRefundEligibilityAsync - Success Cases

    [Fact]
    public async Task ValidateRefundEligibilityAsync_WithEligibleTransaction_ReturnsEligible()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var transaction = CreateMockTransaction();
        transaction.TransactionDate = DateTime.UtcNow.AddDays(-10); // Within 30 days

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(transactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.Should().NotBeNull();
        result.IsEligible.Should().BeTrue();
        result.IneligibilityReasons.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRefundEligibilityAsync_OutsideRefundWindow_RequiresManagerApproval()
    {
        // Arrange
        var transactionNumber = "SALE-20251107-001";
        var transaction = CreateMockTransaction();
        transaction.TransactionDate = DateTime.UtcNow.AddDays(-40); // 40 days ago (outside 30-day window)

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(transactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.IsEligible.Should().BeTrue();
        result.RequiresManagerApproval.Should().BeTrue();
        result.ManagerApprovalReason.Should().Contain("40 days after purchase");
        result.Warnings.Should().Contain(w => w.Contains("Outside 30-day refund window"));
    }

    [Fact]
    public async Task ValidateRefundEligibilityAsync_LargeAmount_RequiresManagerApproval()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var transaction = CreateMockTransaction();
        transaction.TotalAmount = 1500.00m; // Above R1000 threshold
        transaction.TransactionDate = DateTime.UtcNow.AddDays(-5);

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(transactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.IsEligible.Should().BeTrue();
        result.RequiresManagerApproval.Should().BeTrue();
        result.ManagerApprovalReason.Should().Contain("exceeds threshold");
    }

    #endregion

    #region ValidateRefundEligibilityAsync - Ineligible Cases

    [Fact]
    public async Task ValidateRefundEligibilityAsync_TransactionNotFound_ReturnsIneligible()
    {
        // Arrange
        var transactionNumber = "INVALID-123";

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync((RetailTransactionHeader?)null);

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.IsEligible.Should().BeFalse();
        result.IneligibilityReasons.Should().Contain("Transaction not found");
    }

    [Fact]
    public async Task ValidateRefundEligibilityAsync_VoidedTransaction_ReturnsIneligible()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var transaction = CreateMockTransaction();
        transaction.Status = TransactionStatus.Cancelled;

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(transactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.IsEligible.Should().BeFalse();
        result.IneligibilityReasons.Should().Contain(r => r.Contains("voided") || r.Contains("cancelled"));
    }

    [Fact]
    public async Task ValidateRefundEligibilityAsync_FullyRefunded_ReturnsIneligible()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var transaction = CreateMockTransaction();
        transaction.TotalAmount = 200.00m;

        var previousRefund = new RetailTransactionHeader
        {
            TotalAmount = -200.00m, // Full refund already processed
            TransactionDetails = new List<TransactionDetail>()
        };

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(transactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader> { previousRefund });

        // Act
        var result = await _service.ValidateRefundEligibilityAsync(transactionNumber);

        // Assert
        result.IsEligible.Should().BeFalse();
        result.IneligibilityReasons.Should().Contain(r => r.Contains("fully refunded"));
    }

    #endregion

    #region ProcessRefundAsync - Validation Failures

    [Fact]
    public async Task ProcessRefundAsync_WithoutOriginalTransactionNumber_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.OriginalTransactionNumber = string.Empty;

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Original transaction number is required"));
    }

    [Fact]
    public async Task ProcessRefundAsync_WithoutRefundItems_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems.Clear();

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Refund items are required"));
    }

    [Fact]
    public async Task ProcessRefundAsync_WithInvalidProductId_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems[0].ProductId = 0;

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Invalid product ID"));
    }

    [Fact]
    public async Task ProcessRefundAsync_WithInvalidQuantity_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems[0].QuantityToRefund = 0;

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Invalid refund quantity"));
    }

    [Fact]
    public async Task ProcessRefundAsync_WithoutProcessorId_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.ProcessedByUserId = 0;

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Processor/cashier ID is required"));
    }

    [Fact]
    public async Task ProcessRefundAsync_OtherReason_WithoutNotes_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundReason = RefundReason.Other;
        request.Notes = null;

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Notes are required"));
    }

    [Fact]
    public async Task ProcessRefundAsync_LargeAmount_WithoutManagerApproval_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems[0].UnitPriceInclVAT = 1500.00m; // Above R1000 threshold
        request.ManagerApprovalUserId = null;

        var originalTransaction = CreateMockTransaction();
        originalTransaction.TotalAmount = 1500.00m;

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(originalTransaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        // Act
        var result = await _service.ProcessRefundAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Manager approval required"));
    }

    #endregion

    #region CalculateRefundPreviewAsync

    [Fact]
    public async Task CalculateRefundPreviewAsync_WithValidRequest_ReturnsPreview()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        var originalTransaction = CreateMockTransaction();

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(originalTransaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        MockVATService
            .Setup(x => x.CalculateLineItem(100.00m, 2))
            .Returns(new VATBreakdown
            {
                Subtotal = 173.91m,
                TaxAmount = 26.09m,
                Total = 200.00m
            });

        // Act
        var result = await _service.CalculateRefundPreviewAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.RefundSubtotal.Should().Be(173.91m);
        result.RefundVATAmount.Should().Be(26.09m);
        result.RefundTotalAmount.Should().Be(200.00m);
    }

    [Fact]
    public async Task CalculateRefundPreviewAsync_CalculatesRefundPercentage()
    {
        // Arrange
        var request = CreateValidRefundRequest();
        request.RefundItems[0].QuantityToRefund = 1; // Partial refund

        var originalTransaction = CreateMockTransaction();
        originalTransaction.TotalAmount = 200.00m;

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(originalTransaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(request.OriginalTransactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        MockVATService
            .Setup(x => x.CalculateLineItem(100.00m, 1))
            .Returns(new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        // Act
        var result = await _service.CalculateRefundPreviewAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RefundPercentage.Should().Be(50); // 100/200 = 50%
    }

    #endregion

    #region GetRefundByTransactionNumberAsync

    [Fact]
    public async Task GetRefundByTransactionNumberAsync_WithValidRefund_ReturnsRefund()
    {
        // Arrange
        var refundTransactionNumber = "CRN-20251207-001";
        var refundTransaction = CreateMockRefundTransaction();
        refundTransaction.TransactionNumber = refundTransactionNumber;
        refundTransaction.TotalAmount = -100.00m; // Negative for refund

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(refundTransactionNumber))
            .ReturnsAsync(refundTransaction);

        // Act
        var result = await _service.GetRefundByTransactionNumberAsync(refundTransactionNumber);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.RefundTransactionNumber.Should().Be(refundTransactionNumber);
        result.RefundTotalAmount.Should().Be(100.00m); // Absolute value
    }

    [Fact]
    public async Task GetRefundByTransactionNumberAsync_WithNonRefundTransaction_ReturnsNull()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var transaction = CreateMockTransaction();
        transaction.TotalAmount = 200.00m; // Positive (not a refund)

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.GetRefundByTransactionNumberAsync(transactionNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetRefundsForTransactionAsync

    [Fact]
    public async Task GetRefundsForTransactionAsync_WithMultipleRefunds_ReturnsAll()
    {
        // Arrange
        var originalTransactionNumber = "SALE-20251207-001";
        var refund1 = CreateMockRefundTransaction();
        refund1.TotalAmount = -50.00m;

        var refund2 = CreateMockRefundTransaction();
        refund2.TotalAmount = -75.00m;

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(originalTransactionNumber))
            .ReturnsAsync(new List<RetailTransactionHeader> { refund1, refund2 });

        // Act
        var result = await _service.GetRefundsForTransactionAsync(originalTransactionNumber);

        // Assert
        result.Should().HaveCount(2);
        result.Sum(r => r.RefundTotalAmount).Should().Be(125.00m); // 50 + 75
    }

    #endregion

    #region Helper Methods

    private RefundRequestDto CreateValidRefundRequest()
    {
        return new RefundRequestDto
        {
            OriginalTransactionNumber = "SALE-20251207-001",
            RefundItems = new List<RefundItemDto>
            {
                new RefundItemDto
                {
                    ProductId = 1,
                    ProductSku = "PROD001",
                    ProductName = "Product 1",
                    QuantityToRefund = 2,
                    UnitPriceInclVAT = 100.00m,
                    BatchNumber = "BATCH001",
                    CostPrice = 50.00m
                }
            },
            RefundReason = RefundReason.CustomerRequest,
            RefundPaymentMethod = PaymentMethod.Cash,
            ProcessedByUserId = 1,
            CustomerName = "John Doe"
        };
    }

    private RetailTransactionHeader CreateMockTransaction()
    {
        return new RetailTransactionHeader
        {
            Id = 1,
            TransactionNumber = "SALE-20251207-001",
            TransactionDate = DateTime.UtcNow.AddDays(-10),
            CustomerName = "John Doe",
            Subtotal = 173.91m,
            TaxAmount = 26.09m,
            TotalAmount = 200.00m,
            Status = TransactionStatus.Completed,
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    ProductId = 1,
                    ProductSKU = "PROD001",
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 100.00m,
                    VATAmount = 26.09m,
                    LineTotal = 200.00m,
                    BatchNumber = "BATCH001",
                    CostPrice = 50.00m
                }
            },
            Payments = new List<Payment>
            {
                new Payment
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = 200.00m
                }
            }
        };
    }

    private RetailTransactionHeader CreateMockRefundTransaction()
    {
        return new RetailTransactionHeader
        {
            Id = 2,
            TransactionNumber = "CRN-20251207-001",
            TransactionDate = DateTime.UtcNow,
            CustomerName = "John Doe",
            Subtotal = -173.91m,
            TaxAmount = -26.09m,
            TotalAmount = -200.00m,
            Status = TransactionStatus.Completed,
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    ProductId = 1,
                    ProductSKU = "PROD001",
                    ProductName = "Product 1",
                    Quantity = -2, // Negative for refund
                    UnitPrice = 100.00m,
                    VATAmount = -26.09m,
                    LineTotal = -200.00m
                }
            },
            Payments = new List<Payment>
            {
                new Payment
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = -200.00m
                }
            }
        };
    }

    private void SetupMocksForSuccessfulRefund(RetailTransactionHeader originalTransaction, RetailTransactionHeader refundTransaction)
    {
        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(originalTransaction);

        MockTransactionRepository
            .Setup(x => x.GetRefundHistoryAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<RetailTransactionHeader>());

        MockTransactionRepository
            .Setup(x => x.ProcessRefundAsync(
                It.IsAny<string>(),
                It.IsAny<RetailTransactionHeader>(),
                It.IsAny<List<TransactionDetail>>(),
                It.IsAny<Payment>(),
                It.IsAny<RefundReason>()))
            .ReturnsAsync(refundTransaction);

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });
    }

    #endregion
}
