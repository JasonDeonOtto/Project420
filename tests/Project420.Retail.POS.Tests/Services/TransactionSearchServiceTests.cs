using FluentAssertions;
using Moq;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.BLL.Services;
using Project420.Retail.POS.Models.Entities;
using Project420.Retail.POS.Tests.Infrastructure;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Tests for TransactionSearchService
/// Focuses on search, filtering, and compliance traceability
/// </summary>
public class TransactionSearchServiceTests : ServiceTestBase
{
    private readonly TransactionSearchService _service;

    public TransactionSearchServiceTests()
    {
        _service = new TransactionSearchService(MockTransactionRepository.Object);
    }

    #region SearchTransactionsAsync

    [Fact]
    public async Task SearchTransactionsAsync_WithEmptyCriteria_ReturnsAllTransactions()
    {
        // Arrange
        var criteria = new TransactionSearchCriteriaDto
        {
            PageNumber = 1,
            PageSize = 50
        };

        var mockTransactions = CreateMockTransactionList();

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.SearchTransactionsAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Transactions.Should().HaveCount(mockTransactions.Count);
        result.TotalCount.Should().Be(mockTransactions.Count);
    }

    [Fact]
    public async Task SearchTransactionsAsync_WithCustomerNameFilter_FiltersCorrectly()
    {
        // Arrange
        var criteria = new TransactionSearchCriteriaDto
        {
            CustomerName = "John",
            PageNumber = 1,
            PageSize = 50
        };

        var mockTransactions = CreateMockTransactionList();

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.SearchTransactionsAsync(criteria);

        // Assert
        result.Transactions.Should().OnlyContain(t => t.CustomerName.Contains("John"));
    }

    [Fact]
    public async Task SearchTransactionsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var criteria = new TransactionSearchCriteriaDto
        {
            PageNumber = 2,
            PageSize = 2
        };

        var mockTransactions = CreateMockTransactionList(); // 3 transactions

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.SearchTransactionsAsync(criteria);

