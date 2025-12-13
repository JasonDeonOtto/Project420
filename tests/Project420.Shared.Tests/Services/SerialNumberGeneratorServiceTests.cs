using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;
using Project420.Shared.Database.Utilities;

namespace Project420.Shared.Tests.Services;

/// <summary>
/// Unit tests for SerialNumberGeneratorService (Phase 8).
/// Tests serial number generation, validation, and parsing.
/// Full SN: 31 digits (30 + Luhn check)
/// Short SN: 13 digits
/// </summary>
/// <remarks>
/// Uses TestBusinessDbContext which implements IBusinessDbContext for testing.
/// This allows testing without requiring the full PosDbContext.
/// </remarks>
public class SerialNumberGeneratorServiceTests : IDisposable
{
    private readonly TestBusinessDbContext _context;
    private readonly Mock<ILogger<SerialNumberGeneratorService>> _loggerMock;
    private readonly SerialNumberGeneratorService _service;
    private const string TestUser = "TestUser";

    public SerialNumberGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestBusinessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestBusinessDbContext(options);
        _loggerMock = new Mock<ILogger<SerialNumberGeneratorService>>();
        _service = new SerialNumberGeneratorService(_context, _loggerMock.Object, TestUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // FULL SERIAL NUMBER GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Generate_30_Digit_Full_SN()
    {
        // Arrange
        var siteId = 1;
        var strainCode = 100; // Sativa
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);
        var batchSequence = 1;
        var weight = 3.5m;
        var packQty = 1;

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, batchSequence, weight, packQty, TestUser);

        // Assert
        // Format: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1) = 30 digits
        result.FullSerialNumber.Should().NotBeNullOrEmpty();
        result.FullSerialNumber.Length.Should().Be(30);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Generate_13_Digit_Short_SN()
    {
        // Arrange
        var siteId = 1;
        var strainCode = 100;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, 1, 3.5m, 1, TestUser);

        // Assert
        result.ShortSerialNumber.Should().NotBeNullOrEmpty();
        result.ShortSerialNumber.Length.Should().Be(13);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Include_Valid_Luhn_Check_Digit()
    {
        // Arrange
        var siteId = 1;
        var strainCode = 100;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, 1, 3.5m, 1, TestUser);

        // Assert
        var isValid = LuhnCheckDigit.Validate(result.FullSerialNumber);
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_Site_Correctly()
    {
        // Arrange
        var siteId = 5;

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        // First 2 digits are Site ID
        result.FullSerialNumber.Substring(0, 2).Should().Be("05");
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_StrainCode_Correctly()
    {
        // Arrange
        var strainCode = 150; // Sativa (100-199)

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, strainCode, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        // Strain code is positions 3-5 (index 2-4)
        result.FullSerialNumber.Substring(2, 3).Should().Be("150");
        result.StrainCode.Should().Be(150);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_BatchType_Correctly()
    {
        // Arrange
        var batchType = BatchType.Transfer; // 20

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, batchType, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        // Batch type is positions 6-7 (index 5-6)
        result.FullSerialNumber.Substring(5, 2).Should().Be("20");
        result.BatchType.Should().Be(BatchType.Transfer);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_Date_Correctly()
    {
        // Arrange
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, date, 1, 3.5m, 1, TestUser);

        // Assert
        // Date is positions 8-15 (index 7-14) YYYYMMDD
        result.FullSerialNumber.Substring(7, 8).Should().Be("20251212");
        result.ProductionDate.Should().Be(date);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_Weight_In_Tenths()
    {
        // Arrange
        var weight = 3.5m; // Should encode as 0035 (35 tenths of gram)

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, weight, 1, TestUser);

        // Assert
        // Format: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1)
        // Weight starts at position 24 (index 24-27) WWWW
        result.FullSerialNumber.Substring(24, 4).Should().Be("0035");
        result.WeightGrams.Should().Be(3.5m);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Encode_PackQty_Correctly()
    {
        // Arrange
        var packQty = 5;

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, packQty, TestUser);

        // Assert
        // Format: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1)
        // Pack qty is at position 28 (index 28) Q
        result.FullSerialNumber[28].Should().Be('5');
        result.PackQty.Should().Be(5);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Increment_Unit_Sequence()
    {
        // Arrange
        var siteId = 1;
        var strainCode = 100;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);
        var batchSequence = 1;

        // Act
        var result1 = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, batchSequence, 3.5m, 1, TestUser);
        var result2 = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, batchSequence, 3.5m, 1, TestUser);
        var result3 = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, batchType, date, batchSequence, 3.5m, 1, TestUser);

