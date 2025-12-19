using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Shared.Tests.Services;

/// <summary>
/// Unit tests for SerialNumberGeneratorService.
/// Tests serial number generation, validation, and parsing.
/// Format: TTYYYWWBBBBSSSSSS (16 digits, batch-linked)
/// </summary>
/// <remarks>
/// Uses TestBusinessDbContext which implements IBusinessDbContext for testing.
/// The serial number service depends on BatchNumberGeneratorService for batch validation.
/// </remarks>
public class SerialNumberGeneratorServiceTests : IDisposable
{
    private readonly TestBusinessDbContext _context;
    private readonly Mock<ILogger<SerialNumberGeneratorService>> _snLoggerMock;
    private readonly Mock<ILogger<BatchNumberGeneratorService>> _batchLoggerMock;
    private readonly BatchNumberGeneratorService _batchService;
    private readonly SerialNumberGeneratorService _service;
    private const string TestUser = "TestUser";

    public SerialNumberGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestBusinessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestBusinessDbContext(options);
        _snLoggerMock = new Mock<ILogger<SerialNumberGeneratorService>>();
        _batchLoggerMock = new Mock<ILogger<BatchNumberGeneratorService>>();

        _batchService = new BatchNumberGeneratorService(_context, _batchLoggerMock.Object, TestUser);
        _service = new SerialNumberGeneratorService(_context, _snLoggerMock.Object, _batchService, TestUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // SERIAL NUMBER GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Generate_16_Digit_Number()
    {
        // Arrange
        var siteId = 1;
        var serialType = SerialType.Production;
        var date = new DateTime(2025, 12, 15); // Week 51
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            siteId, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, serialType, batchNumber, TestUser);

        // Assert
        result.SerialNumber.Should().NotBeNullOrEmpty();
        result.SerialNumber.Length.Should().Be(16);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Format_Correctly()
    {
        // Arrange
        var siteId = 1;
        var serialType = SerialType.Production; // 10
        var date = new DateTime(2025, 12, 15); // Week 51
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            siteId, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, serialType, batchNumber, TestUser);

        // Assert
        // Format: TTYYYWWBBBBSSSSSS
        // TT = 10 (Production), YY = 25, WW = 51, BBBB = 0001, SSSSSS = 000001
        result.SerialNumber.Should().Be("1025510001000001");
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Increment_Sequence()
    {
        // Arrange
        var siteId = 1;
        var serialType = SerialType.Production;
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            siteId, BatchType.Production, date, TestUser);

        // Act
        var result1 = await _service.GenerateSerialNumberAsync(siteId, serialType, batchNumber, TestUser);
        var result2 = await _service.GenerateSerialNumberAsync(siteId, serialType, batchNumber, TestUser);
        var result3 = await _service.GenerateSerialNumberAsync(siteId, serialType, batchNumber, TestUser);

        // Assert
        result1.SerialNumber.Should().EndWith("000001");
        result2.SerialNumber.Should().EndWith("000002");
        result3.SerialNumber.Should().EndWith("000003");
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Extract_YearWeek_From_Batch()
    {
        // Arrange
        var siteId = 1;
        var date = new DateTime(2025, 12, 15); // Week 51
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            siteId, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, SerialType.Retail, batchNumber, TestUser);

        // Assert
        // Serial should have same YYWW as the batch
        result.SerialNumber.Substring(2, 4).Should().Be("2551"); // YY=25, WW=51
        result.Year.Should().Be(25);
        result.Week.Should().Be(51);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Extract_BatchSequence_From_Batch()
    {
        // Arrange
        var siteId = 1;
        var date = new DateTime(2025, 12, 15);

        // Generate 3 batches to get batch sequence 3
        await _batchService.GenerateBatchNumberAsync(siteId, BatchType.Production, date, TestUser);
        await _batchService.GenerateBatchNumberAsync(siteId, BatchType.Production, date, TestUser);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            siteId, BatchType.Production, date, TestUser); // Seq 0003

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            siteId, SerialType.Production, batchNumber, TestUser);

