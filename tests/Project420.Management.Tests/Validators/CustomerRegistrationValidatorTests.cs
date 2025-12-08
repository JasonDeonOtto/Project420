using FluentAssertions;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.BLL.Sales.SalesCommon.Validators;

namespace Project420.Management.Tests.Validators;

/// <summary>
/// Tests for CustomerRegistrationValidator.
/// Ensures Cannabis Act and POPIA compliance validation works correctly.
/// </summary>
public class CustomerRegistrationValidatorTests
{
    private readonly CustomerRegistrationValidator _validator;

    public CustomerRegistrationValidatorTests()
    {
        _validator = new CustomerRegistrationValidator();
    }

    #region Age Verification Tests (Cannabis Act Compliance)

    [Fact]
    public async Task Validate_Adult18Plus_PassesValidation()
    {
        // Arrange: Valid SA ID for 25-year-old (born 2000-01-01)
        var dto = CreateValidDto();
        dto.IdNumber = "0001015800080"; // 2000-01-01, Male, Valid SA ID

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_Under18_FailsValidation()
    {
        // Arrange: SA ID for 10-year-old (born 2015-01-01) - Under legal age!
        var dto = CreateValidDto();
        dto.IdNumber = "1501015800087"; // 2015-01-01

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "IdNumber" &&
            e.ErrorMessage.Contains("18"));
    }

    [Fact]
    public async Task Validate_InvalidSAIdFormat_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.IdNumber = "1234567890123"; // Invalid date format

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "IdNumber" &&
            e.ErrorMessage.Contains("Invalid"));
    }

    #endregion

    #region Personal Information Tests

    [Fact]
    public async Task Validate_EmptyName_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Name = "";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_NameWithNumbers_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Name = "John123"; // Invalid: contains numbers

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("letters"));
    }

    [Fact]
    public async Task Validate_ValidNameWithHyphen_PassesValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Name = "Mary-Jane Smith"; // Valid: hyphens allowed

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Contact Information Tests

    [Fact]
    public async Task Validate_InvalidMobileFormat_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Mobile = "1234567890"; // Invalid: doesn't start with 0

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Mobile" &&
            e.ErrorMessage.Contains("0XXXXXXXXX"));
    }

    [Fact]
    public async Task Validate_ValidMobile_PassesValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Mobile = "0821234567"; // Valid SA mobile format

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_InvalidEmail_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Email = "not-an-email";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    #endregion

    #region Medical Cannabis Tests (Section 21)

    [Fact]
    public async Task Validate_MedicalPermitWithoutExpiryDate_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MedicalPermitNumber = "MED2025001"; // Has permit
        dto.MedicalPermitExpiryDate = null; // Missing expiry
        dto.PrescribingDoctor = "Dr. Smith";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "MedicalPermitExpiryDate" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_ExpiredMedicalPermit_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MedicalPermitNumber = "MED2025001";
        dto.MedicalPermitExpiryDate = DateTime.Today.AddDays(-1); // Expired yesterday
        dto.PrescribingDoctor = "Dr. Smith";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "MedicalPermitExpiryDate" &&
            e.ErrorMessage.Contains("expired"));
    }

    [Fact]
    public async Task Validate_ValidMedicalPermit_PassesValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MedicalPermitNumber = "MED2025001";
        dto.MedicalPermitExpiryDate = DateTime.Today.AddMonths(6); // Valid for 6 months
        dto.PrescribingDoctor = "Dr. Jane Smith";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region POPIA Compliance Tests

    [Fact]
    public async Task Validate_NoConsentGiven_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.ConsentGiven = false; // POPIA violation!

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ConsentGiven" &&
            e.ErrorMessage.Contains("POPIA"));
    }

    [Fact]
    public async Task Validate_ConsentWithoutPurpose_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.ConsentGiven = true;
        dto.ConsentPurpose = ""; // Missing purpose

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ConsentPurpose" &&
            e.ErrorMessage.Contains("must be specified"));
    }

    #endregion

    #region Credit Management Tests

    [Fact]
    public async Task Validate_NegativeCreditLimit_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.CreditLimit = -100; // Invalid

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreditLimit");
    }

    [Fact]
    public async Task Validate_ExcessiveCreditLimit_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.CreditLimit = 2_000_000; // Over R1 million limit

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CreditLimit" &&
            e.ErrorMessage.Contains("1,000,000"));
    }

    [Fact]
    public async Task Validate_CreditLimitWithoutPaymentTerms_FailsValidation()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.CreditLimit = 5000; // Has credit
        dto.PaymentTerms = 0; // No payment terms

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PaymentTerms" &&
            e.ErrorMessage.Contains("required for credit customers"));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a valid CustomerRegistrationDto for testing.
    /// </summary>
    private CustomerRegistrationDto CreateValidDto()
    {
        return new CustomerRegistrationDto
        {
            Name = "John Doe",
            IdNumber = "8001011234080", // 1980-01-01, 45 years old, valid SA ID
            Mobile = "0821234567",
            Email = "john.doe@example.com",
            PhysicalAddress = "123 Main Street, Cape Town",
            PostalAddress = "PO Box 123, Cape Town, 8000",
            ConsentGiven = true,
            ConsentPurpose = "Account creation and transaction processing",
            CreditLimit = 0, // No credit (cash customer)
            PaymentTerms = 0,
            Notes = "Test customer"
        };
    }

    #endregion
}
