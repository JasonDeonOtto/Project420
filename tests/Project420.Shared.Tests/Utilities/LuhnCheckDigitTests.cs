using FluentAssertions;
using Project420.Shared.Database.Utilities;

namespace Project420.Shared.Tests.Utilities;

/// <summary>
/// Unit tests for LuhnCheckDigit utility (Phase 8).
/// Tests the Luhn algorithm implementation for serial number validation.
/// </summary>
public class LuhnCheckDigitTests
{
    // ============================================================
    // CALCULATE TESTS
    // ============================================================

    [Theory]
    [InlineData("7992739871", 3)]  // Standard test case
    [InlineData("123456789", 7)]   // Sequential digits
    [InlineData("0", 0)]           // Single zero
    [InlineData("1", 8)]           // Single digit
    [InlineData("00000", 0)]       // All zeros
    [InlineData("11111", 2)]       // All ones: (1*2=2)+1+(1*2=2)+1+(1*2=2)=8, check=(10-8)%10=2
    public void Calculate_Should_Return_Correct_CheckDigit(string number, int expectedCheckDigit)
    {
        // Act
        var result = LuhnCheckDigit.Calculate(number);

        // Assert
        result.Should().Be(expectedCheckDigit);
    }

    [Fact]
    public void Calculate_Should_Handle_Long_Numbers()
    {
        // Arrange - 30 digit number for full serial number
        var longNumber = "011001020251206000100001003551";

        // Act
        var result = LuhnCheckDigit.Calculate(longNumber);

        // Assert
        result.Should().BeInRange(0, 9);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Calculate_Should_Throw_For_Empty_Input(string number)
    {
        // Act
        var act = () => LuhnCheckDigit.Calculate(number);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("123ABC")]
    [InlineData("12.34")]
    [InlineData("12-34")]
    [InlineData("12 34")]
    public void Calculate_Should_Throw_For_NonNumeric_Input(string number)
    {
        // Act
        var act = () => LuhnCheckDigit.Calculate(number);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // APPEND CHECK DIGIT TESTS
    // ============================================================

    [Theory]
    [InlineData("7992739871", "79927398713")]
    [InlineData("123456789", "1234567897")]
    [InlineData("0", "00")]
    public void AppendCheckDigit_Should_Append_Correct_Digit(string input, string expected)
    {
        // Act
        var result = LuhnCheckDigit.AppendCheckDigit(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void AppendCheckDigit_Should_Increase_Length_By_One()
    {
        // Arrange
        var input = "123456789";

        // Act
        var result = LuhnCheckDigit.AppendCheckDigit(input);

        // Assert
        result.Length.Should().Be(input.Length + 1);
    }

    // ============================================================
    // VALIDATE TESTS
    // ============================================================

    [Theory]
    [InlineData("79927398713", true)]   // Valid with check digit
    [InlineData("1234567897", true)]    // Valid
    [InlineData("00", true)]            // Valid minimum
    [InlineData("79927398710", false)]  // Invalid (wrong check digit)
    [InlineData("1234567890", false)]   // Invalid
    public void Validate_Should_Return_Correct_Result(string numberWithCheck, bool expected)
    {
        // Act
        var result = LuhnCheckDigit.Validate(numberWithCheck);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]  // Only check digit, no original number
    public void Validate_Should_Return_False_For_Invalid_Input(string numberWithCheck)
    {
        // Act
        var result = LuhnCheckDigit.Validate(numberWithCheck);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("12.3")]
    public void Validate_Should_Return_False_For_NonNumeric_Input(string numberWithCheck)
    {
        // Act
        var result = LuhnCheckDigit.Validate(numberWithCheck);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Generated_Number()
    {
        // Arrange
        var original = "011001020251206000100001003551";
        var withCheck = LuhnCheckDigit.AppendCheckDigit(original);

        // Act
        var isValid = LuhnCheckDigit.Validate(withCheck);

        // Assert
        isValid.Should().BeTrue();
    }

    // ============================================================
    // EXTRACT CHECK DIGIT TESTS
    // ============================================================

    [Theory]
    [InlineData("79927398713", 3)]
    [InlineData("1234567897", 7)]
    [InlineData("00", 0)]
    [InlineData("99", 9)]
    public void ExtractCheckDigit_Should_Return_Last_Digit(string numberWithCheck, int expected)
    {
        // Act
        var result = LuhnCheckDigit.ExtractCheckDigit(numberWithCheck);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExtractCheckDigit_Should_Throw_For_Empty_Input(string numberWithCheck)
    {
        // Act
        var act = () => LuhnCheckDigit.ExtractCheckDigit(numberWithCheck);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractCheckDigit_Should_Throw_For_NonNumeric_LastChar()
    {
        // Act
        var act = () => LuhnCheckDigit.ExtractCheckDigit("123X");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // REMOVE CHECK DIGIT TESTS
    // ============================================================

    [Theory]
    [InlineData("79927398713", "7992739871")]
    [InlineData("1234567897", "123456789")]
    [InlineData("00", "0")]
    public void RemoveCheckDigit_Should_Return_Original_Number(string withCheck, string expected)
    {
        // Act
        var result = LuhnCheckDigit.RemoveCheckDigit(withCheck);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]  // Too short - only 1 character
    public void RemoveCheckDigit_Should_Throw_For_Short_Input(string numberWithCheck)
    {
        // Act
        var act = () => LuhnCheckDigit.RemoveCheckDigit(numberWithCheck);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // ROUND-TRIP TESTS
    // ============================================================

    [Theory]
    [InlineData("7992739871")]
    [InlineData("123456789")]
    [InlineData("000000000")]
    [InlineData("999999999")]
    [InlineData("011001020251206000100001003551")] // 30-digit serial number base
    public void RoundTrip_AppendThenValidate_Should_Work(string original)
    {
        // Act
        var withCheck = LuhnCheckDigit.AppendCheckDigit(original);
        var isValid = LuhnCheckDigit.Validate(withCheck);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("7992739871")]
    [InlineData("123456789")]
    [InlineData("011001020251206000100001003551")]
    public void RoundTrip_AppendThenRemove_Should_Return_Original(string original)
    {
        // Act
        var withCheck = LuhnCheckDigit.AppendCheckDigit(original);
        var removedCheck = LuhnCheckDigit.RemoveCheckDigit(withCheck);

        // Assert
        removedCheck.Should().Be(original);
    }

    // ============================================================
    // ERROR DETECTION TESTS
    // ============================================================

    [Fact]
    public void Validate_Should_Detect_SingleDigit_Transcription_Errors()
    {
        // Arrange
        var valid = LuhnCheckDigit.AppendCheckDigit("123456789");
        var invalid = "123456729"; // Changed 8 to 2 (single digit error) - need to append original check

        // The original with check is "1234567897"
        // Corrupting it to "1234567297" (changed position 7 from 8 to 2)
        var corrupted = "1234567297";

        // Act
        var isValid = LuhnCheckDigit.Validate(corrupted);

        // Assert
        isValid.Should().BeFalse("Luhn should detect single-digit transcription errors");
    }

    [Fact]
    public void Validate_Should_Detect_Adjacent_Transposition_Errors()
    {
        // Arrange
        var original = "123456789";
        var valid = LuhnCheckDigit.AppendCheckDigit(original); // "1234567897"

        // Transpose 78 to 87 in the original (positions 6-7)
        // Original: 123456789 -> Transposed: 123456879
        // But we need to include the check digit, so corrupt the validated number
        var transposed = "1234568797"; // 78 -> 87 transposition (but wrong check now)

        // Act
        var isValid = LuhnCheckDigit.Validate(transposed);

        // Assert
        isValid.Should().BeFalse("Luhn should detect most adjacent transposition errors");
    }

    // ============================================================
    // CANNABIS COMPLIANCE: SERIAL NUMBER SPECIFIC TESTS
    // ============================================================

    [Fact]
    public void Calculate_Should_Work_With_Serial_Number_Format()
    {
        // Arrange - Format: SS+SSS+TT+YYYYMMDD+BBBB+UUUUU+WWWW+Q (30 digits)
        // Site:01, Strain:100, Type:10, Date:20251212, Batch:0001, Unit:00001, Weight:0035, Pack:1
        var serialBase = "011001020251212000100001003501";

        // Act
        var checkDigit = LuhnCheckDigit.Calculate(serialBase);
        var fullSerial = LuhnCheckDigit.AppendCheckDigit(serialBase);
        var isValid = LuhnCheckDigit.Validate(fullSerial);

        // Assert
        checkDigit.Should().BeInRange(0, 9);
        fullSerial.Length.Should().Be(31);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Work_With_Different_Site_IDs()
    {
        // Test various site IDs (01-99) maintain validity
        var siteIds = new[] { "01", "05", "10", "50", "99" };

        foreach (var siteId in siteIds)
        {
            // Arrange
            var serialBase = $"{siteId}1001020251212000100001003501";
            var fullSerial = LuhnCheckDigit.AppendCheckDigit(serialBase);

            // Act
            var isValid = LuhnCheckDigit.Validate(fullSerial);

            // Assert
            isValid.Should().BeTrue($"Serial with site ID {siteId} should be valid");
        }
    }

    [Fact]
    public void Validate_Should_Work_With_Different_Strain_Codes()
    {
        // Test various strain codes (100-999)
        var strainCodes = new[] { "100", "200", "300", "400", "500", "999" };

        foreach (var strainCode in strainCodes)
        {
            // Arrange
            var serialBase = $"01{strainCode}1020251212000100001003501";
            var fullSerial = LuhnCheckDigit.AppendCheckDigit(serialBase);

            // Act
            var isValid = LuhnCheckDigit.Validate(fullSerial);

            // Assert
            isValid.Should().BeTrue($"Serial with strain code {strainCode} should be valid");
        }
    }
}