        // Assert
        result.SerialNumber.Substring(6, 4).Should().Be("0003"); // BBBB from batch
        result.BatchSequence.Should().Be(3);
    }

    [Theory]
    [InlineData(SerialType.Production, "10")]
    [InlineData(SerialType.GRV, "20")]
    [InlineData(SerialType.Retail, "30")]
    [InlineData(SerialType.Bucking, "40")]
    [InlineData(SerialType.Transfer, "50")]
    [InlineData(SerialType.Adjustment, "60")]
    [InlineData(SerialType.Packaging, "70")]
    [InlineData(SerialType.QCSample, "80")]
    [InlineData(SerialType.Destruction, "90")]
    public async Task GenerateSerialNumberAsync_Should_Use_Correct_Type_Codes(
        SerialType serialType, string expectedCode)
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(1, serialType, batchNumber, TestUser);

        // Assert
        result.SerialNumber.Substring(0, 2).Should().Be(expectedCode);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Store_ParentBatchNumber()
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, SerialType.Production, batchNumber, TestUser);

        // Assert
        result.ParentBatchNumber.Should().Be(batchNumber);
    }

    // ============================================================
    // BULK GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Generate_Correct_Count()
    {
        // Arrange
        var count = 5;
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, SerialType.Production, batchNumber, TestUser);

        // Assert
        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Generate_Unique_Numbers()
    {
        // Arrange
        var count = 10;
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, SerialType.Production, batchNumber, TestUser);

        // Assert
        var serialNumbers = results.Select(r => r.SerialNumber).ToList();
        serialNumbers.Distinct().Count().Should().Be(count);
    }

    [Fact]
    public async Task GenerateBulkSerialNumbersAsync_Should_Have_Sequential_Sequences()
    {
        // Arrange
        var count = 5;
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var results = await _service.GenerateBulkSerialNumbersAsync(
            count, 1, SerialType.Production, batchNumber, TestUser);

        // Assert
        var sequences = results.Select(r => r.Sequence).ToList();
        sequences.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    // ============================================================
    // VALIDATION TESTS
    // ============================================================

    [Theory]
    [InlineData("1025510001000001", true)]  // Valid: Production, 2025-W51, Batch 1, Seq 1
    [InlineData("3025510001999999", true)]  // Valid: Retail, 2025-W51, Batch 1, Seq 999999
    [InlineData("9020010001000001", true)]  // Valid: Destruction, 2020-W01, Batch 1, Seq 1
    public void ValidateSerialNumber_Should_Return_True_For_Valid_Numbers(string serialNumber, bool expected)
    {
        // Act
        var result = _service.ValidateSerialNumber(serialNumber);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]                        // Empty
    [InlineData(null)]                      // Null
    [InlineData("10255100010000")]          // Too short (14 chars)
    [InlineData("102551000100000123")]      // Too long (18 chars)
    [InlineData("ABCD510001000001")]        // Non-numeric
    [InlineData("9925510001000001")]        // Invalid serial type 99
    [InlineData("1025540001000001")]        // Invalid week 54
    [InlineData("1019510001000001")]        // Invalid year 19 (before 2020)
    public void ValidateSerialNumber_Should_Return_False_For_Invalid_Numbers(string serialNumber)
    {
        // Act
        var result = _service.ValidateSerialNumber(serialNumber);

        // Assert
        result.Should().BeFalse();
    }

    // ============================================================
    // PARSING TESTS
    // ============================================================

    [Fact]
    public void ParseSerialNumber_Should_Extract_All_Components()
    {
        // Arrange
        var serialNumber = "3025510003000042";

        // Act
        var components = _service.ParseSerialNumber(serialNumber);

        // Assert
        components.SerialType.Should().Be(SerialType.Retail);
        components.Year.Should().Be(25);
        components.Week.Should().Be(51);
        components.BatchSequence.Should().Be(3);
        components.Sequence.Should().Be(42);
    }

    [Fact]
    public void ParseSerialNumber_Should_Handle_Max_Values()
    {
        // Arrange
        var serialNumber = "9099539999999999";

        // Act
        var components = _service.ParseSerialNumber(serialNumber);

        // Assert
        components.SerialType.Should().Be(SerialType.Destruction);
        components.Year.Should().Be(99);
        components.Week.Should().Be(53);
        components.BatchSequence.Should().Be(9999);
        components.Sequence.Should().Be(999999);
    }

    [Fact]
    public void ParseSerialNumber_ApproximateDate_Should_Return_Week_Start()
    {
        // Arrange
        var serialNumber = "1025510001000001"; // 2025, Week 51

        // Act
        var components = _service.ParseSerialNumber(serialNumber);

        // Assert
        // Week 51 of 2025 starts on Monday, December 15
        components.ApproximateDate.Should().Be(new DateTime(2025, 12, 15));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void ParseSerialNumber_Should_Throw_For_Invalid_Input(string serialNumber)
    {
        // Act
        var act = () => _service.ParseSerialNumber(serialNumber);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // SERIAL TYPE NAME TESTS
    // ============================================================

    [Theory]
    [InlineData(SerialType.Production, "Production")]
    [InlineData(SerialType.GRV, "Goods Received")]
    [InlineData(SerialType.Retail, "Retail Sub")]
    [InlineData(SerialType.Bucking, "Bucking")]
    [InlineData(SerialType.Transfer, "Transfer")]
    [InlineData(SerialType.Adjustment, "Adjustment")]
    [InlineData(SerialType.Packaging, "Packaging")]
    [InlineData(SerialType.QCSample, "QC Sample")]
    [InlineData(SerialType.Destruction, "Destruction")]
    public void GetSerialTypeName_Should_Return_Correct_Name(SerialType serialType, string expectedName)
    {
        // Act
        var result = _service.GetSerialTypeName(serialType);

        // Assert
        result.Should().Be(expectedName);
    }

    // ============================================================
    // DERIVE PARENT BATCH TESTS
    // ============================================================

    [Fact]
    public async Task DeriveParentBatchNumber_Should_Reconstruct_Batch()
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);
        var serial = await _service.GenerateSerialNumberAsync(
            1, SerialType.Production, batchNumber, TestUser);

        // Act
        var derivedBatch = _service.DeriveParentBatchNumber(
            serial.SerialNumber, 1, BatchType.Production);

        // Assert
        derivedBatch.Should().Be(batchNumber);
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
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var act = async () => await _service.GenerateSerialNumberAsync(
            siteId, SerialType.Production, batchNumber, TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Throw_For_Invalid_BatchNumber()
    {
        // Act
        var act = async () => await _service.GenerateSerialNumberAsync(
            1, SerialType.Production, "invalid", TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Persist_SerialNumber_Record()
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        var result = await _service.GenerateSerialNumberAsync(
            1, SerialType.Production, batchNumber, TestUser);

        // Assert
        var serialNumbers = await _context.SerialNumbers.ToListAsync();
        serialNumbers.Should().HaveCount(1);
        serialNumbers[0].FullSerialNumber.Should().Be(result.SerialNumber);
    }

    [Fact]
    public async Task GenerateSerialNumberAsync_Should_Set_Audit_Fields()
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Act
        await _service.GenerateSerialNumberAsync(1, SerialType.Production, batchNumber, TestUser);

        // Assert
        var serial = await _context.SerialNumbers.FirstAsync();
        serial.CreatedBy.Should().Be(TestUser);
        serial.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ============================================================
    // VISUAL IDENTIFICATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateSerialNumber_Should_Be_Visually_Identifiable()
    {
        // This test documents the visual identification capability of the new format
        // Format: TT YY WW BBBB SSSSSS
        //         10 25 51 0001 000001
        //         │  │  │  │    └── Serial #1 from this batch
        //         │  │  │  └─────── From Batch 0001
        //         │  │  └────────── Week 51
        //         │  └───────────── 2025
        //         └──────────────── Production type (10)

        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);
        var serial = await _service.GenerateSerialNumberAsync(
            1, SerialType.Production, batchNumber, TestUser);

        // Parse visually
        serial.SerialNumber.Substring(0, 2).Should().Be("10", "Type should be Production (10)");
        serial.SerialNumber.Substring(2, 2).Should().Be("25", "Year should be 25");
        serial.SerialNumber.Substring(4, 2).Should().Be("51", "Week should be 51");
        serial.SerialNumber.Substring(6, 4).Should().Be("0001", "Batch sequence should be 0001");
        serial.SerialNumber.Substring(10, 6).Should().Be("000001", "Serial sequence should be 000001");
    }

    [Fact]
    public async Task Serial_Batch_Relationship_Should_Be_Traceable()
    {
        // This test documents the traceability from Serial → Batch
        var date = new DateTime(2025, 12, 15);

        // Create batch: 011025510001 (Site 01, Production, 2025-W51, Batch #1)
        var batchNumber = await _batchService.GenerateBatchNumberAsync(
            1, BatchType.Production, date, TestUser);

        // Create serial from this batch
        var serial = await _service.GenerateSerialNumberAsync(
            1, SerialType.Retail, batchNumber, TestUser);

        // Serial: 3025510001000001 (Retail, 2025-W51, from Batch 0001, Serial #1)
        // The YYWW and BBBB in serial match the batch!
        var batchComponents = _batchService.ParseBatchNumber(batchNumber);
        var serialComponents = _service.ParseSerialNumber(serial.SerialNumber);

        // Verify traceability
        serialComponents.Year.Should().Be(batchComponents.Year);
        serialComponents.Week.Should().Be(batchComponents.Week);
        serialComponents.BatchSequence.Should().Be(batchComponents.Sequence);

        // Can reconstruct parent batch from serial
        var derivedBatch = _service.DeriveParentBatchNumber(
            serial.SerialNumber, 1, BatchType.Production);
        derivedBatch.Should().Be(batchNumber);
    }
}
