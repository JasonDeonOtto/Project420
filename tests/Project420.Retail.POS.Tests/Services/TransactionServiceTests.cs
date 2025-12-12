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
/// Comprehensive tests for TransactionService
/// </summary>
public class TransactionServiceTests : ServiceTestBase
{
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(
            MockTransactionRepository.Object,
            MockVATService.Object,
            MockTransactionNumberService.Object,
            MockMovementService.Object);
    }

    #region ProcessCheckoutAsync - Success Scenarios

    [Fact]
    public async Task ProcessCheckoutAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransactionNumber.Should().NotBeNullOrEmpty();
        result.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessCheckoutAsync_CalculatesVATCorrectly()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(100.00m, 2))
            .Returns(new VATBreakdown
            {
                Subtotal = 173.91m,
                TaxAmount = 26.09m,
                Total = 200.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Subtotal.Should().Be(173.91m);
        result.VATAmount.Should().Be(26.09m);
        result.TotalAmount.Should().Be(200.00m);
    }

    [Fact]
    public async Task ProcessCheckoutAsync_AppliesPercentageDiscount()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.DiscountPercentage = 10; // 10% discount

        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.DiscountAmount.Should().Be(10.00m); // 10% of 100.00
        result.TotalAmount.Should().Be(90.00m); // 100.00 - 10.00
    }

    [Fact]
    public async Task ProcessCheckoutAsync_AppliesFixedDiscount()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.DiscountAmount = 15.00m; // R15 discount

        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.DiscountAmount.Should().Be(15.00m);
        result.TotalAmount.Should().Be(85.00m); // 100.00 - 15.00
    }

    [Fact]
    public async Task ProcessCheckoutAsync_CalculatesChangeDueForCashPayment()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.PaymentMethod = PaymentMethod.Cash;
        request.AmountTendered = 150.00m;

        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.AmountTendered.Should().Be(150.00m);
        result.ChangeDue.Should().Be(50.00m); // 150.00 - 100.00
    }

    [Fact]
    public async Task ProcessCheckoutAsync_NoChangeDueForCardPayment()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.PaymentMethod = PaymentMethod.Card;

        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .Setup(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns((decimal price, int qty) => new VATBreakdown
            {
                Subtotal = 86.96m,
                TaxAmount = 13.04m,
                Total = 100.00m
            });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ChangeDue.Should().BeNull();
    }

    [Fact]
    public async Task ProcessCheckoutAsync_MultipleItems_CalculatesTotalCorrectly()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CartItems.Add(new CartItemDto
        {
            ProductId = 2,
            ProductSku = "PROD002",
            ProductName = "Product 2",
            UnitPriceInclVAT = 50.00m,
            Quantity = 1,
            BatchNumber = "BATCH002",
            CostPrice = 30.00m
        });

        var expectedTransaction = CreateMockTransaction();

        MockVATService
            .SetupSequence(x => x.CalculateLineItem(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns(new VATBreakdown { Subtotal = 86.96m, TaxAmount = 13.04m, Total = 100.00m })
            .Returns(new VATBreakdown { Subtotal = 43.48m, TaxAmount = 6.52m, Total = 50.00m });

        MockTransactionRepository
            .Setup(x => x.CreateSaleAsync(It.IsAny<RetailTransactionHeader>(), It.IsAny<List<TransactionDetail>>(), It.IsAny<Payment>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Subtotal.Should().Be(130.44m); // 86.96 + 43.48
        result.VATAmount.Should().Be(19.56m); // 13.04 + 6.52
        result.TotalAmount.Should().Be(150.00m); // 100.00 + 50.00
        result.ItemCount.Should().Be(2);
    }

    #endregion

    #region ProcessCheckoutAsync - Validation Failures

    [Fact]
    public async Task ProcessCheckoutAsync_WithoutAgeVerification_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.AgeVerified = false;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().ContainSingle();
        result.ErrorMessages[0].Should().Contain("Age verification required");
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithEmptyCart_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CartItems.Clear();

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Cart is empty"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithInvalidProductId_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CartItems[0].ProductId = 0;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Invalid product ID"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithInvalidQuantity_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CartItems[0].Quantity = 0;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Invalid quantity"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithInvalidPrice_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CartItems[0].UnitPriceInclVAT = 0;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Invalid price"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithInsufficientCashTendered_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.PaymentMethod = PaymentMethod.Cash;
        request.AmountTendered = 50.00m; // Less than the total

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Amount tendered is less than total"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithoutCustomerName_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.CustomerName = string.Empty;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Customer name is required"));
    }

    [Fact]
    public async Task ProcessCheckoutAsync_WithInvalidProcessorId_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidCheckoutRequest();
        request.ProcessedBy = 0;

        // Act
        var result = await _service.ProcessCheckoutAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessages.Should().Contain(e => e.Contains("Processor/cashier ID is required"));
    }

    #endregion

    #region GetTransactionByNumberAsync

    [Fact]
    public async Task GetTransactionByNumberAsync_WithValidNumber_ReturnsTransaction()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var mockTransaction = CreateMockTransaction();
        mockTransaction.TransactionNumber = transactionNumber;

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(mockTransaction);

        // Act
        var result = await _service.GetTransactionByNumberAsync(transactionNumber);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.TransactionNumber.Should().Be(transactionNumber);
    }

    [Fact]
    public async Task GetTransactionByNumberAsync_WithInvalidNumber_ReturnsNull()
    {
        // Arrange
        var transactionNumber = "INVALID-123";

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync((RetailTransactionHeader?)null);

        // Act
        var result = await _service.GetTransactionByNumberAsync(transactionNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region VoidTransactionAsync

    [Fact]
    public async Task VoidTransactionAsync_WithValidNumber_VoidsTransaction()
    {
        // Arrange
        var transactionNumber = "SALE-20251207-001";
        var voidReason = "Customer request";
        var userId = 1;
        var mockTransaction = CreateMockTransaction();

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync(mockTransaction);

        MockTransactionRepository
            .Setup(x => x.VoidTransactionAsync(mockTransaction.Id, voidReason, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VoidTransactionAsync(transactionNumber, voidReason, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VoidTransactionAsync_WithInvalidNumber_ReturnsFalse()
    {
        // Arrange
        var transactionNumber = "INVALID-123";
        var voidReason = "Customer request";
        var userId = 1;

        MockTransactionRepository
            .Setup(x => x.GetByTransactionNumberAsync(transactionNumber))
            .ReturnsAsync((RetailTransactionHeader?)null);

        // Act
        var result = await _service.VoidTransactionAsync(transactionNumber, voidReason, userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private CheckoutRequestDto CreateValidCheckoutRequest()
    {
        return new CheckoutRequestDto
        {
            DebtorId = 1,
            CustomerName = "John Doe",
            AgeVerified = true,
            AgeVerificationDate = DateTime.UtcNow,
            CartItems = new List<CartItemDto>
            {
                new CartItemDto
                {
                    ProductId = 1,
                    ProductSku = "PROD001",
                    ProductName = "Product 1",
                    UnitPriceInclVAT = 100.00m,
                    Quantity = 2,
                    BatchNumber = "BATCH001",
                    CostPrice = 50.00m
                }
            },
            PaymentMethod = PaymentMethod.Card,
            ProcessedBy = 1,
            PricelistId = 1
        };
    }

    private RetailTransactionHeader CreateMockTransaction()
    {
        return new RetailTransactionHeader
        {
            Id = 1,
            TransactionNumber = "SALE-20251207-001",
            TransactionDate = DateTime.UtcNow,
            DebtorId = 1,
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
                    LineTotal = 200.00m
                }
            },
            Payments = new List<Payment>
            {
                new Payment
                {
                    PaymentMethod = PaymentMethod.Card,
                    Amount = 200.00m
                }
            }
        };
    }

    #endregion
}
