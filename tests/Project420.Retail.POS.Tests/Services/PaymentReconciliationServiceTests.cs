using FluentAssertions;
using Moq;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.BLL.Services;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Retail.POS.Models.Entities;
using Project420.Retail.POS.Tests.Infrastructure;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Tests for PaymentReconciliationService
/// Focuses on cash drawer management and reconciliation logic
/// </summary>
public class PaymentReconciliationServiceTests
{
    private readonly PaymentReconciliationService _service;
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;

    public PaymentReconciliationServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();

        _service = new PaymentReconciliationService(
            _mockPaymentRepository.Object,
            _mockTransactionRepository.Object);
    }

    #region OpenCashDrawerAsync

    [Fact]
    public async Task OpenCashDrawerAsync_WithValidRequest_CreatesSession()
    {
        // Arrange
        var request = new CashDrawerOpenRequestDto
        {
            CashierId = 1,
            OpeningFloat = 500.00m,
            DenominationBreakdown = CreateDenominationBreakdown(500.00m)
        };

        // Act
        var result = await _service.OpenCashDrawerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CashierId.Should().Be(1);
        result.OpeningFloat.Should().Be(500.00m);
        result.Status.Should().Be("Open");
        result.SessionId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task OpenCashDrawerAsync_WithInvalidCashierId_ThrowsException()
    {
        // Arrange
        var request = new CashDrawerOpenRequestDto
        {
            CashierId = 0,
            OpeningFloat = 500.00m
        };

        // Act & Assert
        await _service.Invoking(s => s.OpenCashDrawerAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid cashier ID*");
    }

    [Fact]
    public async Task OpenCashDrawerAsync_WithNegativeFloat_ThrowsException()
    {
        // Arrange
        var request = new CashDrawerOpenRequestDto
        {
            CashierId = 1,
            OpeningFloat = -100.00m
        };

        // Act & Assert
        await _service.Invoking(s => s.OpenCashDrawerAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Opening float cannot be negative*");
    }

    #endregion

    #region ReconcileCashDrawerAsync

    [Fact]
    public async Task ReconcileCashDrawerAsync_WithAcceptableVariance_Succeeds()
    {
        // Arrange
        // First open a session
        var openRequest = new CashDrawerOpenRequestDto
        {
            CashierId = 1,
            OpeningFloat = 500.00m,
            DenominationBreakdown = CreateDenominationBreakdown(500.00m)
        };
        var session = await _service.OpenCashDrawerAsync(openRequest);

        // Setup mocks for reconciliation
        SetupReconciliationMocks();

        // Create close request with small variance (R5)
        var closeRequest = new CashDrawerCloseRequestDto
        {
            SessionId = session.SessionId,
            ActualCash = 505.00m, // R5 over (acceptable)
            DenominationBreakdown = CreateDenominationBreakdown(505.00m)
        };

        // Act
        var result = await _service.ReconcileCashDrawerAsync(closeRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Variance.Should().Be(5.00m);
        result.VarianceStatus.Should().Be("Acceptable");
    }

    [Fact]
    public async Task ReconcileCashDrawerAsync_WithLargeVariance_RequiresManagerApproval()
    {
        // Arrange
        var openRequest = new CashDrawerOpenRequestDto
        {
            CashierId = 1,
            OpeningFloat = 500.00m,
            DenominationBreakdown = CreateDenominationBreakdown(500.00m)
        };
        var session = await _service.OpenCashDrawerAsync(openRequest);

        SetupReconciliationMocks();

        // Large variance (R75) without manager approval
        var closeRequest = new CashDrawerCloseRequestDto
        {
            SessionId = session.SessionId,
            ActualCash = 575.00m, // R75 over (exceeds R50 threshold)
            DenominationBreakdown = CreateDenominationBreakdown(575.00m),
            VarianceReason = "Large variance"
        };

        // Act
        var result = await _service.ReconcileCashDrawerAsync(closeRequest);

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationMessages.Should().Contain(m => m.Contains("manager approval required"));
    }

    [Fact]
    public async Task ReconcileCashDrawerAsync_WithManagerApproval_Succeeds()
    {
        // Arrange
        var openRequest = new CashDrawerOpenRequestDto
        {
            CashierId = 1,
            OpeningFloat = 500.00m,
            DenominationBreakdown = CreateDenominationBreakdown(500.00m)
        };
        var session = await _service.OpenCashDrawerAsync(openRequest);

        SetupReconciliationMocks();

        var closeRequest = new CashDrawerCloseRequestDto
        {
            SessionId = session.SessionId,
            ActualCash = 575.00m,
            DenominationBreakdown = CreateDenominationBreakdown(575.00m),
            VarianceReason = "Customer returned cash from previous day",
            ApprovedByManagerId = 99
        };

        // Act
        var result = await _service.ReconcileCashDrawerAsync(closeRequest);

        // Assert
        result.Success.Should().BeTrue();
        result.Variance.Should().Be(75.00m);
        result.VarianceStatus.Should().Be("Approved");
        result.ApprovedByManager.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private DenominationBreakdownDto CreateDenominationBreakdown(decimal total)
    {
        return new DenominationBreakdownDto
        {
            Notes200 = (int)(total / 200),
            Notes100 = (int)((total % 200) / 100),
            Notes50 = (int)(((total % 200) % 100) / 50)
        };
    }

    private void SetupReconciliationMocks()
    {
        _mockPaymentRepository
            .Setup(x => x.GetPaymentSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new PaymentSummary());

        _mockTransactionRepository
            .Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<RetailTransactionHeader>());
    }

    #endregion
}
