using FluentAssertions;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.BLL.Services;
using Project420.Shared.Core.Enums;
using Xunit;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Unit tests for ReceiptService
/// Phase 9.8: Compliant Receipt Generation
/// </summary>
public class ReceiptServiceTests
{
    private readonly ReceiptService _service;

    public ReceiptServiceTests()
    {
        _service = new ReceiptService();
    }

    // ========================================
    // GENERATE RECEIPT FROM CHECKOUT TESTS
    // ========================================

    [Fact]
    public void GenerateReceiptFromCheckout_ValidCheckout_ReturnsReceipt()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.Should().NotBeNull();
        receipt.ReceiptNumber.Should().Be(checkoutResult.TransactionNumber);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesBusinessDetails()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.BusinessName.Should().NotBeEmpty();
        receipt.BusinessAddress.Should().NotBeEmpty();
        receipt.VATRegistrationNumber.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesSAHPRALicense()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.SAHPRALicenseNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesDALRRDPermit()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.DALRRDPermitNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateReceiptFromCheckout_ConvertsLineItems()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.LineItems.Should().HaveCount(checkoutResult.LineItems.Count);
        receipt.LineItems[0].ProductName.Should().Be(checkoutResult.LineItems[0].ProductName);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesBatchNumbers()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.LineItems[0].BatchNumber = "BATCH-2025-001";

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.LineItems[0].BatchNumber.Should().Be("BATCH-2025-001");
    }

    [Fact]
    public void GenerateReceiptFromCheckout_CalculatesVATBreakdown()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.SubtotalExclVAT.Should().Be(checkoutResult.Subtotal);
        receipt.VATAmount.Should().Be(checkoutResult.VATAmount);
        receipt.TotalInclVAT.Should().Be(checkoutResult.TotalAmount);
        receipt.VATRate.Should().Be(15m);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesPaymentDetails()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.Payments.Should().HaveCount(1);
        receipt.Payments[0].PaymentMethod.Should().Be(checkoutResult.PaymentMethod);
        receipt.Payments[0].Amount.Should().Be(checkoutResult.TotalAmount);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesChangeDue()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.AmountTendered = 200m;
        checkoutResult.ChangeDue = 50m;

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.AmountTendered.Should().Be(200m);
        receipt.ChangeDue.Should().Be(50m);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_IncludesAgeVerificationStatus()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.AgeVerificationDate = DateTime.UtcNow;

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.AgeVerified.Should().BeTrue();
        receipt.AgeVerificationMethod.Should().Be("ID Verified");
    }

    [Fact]
    public void GenerateReceiptFromCheckout_WithoutAgeVerification_ShowsNotVerified()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.AgeVerificationDate = null;

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        receipt.AgeVerified.Should().BeFalse();
        receipt.AgeVerificationMethod.Should().BeNull();
    }

    // ========================================
    // COMPLIANCE NOTICES TESTS
    // ========================================

    [Fact]
    public void GetComplianceNotices_WithAgeVerified_IncludesVerificationNotice()
    {
        // Act
        var notices = _service.GetComplianceNotices(true, "850101*****86");

        // Assert
        notices.Should().Contain(n => n.Contains("850101*****86"));
    }

    [Fact]
    public void GetComplianceNotices_IncludesDailyLimitNotice()
    {
        // Act
        var notices = _service.GetComplianceNotices(true, null);

        // Assert
        notices.Should().Contain(n => n.Contains("daily limit"));
    }

    [Fact]
    public void GetComplianceNotices_IncludesTraceabilityNotice()
    {
        // Act
        var notices = _service.GetComplianceNotices(true, null);

        // Assert
        notices.Should().Contain(n => n.Contains("traceability"));
    }

    // ========================================
    // LEGAL DISCLAIMERS TESTS
    // ========================================

    [Fact]
    public void GetCannabisLegalDisclaimers_ReturnsDisclaimers()
    {
        // Act
        var disclaimers = _service.GetCannabisLegalDisclaimers();

        // Assert
        disclaimers.Should().NotBeEmpty();
        disclaimers.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void GetCannabisLegalDisclaimers_IncludesAgeRestriction()
    {
        // Act
        var disclaimers = _service.GetCannabisLegalDisclaimers();

        // Assert
        disclaimers.Should().Contain(d => d.Contains("18+") || d.Contains("adult"));
    }

    [Fact]
    public void GetCannabisLegalDisclaimers_IncludesResponsibleUse()
    {
        // Act
        var disclaimers = _service.GetCannabisLegalDisclaimers();

        // Assert
        disclaimers.Should().Contain(d => d.Contains("drive") || d.Contains("machinery"));
    }

    [Fact]
    public void GetCannabisLegalDisclaimers_IncludesCannabisAct()
    {
        // Act
        var disclaimers = _service.GetCannabisLegalDisclaimers();

        // Assert
        disclaimers.Should().Contain(d => d.Contains("Cannabis") && d.Contains("Act"));
    }

    // ========================================
    // FORMAT RECEIPT AS TEXT TESTS
    // ========================================

    [Fact]
    public void FormatReceiptAsText_ReturnsFormattedText()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var text = _service.FormatReceiptAsText(receipt);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Should().Contain(receipt.BusinessName.ToUpper());
    }

    [Fact]
    public void FormatReceiptAsText_IncludesVATNumber()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var text = _service.FormatReceiptAsText(receipt);

        // Assert
        text.Should().Contain("VAT:");
        text.Should().Contain(receipt.VATRegistrationNumber);
    }

    [Fact]
    public void FormatReceiptAsText_IncludesLineItems()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var text = _service.FormatReceiptAsText(receipt);

        // Assert
        foreach (var item in receipt.LineItems)
        {
            text.Should().Contain(item.ProductName.Substring(0, Math.Min(item.ProductName.Length, 20)));
        }
    }

    [Fact]
    public void FormatReceiptAsText_IncludesBatchNumber()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.LineItems[0].BatchNumber = "BATCH-TEST-001";
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var text = _service.FormatReceiptAsText(receipt);

        // Assert
        text.Should().Contain("Batch: BATCH-TEST-001");
    }

    [Fact]
    public void FormatReceiptAsText_IncludesTotals()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var text = _service.FormatReceiptAsText(receipt);

        // Assert
        text.Should().Contain("Subtotal");
        text.Should().Contain("VAT");
        text.Should().Contain("TOTAL");
    }

    // ========================================
    // FORMAT RECEIPT AS HTML TESTS
    // ========================================

    [Fact]
    public void FormatReceiptAsHtml_ReturnsValidHtml()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var html = _service.FormatReceiptAsHtml(receipt);

        // Assert
        html.Should().NotBeNullOrEmpty();
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html>");
        html.Should().Contain("</html>");
    }

    [Fact]
    public void FormatReceiptAsHtml_IncludesBusinessName()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var html = _service.FormatReceiptAsHtml(receipt);

        // Assert
        html.Should().Contain(receipt.BusinessName);
    }

    [Fact]
    public void FormatReceiptAsHtml_IncludesComplianceSection()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.AgeVerificationDate = DateTime.UtcNow;
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var html = _service.FormatReceiptAsHtml(receipt);

        // Assert
        html.Should().Contain("compliance");
    }

    [Fact]
    public void FormatReceiptAsHtml_IncludesDisclaimers()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Act
        var html = _service.FormatReceiptAsHtml(receipt);

        // Assert
        html.Should().Contain("disclaimers");
    }

    // ========================================
    // BUSINESS SETTINGS TESTS
    // ========================================

    [Fact]
    public void GetDefaultBusinessSettings_ReturnsValidSettings()
    {
        // Act
        var settings = _service.GetDefaultBusinessSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.BusinessName.Should().NotBeEmpty();
        settings.VATNumber.Should().NotBeEmpty();
        settings.ReceiptWidth.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateReceiptFromCheckout_WithCustomSettings_UsesCustomSettings()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        var customSettings = new BusinessReceiptSettings
        {
            BusinessName = "Custom Cannabis Shop",
            VATNumber = "VAT-CUSTOM-123",
            SAHPRALicense = "SAHPRA-CUSTOM-001"
        };

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult, customSettings);

        // Assert
        receipt.BusinessName.Should().Be("Custom Cannabis Shop");
        receipt.VATRegistrationNumber.Should().Be("VAT-CUSTOM-123");
        receipt.SAHPRALicenseNumber.Should().Be("SAHPRA-CUSTOM-001");
    }

    // ========================================
    // LINE ITEM VAT CALCULATION TESTS
    // ========================================

    [Fact]
    public void GenerateReceiptFromCheckout_CalculatesLineItemExclVAT()
    {
        // Arrange
        var checkoutResult = CreateValidCheckoutResult();
        checkoutResult.LineItems[0].UnitPriceInclVAT = 115m; // R115 incl VAT

        // Act
        var receipt = _service.GenerateReceiptFromCheckout(checkoutResult);

        // Assert
        // R115 / 1.15 = R100 excl VAT
        receipt.LineItems[0].UnitPriceExclVAT.Should().Be(100m);
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private static CheckoutResultDto CreateValidCheckoutResult()
    {
        return new CheckoutResultDto
        {
            Success = true,
            TransactionId = 1,
            TransactionNumber = "SALE-20251214-001",
            TransactionDate = DateTime.Now,
            CustomerName = "Test Customer",
            PaymentMethod = "Cash",
            ItemCount = 2,
            Subtotal = 173.91m, // R200 / 1.15
            VATAmount = 26.09m, // R200 - R173.91
            DiscountAmount = 0m,
            TotalAmount = 200m,
            AgeVerificationDate = DateTime.UtcNow,
            LineItems = new List<CartItemDto>
            {
                new CartItemDto
                {
                    ProductId = 1,
                    ProductSku = "CAN-001",
                    ProductName = "Cannabis Flower - Indica",
                    UnitPriceInclVAT = 100m,
                    Quantity = 1,
                    BatchNumber = "BATCH-2025-001",
                    LineSubtotal = 86.96m,
                    LineVATAmount = 13.04m,
                    LineTotal = 100m
                },
                new CartItemDto
                {
                    ProductId = 2,
                    ProductSku = "CAN-002",
                    ProductName = "Cannabis Pre-Roll",
                    UnitPriceInclVAT = 100m,
                    Quantity = 1,
                    BatchNumber = "BATCH-2025-002",
                    LineSubtotal = 86.96m,
                    LineVATAmount = 13.04m,
                    LineTotal = 100m
                }
            }
        };
    }
}
