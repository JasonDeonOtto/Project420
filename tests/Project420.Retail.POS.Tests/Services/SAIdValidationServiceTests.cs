using FluentAssertions;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.BLL.Services;
using Xunit;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Unit tests for SAIdValidationService
/// Phase 9.7: Age Verification Enhancement
/// </summary>
/// <remarks>
/// SA ID Format: YYMMDDGSSSCAZ (13 digits)
/// - YYMMDD: Date of birth
/// - G: Gender (0-4 = female, 5-9 = male)
/// - SSS: Sequence
/// - C: Citizenship (0 = SA, 1 = PR)
/// - A: Usually 8
/// - Z: Luhn check digit
/// </remarks>
public class SAIdValidationServiceTests
{
    private readonly SAIdValidationService _service;

    public SAIdValidationServiceTests()
    {
        _service = new SAIdValidationService();
    }

    // ========================================
    // VALID ID TESTS
    // ========================================

    [Theory]
    [InlineData("8501015009086")] // Valid male, 1985-01-01, SA Citizen (Luhn verified)
    public void ValidateIdNumber_ValidId_ReturnsValid(string idNumber)
    {
        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateIdNumber_ValidId_ExtractsCorrectDob()
    {
        // Arrange - 1985-01-01 (with valid check digit)
        var idNumber = "8501015009086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DateOfBirth.Should().NotBeNull();
        result.DateOfBirth!.Value.Year.Should().Be(1985);
        result.DateOfBirth!.Value.Month.Should().Be(1);
        result.DateOfBirth!.Value.Day.Should().Be(1);
    }

    [Fact]
    public void ValidateIdNumber_ValidId_CalculatesCorrectAge()
    {
        // Arrange - Person born 1985-01-01 (about 40 years old in 2025)
        var idNumber = "8501015009086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Age.Should().NotBeNull();
        result.Age.Should().BeGreaterThanOrEqualTo(39); // Born in 1985
        result.Age.Should().BeLessThanOrEqualTo(40);
    }

    [Fact]
    public void ValidateIdNumber_AdultId_ReturnsIsOver18True()
    {
        // Arrange - Adult (40 years old)
        var idNumber = "8501015009086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsOver18.Should().BeTrue();
    }

    [Fact]
    public void ValidateIdNumber_MaleId_ExtractsGenderMale()
    {
        // Arrange - Gender digit 5 = male (valid Luhn)
        var idNumber = "8501015009086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Gender.Should().Be("Male");
    }

    [Fact]
    public void ValidateIdNumber_FemaleId_ExtractsGenderFemale()
    {
        // Arrange - Test ExtractGender directly (doesn't need valid Luhn)
        var gender = _service.ExtractGender("8501010009080"); // Gender digit 0

        // Assert
        gender.Should().Be("Female");
    }

    [Fact]
    public void ValidateIdNumber_SACitizen_ExtractsCitizenshipCorrectly()
    {
        // Arrange - Citizenship digit 0 = SA Citizen
        var idNumber = "8501015009086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Citizenship.Should().Be("SA Citizen");
    }

    // ========================================
    // INVALID ID TESTS
    // ========================================

    [Fact]
    public void ValidateIdNumber_NullInput_ReturnsInvalid()
    {
        // Act
        var result = _service.ValidateIdNumber(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void ValidateIdNumber_EmptyInput_ReturnsInvalid()
    {
        // Act
        var result = _service.ValidateIdNumber("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void ValidateIdNumber_WrongLength_ReturnsInvalid()
    {
        // Act
        var result = _service.ValidateIdNumber("123456789012"); // 12 digits

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("13 digits"));
    }

    [Fact]
    public void ValidateIdNumber_NonNumeric_ReturnsInvalid()
    {
        // Act
        var result = _service.ValidateIdNumber("850101500908A");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("only digits"));
    }

    [Fact]
    public void ValidateIdNumber_InvalidMonth_ReturnsInvalid()
    {
        // Arrange - Month 13 is invalid
        var idNumber = "8513015009080"; // Recalculated check digit

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid month"));
    }

    [Fact]
    public void ValidateIdNumber_InvalidDay_ReturnsInvalid()
    {
        // Arrange - Day 32 is invalid
        var idNumber = "8501325009080"; // Recalculated check digit

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid day"));
    }

    [Fact]
    public void ValidateIdNumber_InvalidCheckDigit_ReturnsInvalid()
    {
        // Arrange - Wrong check digit (should be 2, using 9)
        var idNumber = "8501015009089";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("check digit"));
    }

    [Fact]
    public void ValidateIdNumber_WithSpaces_StillValidates()
    {
        // Arrange - ID with spaces (common when typed)
        var idNumber = "8501 0150 0908 6";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateIdNumber_WithDashes_StillValidates()
    {
        // Arrange - ID with dashes
        var idNumber = "850101-5009-086";

        // Act
        var result = _service.ValidateIdNumber(idNumber);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ========================================
    // EXTRACT DATE OF BIRTH TESTS
    // ========================================

    [Fact]
    public void ExtractDateOfBirth_ValidId_ReturnsCorrectDate()
    {
        // Act
        var dob = _service.ExtractDateOfBirth("8501015009086");

        // Assert
        dob.Should().NotBeNull();
        dob!.Value.Should().Be(new DateTime(1985, 1, 1));
    }

    [Fact]
    public void ExtractDateOfBirth_InvalidId_ReturnsNull()
    {
        // Act
        var dob = _service.ExtractDateOfBirth("invalid");

        // Assert
        dob.Should().BeNull();
    }

    [Fact]
    public void ExtractDateOfBirth_2000sId_ReturnsCorrectCentury()
    {
        // Arrange - Someone born in 2005 would have ID starting with 05
        // In 2025, they would be 20 years old
        // For this test, we calculate age to confirm century detection
        // Note: Uses ExtractDateOfBirth which doesn't validate Luhn
        var dob = _service.ExtractDateOfBirth("0501015009087");

        // Assert - Should be 2005, not 1905!
        dob.Should().NotBeNull();
        dob!.Value.Year.Should().Be(2005);
    }

    // ========================================
    // CALCULATE AGE TESTS
    // ========================================

    [Fact]
    public void CalculateAge_ValidId_ReturnsAge()
    {
        // Act
        var age = _service.CalculateAge("8501015009086");

        // Assert
        age.Should().NotBeNull();
        age.Should().BePositive();
    }

    [Fact]
    public void CalculateAge_InvalidId_ReturnsNull()
    {
        // Act
        var age = _service.CalculateAge("invalid");

        // Assert
        age.Should().BeNull();
    }

    // ========================================
    // IS OVER 18 TESTS
    // ========================================

    [Fact]
    public void IsOver18_AdultId_ReturnsTrue()
    {
        // Act
        var isOver18 = _service.IsOver18("8501015009086");

        // Assert
        isOver18.Should().BeTrue();
    }

    [Fact]
    public void IsOver18_InvalidId_ReturnsNull()
    {
        // Act
        var isOver18 = _service.IsOver18("invalid");

        // Assert
        isOver18.Should().BeNull();
    }

    // ========================================
    // MANUAL DOB VALIDATION TESTS
    // ========================================

    [Fact]
    public void ValidateManualDob_Adult_ReturnsVerified()
    {
        // Arrange
        var dob = new DateTime(1985, 1, 1);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeTrue();
        result.IsOver18.Should().BeTrue();
        result.Age.Should().BeGreaterThanOrEqualTo(39);
    }

    [Fact]
    public void ValidateManualDob_Minor_ReturnsNotOver18()
    {
        // Arrange - 10 years old
        var dob = DateTime.Today.AddYears(-10);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeTrue();
        result.IsOver18.Should().BeFalse();
        result.Age.Should().Be(10);
        result.ErrorMessage.Should().Contain("18+");
    }

    [Fact]
    public void ValidateManualDob_FutureDate_ReturnsError()
    {
        // Arrange
        var dob = DateTime.Today.AddDays(1);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeFalse();
        result.ErrorMessage.Should().Contain("future");
    }

    [Fact]
    public void ValidateManualDob_TooOld_ReturnsError()
    {
        // Arrange - 130 years old
        var dob = DateTime.Today.AddYears(-130);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeFalse();
        result.ErrorMessage.Should().Contain("120");
    }

    [Fact]
    public void ValidateManualDob_Exactly18Today_ReturnsOver18()
    {
        // Arrange - Exactly 18 today
        var dob = DateTime.Today.AddYears(-18);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeTrue();
        result.IsOver18.Should().BeTrue();
        result.Age.Should().Be(18);
    }

    [Fact]
    public void ValidateManualDob_17YearsOld_ReturnsNotOver18()
    {
        // Arrange - Just under 18
        var dob = DateTime.Today.AddYears(-18).AddDays(1);

        // Act
        var result = _service.ValidateManualDob(dob);

        // Assert
        result.IsVerified.Should().BeTrue();
        result.IsOver18.Should().BeFalse();
        result.Age.Should().Be(17);
    }

    // ========================================
    // MASK ID NUMBER TESTS
    // ========================================

    [Fact]
    public void MaskIdNumber_ValidId_MasksCorrectly()
    {
        // Act
        var masked = SAIdValidationService.MaskIdNumber("8501015009086");

        // Assert
        masked.Should().Be("850101*****86");
    }

    [Fact]
    public void MaskIdNumber_InvalidId_ReturnsInvalidMessage()
    {
        // Act
        var masked = SAIdValidationService.MaskIdNumber("invalid");

        // Assert
        masked.Should().Be("Invalid ID");
    }

    [Fact]
    public void MaskIdNumber_NullId_ReturnsInvalidMessage()
    {
        // Act
        var masked = SAIdValidationService.MaskIdNumber(null!);

        // Assert
        masked.Should().Be("Invalid ID");
    }

    // ========================================
    // EXTRACT GENDER TESTS
    // ========================================

    [Theory]
    [InlineData("8501010009080", "Female")] // Gender digit 0
    [InlineData("8501011009085", "Female")] // Gender digit 1
    [InlineData("8501012009080", "Female")] // Gender digit 2
    [InlineData("8501013009085", "Female")] // Gender digit 3
    [InlineData("8501014009080", "Female")] // Gender digit 4
    [InlineData("8501015009087", "Male")]   // Gender digit 5
    [InlineData("8501016009082", "Male")]   // Gender digit 6
    [InlineData("8501017009087", "Male")]   // Gender digit 7
    [InlineData("8501018009082", "Male")]   // Gender digit 8
    [InlineData("8501019009087", "Male")]   // Gender digit 9
    public void ExtractGender_ValidDigit_ReturnsCorrectGender(string idNumber, string expectedGender)
    {
        // Act
        var gender = _service.ExtractGender(idNumber);

        // Assert
        gender.Should().Be(expectedGender);
    }

    // ========================================
    // EXTRACT CITIZENSHIP TESTS
    // ========================================

    [Fact]
    public void ExtractCitizenship_SACitizen_ReturnsCorrect()
    {
        // Arrange - Citizenship digit 0
        var citizenship = _service.ExtractCitizenship("8501015009086");

        // Assert
        citizenship.Should().Be("SA Citizen");
    }

    [Fact]
    public void ExtractCitizenship_PermanentResident_ReturnsCorrect()
    {
        // Arrange - Citizenship digit 1 (doesn't need valid Luhn for extract)
        var citizenship = _service.ExtractCitizenship("8501015009180");

        // Assert
        citizenship.Should().Be("Permanent Resident");
    }
}