        // Assert
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2); // 3 items / 2 per page = 2 pages
        result.Transactions.Should().HaveCountLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchTransactionsAsync_CalculatesSummaryCorrectly()
    {
        // Arrange
        var criteria = new TransactionSearchCriteriaDto();

        var mockTransactions = new List<RetailTransactionHeader>
        {
            CreateTransaction(1, "SALE-001", 200.00m),
            CreateTransaction(2, "SALE-002", 150.00m),
            CreateTransaction(3, "CRN-001", -50.00m) // Refund
        };

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.SearchTransactionsAsync(criteria);

        // Assert
        result.Summary.Should().NotBeNull();
        result.Summary.TotalSales.Should().Be(350.00m); // 200 + 150
        result.Summary.TotalRefunds.Should().Be(50.00m);
        result.Summary.NetAmount.Should().Be(300.00m); // 350 - 50
        result.Summary.TransactionCount.Should().Be(3);
    }

    #endregion

    #region GetTransactionStatisticsAsync

    [Fact]
    public async Task GetTransactionStatisticsAsync_CalculatesCorrectly()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var mockTransactions = new List<RetailTransactionHeader>
        {
            CreateTransaction(1, "SALE-001", 200.00m),
            CreateTransaction(2, "SALE-002", 300.00m),
            CreateTransaction(3, "CRN-001", -100.00m) // Refund
        };

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.GetTransactionStatisticsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalTransactionCount.Should().Be(3);
        result.TotalSalesAmount.Should().Be(500.00m);
        result.TotalRefundAmount.Should().Be(100.00m);
        result.NetSales.Should().Be(400.00m);
        result.AverageTransactionValue.Should().BeApproximately(166.67m, 0.01m); // 500/3 ≈ 166.67
    }

    #endregion

    #region SearchTransactionsByBatchNumberAsync (Compliance-Critical)

    [Fact]
    public async Task SearchTransactionsByBatchNumberAsync_FindsAllTransactionsForBatch()
    {
        // Arrange
        var batchNumber = "BATCH-001";

        var mockTransactions = new List<RetailTransactionHeader>
        {
            CreateTransactionWithBatch(1, "SALE-001", 200.00m, batchNumber, "Product A", 2),
            CreateTransactionWithBatch(2, "SALE-002", 150.00m, batchNumber, "Product A", 1),
            CreateTransactionWithBatch(3, "SALE-003", 300.00m, "BATCH-002", "Product B", 3) // Different batch
        };

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.SearchTransactionsByBatchNumberAsync(batchNumber);

        // Assert
        result.Should().NotBeNull();
        result.BatchNumber.Should().Be(batchNumber);
        result.Transactions.Should().HaveCount(2); // Only BATCH-001 transactions
        result.TotalQuantitySold.Should().Be(3); // 2 + 1
    }

    [Fact]
    public async Task SearchTransactionsByBatchNumberAsync_WithEmptyBatch_ThrowsException()
    {
        // Act & Assert
        await _service.Invoking(s => s.SearchTransactionsByBatchNumberAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Batch number is required*");
    }

    #endregion

    #region GetRecentTransactionsAsync

    [Fact]
    public async Task GetRecentTransactionsAsync_ReturnsLimitedResults()
    {
        // Arrange
        var mockTransactions = CreateMockTransactionList();

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.GetRecentTransactionsAsync(count: 2);

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_ExcludesVoidedByDefault()
    {
        // Arrange
        var mockTransactions = new List<RetailTransactionHeader>
        {
            CreateTransaction(1, "SALE-001", 200.00m, TransactionStatus.Completed),
            CreateTransaction(2, "SALE-002", 150.00m, TransactionStatus.Cancelled), // Voided
            CreateTransaction(3, "SALE-003", 300.00m, TransactionStatus.Completed)
        };

        MockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockTransactions);

        // Act
        var result = await _service.GetRecentTransactionsAsync(count: 20, includeVoided: false);

        // Assert
        result.Should().OnlyContain(t => t.Status == "Completed");
    }

    #endregion

    #region Helper Methods

    private List<RetailTransactionHeader> CreateMockTransactionList()
    {
        return new List<RetailTransactionHeader>
        {
            CreateTransaction(1, "SALE-001", 200.00m, customerName: "John Doe"),
            CreateTransaction(2, "SALE-002", 150.00m, customerName: "Jane Smith"),
            CreateTransaction(3, "SALE-003", 300.00m, customerName: "John Davis")
        };
    }

    private RetailTransactionHeader CreateTransaction(
        int id,
        string transactionNumber,
        decimal totalAmount,
        TransactionStatus status = TransactionStatus.Completed,
        string customerName = "Customer")
    {
        return new RetailTransactionHeader
        {
            Id = id,
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow.AddDays(-id),
            CustomerName = customerName,
            Subtotal = totalAmount / 1.15m,
            TaxAmount = totalAmount - (totalAmount / 1.15m),
            TotalAmount = totalAmount,
            Status = status,
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    ProductId = 1,
                    ProductSKU = "PROD001",
                    ProductName = "Product 1",
                    Quantity = 1,
                    LineTotal = totalAmount,
                    VATAmount = totalAmount - (totalAmount / 1.15m)
                }
            },
            Payments = new List<Payment>
            {
                new Payment
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = totalAmount
                }
            }
        };
    }

    private RetailTransactionHeader CreateTransactionWithBatch(
        int id,
        string transactionNumber,
        decimal totalAmount,
        string batchNumber,
        string productName,
        int quantity)
    {
        return new RetailTransactionHeader
        {
            Id = id,
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow.AddDays(-id),
            CustomerName = "Customer",
            TotalAmount = totalAmount,
            Status = TransactionStatus.Completed,
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    ProductId = 1,
                    ProductSKU = "PROD001",
                    ProductName = productName,
                    Quantity = quantity,
                    BatchNumber = batchNumber,
                    LineTotal = totalAmount,
                    VATAmount = totalAmount - (totalAmount / 1.15m)
                }
            },
            Payments = new List<Payment>
            {
                new Payment
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = totalAmount
                }
            }
        };
    }

    #endregion
}