        // Assert
        result1.UnitSequence.Should().Be(1);
        result2.UnitSequence.Should().Be(2);
        result3.UnitSequence.Should().Be(3);
    }

    // ============================================================
    // SHORT SERIAL NUMBER TESTS
    // ============================================================

    [Fact]
    public async Task GenerateSerialNumberAsync_ShortSN_Should_Include_Site()
    {
        // Arrange
        var siteId = 5;

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        // Short SN format: SSYYMMDDNNNNN
        result.ShortSerialNumber.Substring(0, 2).Should().Be("05");
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_ShortSN_Should_Include_ShortDate()
    {
        // Arrange
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, date, 1, 3.5m, 1, TestUser);

        // Assert
        // Short SN format: SSYYMMDDNNNNN - date is YYMMDD
        result.ShortSerialNumber.Substring(2, 6).Should().Be("251212");
    }

    // ============================================================
    // BULK GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Generate_Correct_Count()
    {
        // Arrange
        var count = 5;

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Generate_Unique_Numbers()
    {
        // Arrange
        var count = 10;

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        var fullSNs = results.Select(r => r.FullSerialNumber).ToList();
        fullSNs.Distinct().Count().Should().Be(count);

        var shortSNs = results.Select(r => r.ShortSerialNumber).ToList();
        shortSNs.Distinct().Count().Should().Be(count);
    }

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Have_Sequential_UnitSequences()
    {
        // Arrange
        var count = 5;

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        var sequences = results.Select(r => r.UnitSequence).ToList();
        sequences.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    // ============================================================
    // VALIDATION TESTS
    // ============================================================

    [Fact]
    public async Task ValidateFullSerialNumber_Should_Return_True_For_Valid()
    {
        // Arrange
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Act
        var isValid = _service.ValidateFullSerialNumber(result.FullSerialNumber);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")] // Too short
    [InlineData("12345678901234567890123456789012345")] // Too long (35)
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234")] // Non-numeric (30 chars)
    public void ValidateFullSerialNumber_Should_Return_False_For_Invalid(string serialNumber)
    {
        // Act
        var isValid = _service.ValidateFullSerialNumber(serialNumber);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateFullSerialNumber_Should_Return_False_For_Invalid_Luhn()
    {
        // Arrange - Create a 30-digit number but with wrong check digit
        // Valid base (29 digits): 01100102025121200010000100351
        // Correct check digit would be calculated from this base
        // We'll use a wrong check digit (0 instead of the correct one)
        var baseSN = "01100102025121200010000100351";
        var correctCheck = LuhnCheckDigit.Calculate(baseSN);
        var wrongCheck = (correctCheck + 1) % 10; // Use wrong check digit
        var invalidSerial = baseSN + wrongCheck.ToString();

        // Act
        var isValid = _service.ValidateFullSerialNumber(invalidSerial);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateShortSerialNumber_Should_Return_True_For_Valid()
    {
        // Arrange
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Act
        var isValid = _service.ValidateShortSerialNumber(result.ShortSerialNumber);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")] // Too short
    [InlineData("12345678901234")] // Too long (14)
    [InlineData("ABCDEFGHIJKLM")] // Non-numeric
    public void ValidateShortSerialNumber_Should_Return_False_For_Invalid(string serialNumber)
    {
        // Act
        var isValid = _service.ValidateShortSerialNumber(serialNumber);

        // Assert
        isValid.Should().BeFalse();
    }

    // ============================================================
    // PARSING TESTS
    // ============================================================

    [Fact]
    public async Task ParseFullSerialNumber_Should_Extract_All_Components()
    {
        // Arrange
        var date = new DateTime(2025, 12, 12);
        var result = await _service.GenerateSerialNumberAsync(
            1, 150, BatchType.Transfer, date, 5, 7.5m, 2, TestUser);

        // Act
        var parsed = _service.ParseFullSerialNumber(result.FullSerialNumber);

        // Assert
        parsed.SiteId.Should().Be(1);
        parsed.StrainCode.Should().Be(150);
        parsed.BatchType.Should().Be(BatchType.Transfer);
        parsed.ProductionDate.Should().Be(date);
        parsed.BatchSequence.Should().Be(5);
        parsed.WeightGrams.Should().Be(7.5m);
        parsed.PackQty.Should().Be(2);
        parsed.IsCheckDigitValid.Should().BeTrue();
    }

    [Fact]
    public async Task ParseShortSerialNumber_Should_Extract_Components()
    {
        // Arrange
        var date = new DateTime(2025, 12, 12);
        var result = await _service.GenerateSerialNumberAsync(
            5, 100, BatchType.Production, date, 1, 3.5m, 1, TestUser);

        // Act
        var parsed = _service.ParseShortSerialNumber(result.ShortSerialNumber);

        // Assert
        parsed.SiteId.Should().Be(5);
        parsed.ProductionDate.Should().Be(date);
        parsed.Sequence.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void ParseFullSerialNumber_Should_Throw_For_Invalid_Input(string serialNumber)
    {
        // Act
        var act = () => _service.ParseFullSerialNumber(serialNumber);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // STRAIN TYPE TESTS
    // ============================================================

    [Theory]
    [InlineData(100, "Sativa")]
    [InlineData(150, "Sativa")]
    [InlineData(199, "Sativa")]
    [InlineData(200, "Indica")]
    [InlineData(250, "Indica")]
    [InlineData(299, "Indica")]
    [InlineData(300, "Hybrid")]
    [InlineData(350, "Hybrid")]
    [InlineData(399, "Hybrid")]
    [InlineData(400, "CBD")]
    [InlineData(450, "CBD")]
    [InlineData(499, "CBD")]
    [InlineData(500, "Unknown")]
    [InlineData(999, "Unknown")]
    public void GetStrainType_Should_Return_Correct_Type(int strainCode, string expectedType)
    {
        // Act
        var result = _service.GetStrainType(strainCode);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Include_StrainType_In_Result()
    {
        // Arrange
        var strainCode = 250; // Indica

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, strainCode, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        result.StrainType.Should().Be("Indica");
    }

    // ============================================================
    // EDGE CASES & ERROR HANDLING
    // ============================================================

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task GenerateSerialNumberAsync_Should_Throw_For_Invalid_SiteId(int siteId)
    {
        // Act
        var act = async () => await _service.GenerateSerialNumberAsync(
            siteId, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(1000)]
    [InlineData(-1)]
    public async Task GenerateSerialNumberAsync_Should_Throw_For_Invalid_StrainCode(int strainCode)
    {
        // Act
        var act = async () => await _service.GenerateSerialNumberAsync(
            1, strainCode, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    public async Task GenerateSerialNumberAsync_Should_Throw_For_Invalid_PackQty(int packQty)
    {
        // Act
        var act = async () => await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, packQty, TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Persist_SerialNumber_Record()
    {
        // Arrange
        var siteId = 1;
        var strainCode = 100;
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, strainCode, BatchType.Production, date, 1, 3.5m, 1, TestUser);

        // Assert
        var serialNumbers = await _context.SerialNumbers.ToListAsync();
        serialNumbers.Should().HaveCount(1);
        serialNumbers[0].FullSerialNumber.Should().Be(result.FullSerialNumber);
        serialNumbers[0].ShortSerialNumber.Should().Be(result.ShortSerialNumber);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Set_Audit_Fields()
    {
        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, 100, BatchType.Production, DateTime.Today, 1, 3.5m, 1, TestUser);

        // Assert
        var serial = await _context.SerialNumbers.FirstAsync();
        serial.CreatedBy.Should().Be(TestUser);
        serial.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
