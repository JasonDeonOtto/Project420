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
/// Unit tests for BatchNumberGeneratorService.
/// Tests batch number generation, validation, and parsing.
/// Format: SSTTYYYWWNNNN (12 digits, week-based)
/// </summary>
/// <remarks>
/// Uses TestBusinessDbContext which implements IBusinessDbContext for testing.
/// This allows testing without requiring the full PosDbContext.
/// </remarks>
public class BatchNumberGeneratorServiceTests : IDisposable
{
    private readonly TestBusinessDbContext _context;
    private readonly Mock<ILogger<BatchNumberGeneratorService>> _loggerMock;
    private readonly BatchNumberGeneratorService _service;
    private const string TestUser = "TestUser";

    public BatchNumberGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestBusinessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestBusinessDbContext(options);
        _loggerMock = new Mock<ILogger<BatchNumberGeneratorService>>();
        _service = new BatchNumberGeneratorService(_context, _loggerMock.Object, TestUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // BATCH NUMBER GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Generate_12_Digit_Number()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 19); // Week 51 of 2025

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(12);
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Format_Correctly()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production; // 10
        var date = new DateTime(2025, 12, 15); // Week 51 of 2025

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        // Format: SSTTYYYWWNNNN
        // SS = 01, TT = 10, YY = 25, WW = 51, NNNN = 0001
        result.Should().Be("011025510001");
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Increment_Sequence()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15); // Week 51

        // Act
        var result1 = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);
        var result2 = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);
        var result3 = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        result1.Should().EndWith("0001");
        result2.Should().EndWith("0002");
        result3.Should().EndWith("0003");
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_Week()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var week51Date = new DateTime(2025, 12, 15); // Week 51
        var week52Date = new DateTime(2025, 12, 22); // Week 52

        // Act
        var result1Week51 = await _service.GenerateBatchNumberAsync(siteId, batchType, week51Date, TestUser);
        var result2Week51 = await _service.GenerateBatchNumberAsync(siteId, batchType, week51Date, TestUser);
        var result1Week52 = await _service.GenerateBatchNumberAsync(siteId, batchType, week52Date, TestUser);

        // Assert
        result1Week51.Should().EndWith("0001");
        result2Week51.Should().EndWith("0002");
        result1Week52.Should().EndWith("0001"); // Resets for new week
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Same_Week_Different_Days_Should_Share_Sequence()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        // Both dates are in Week 51 of 2025
        var monday = new DateTime(2025, 12, 15); // Monday
        var friday = new DateTime(2025, 12, 19); // Friday

        // Act
        var result1 = await _service.GenerateBatchNumberAsync(siteId, batchType, monday, TestUser);
        var result2 = await _service.GenerateBatchNumberAsync(siteId, batchType, friday, TestUser);

        // Assert - Same week should increment the same sequence
        result1.Should().EndWith("0001");
        result2.Should().EndWith("0002");
        result1.Substring(4, 4).Should().Be("2551"); // YYWW
        result2.Substring(4, 4).Should().Be("2551"); // Same YYWW
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_BatchType()
    {
        // Arrange
        var siteId = 1;
        var date = new DateTime(2025, 12, 15);

        // Act
        var production = await _service.GenerateBatchNumberAsync(siteId, BatchType.Production, date, TestUser);
        var transfer = await _service.GenerateBatchNumberAsync(siteId, BatchType.Transfer, date, TestUser);

        // Assert
        production.Substring(2, 2).Should().Be("10"); // Production type code
        transfer.Substring(2, 2).Should().Be("20");   // Transfer type code
        production.Should().EndWith("0001");
        transfer.Should().EndWith("0001");
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_Site()
    {
        // Arrange
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);

        // Act
        var site1 = await _service.GenerateBatchNumberAsync(1, batchType, date, TestUser);
        var site2 = await _service.GenerateBatchNumberAsync(2, batchType, date, TestUser);

        // Assert
        site1.Should().StartWith("01"); // Site 1
        site2.Should().StartWith("02"); // Site 2
        site1.Should().EndWith("0001");
        site2.Should().EndWith("0001");
    }

    [Theory]
    [InlineData(BatchType.Production, "10")]
    [InlineData(BatchType.Transfer, "20")]
    [InlineData(BatchType.StockTake, "30")]
    [InlineData(BatchType.Adjustment, "40")]
    [InlineData(BatchType.ReturnToSupplier, "50")]
    [InlineData(BatchType.Destruction, "60")]
    [InlineData(BatchType.CustomerReturn, "70")]
    [InlineData(BatchType.Quarantine, "80")]
    [InlineData(BatchType.Reserved, "90")]
    public async Task GenerateBatchNumberAsync_Should_Use_Correct_Type_Codes(BatchType batchType, string expectedCode)
    {
        // Arrange
        var date = new DateTime(2025, 12, 15);

        // Act
        var result = await _service.GenerateBatchNumberAsync(1, batchType, date, TestUser);

        // Assert
        result.Substring(2, 2).Should().Be(expectedCode);
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Default_To_Today()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var today = DateTime.Today;
        var expectedYear = ISOWeek.GetYear(today) % 100;
        var expectedWeek = ISOWeek.GetWeekOfYear(today);

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, requestedBy: TestUser);

        // Assert
        var yearWeek = result.Substring(4, 4);
        yearWeek.Should().Be($"{expectedYear:D2}{expectedWeek:D2}");
    }

    // ============================================================
    // VALIDATION TESTS
    // ============================================================

    [Theory]
    [InlineData("011025510001", true)]  // Valid: Site 01, Production, 2025-W51, Seq 1
    [InlineData("992099539999", true)]  // Valid: Site 99, Transfer, 2099-W53, Seq 9999
    [InlineData("012020010100", true)]  // Valid: Site 01, Transfer, 2020-W01, Seq 100
    public void ValidateBatchNumber_Should_Return_True_For_Valid_Numbers(string batchNumber, bool expected)
    {
        // Act
        var result = _service.ValidateBatchNumber(batchNumber);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]                    // Empty
    [InlineData(null)]                  // Null
    [InlineData("0110255100")]          // Too short (10 chars)
    [InlineData("01102551000123")]      // Too long (14 chars)
    [InlineData("ABCD25510001")]        // Non-numeric
    [InlineData("019925510001")]        // Invalid batch type 99
    [InlineData("011025540001")]        // Invalid week 54
    [InlineData("011019510001")]        // Invalid year 19 (before 2020)
    public void ValidateBatchNumber_Should_Return_False_For_Invalid_Numbers(string batchNumber)
    {
        // Act
        var result = _service.ValidateBatchNumber(batchNumber);

        // Assert
        result.Should().BeFalse();
    }

    // ============================================================
    // PARSING TESTS
    // ============================================================

    [Fact]
    public void ParseBatchNumber_Should_Extract_All_Components()
    {
        // Arrange
        var batchNumber = "011025510001";

        // Act
        var components = _service.ParseBatchNumber(batchNumber);

        // Assert
        components.SiteId.Should().Be(1);
        components.BatchType.Should().Be(BatchType.Production);
        components.Year.Should().Be(25);
        components.Week.Should().Be(51);
        components.Sequence.Should().Be(1);
    }

    [Fact]
    public void ParseBatchNumber_Should_Handle_Max_Values()
    {
        // Arrange
        var batchNumber = "999025539999";

        // Act
        var components = _service.ParseBatchNumber(batchNumber);

        // Assert
        components.SiteId.Should().Be(99);
        components.BatchType.Should().Be(BatchType.Reserved); // 90
        components.Year.Should().Be(25);
        components.Week.Should().Be(53);
        components.Sequence.Should().Be(9999);
    }

    [Fact]
    public void ParseBatchNumber_ApproximateDate_Should_Return_Week_Start()
    {
        // Arrange
        var batchNumber = "011025510001"; // 2025, Week 51

        // Act
        var components = _service.ParseBatchNumber(batchNumber);

        // Assert
        // Week 51 of 2025 starts on Monday, December 15
        components.ApproximateDate.Should().Be(new DateTime(2025, 12, 15));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void ParseBatchNumber_Should_Throw_For_Invalid_Input(string batchNumber)
    {
        // Act
        var act = () => _service.ParseBatchNumber(batchNumber);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ============================================================
    // SEQUENCE MANAGEMENT TESTS
    // ============================================================

    [Fact]
    public async Task GetCurrentSequenceAsync_Should_Return_0_For_New_Combination()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);

        // Act
        var sequence = await _service.GetCurrentSequenceAsync(siteId, batchType, date);

        // Assert
        sequence.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentSequenceAsync_Should_Return_Current_Sequence()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);

        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Act
        var sequence = await _service.GetCurrentSequenceAsync(siteId, batchType, date);

        // Assert
        sequence.Should().Be(3);
    }

    [Fact]
    public async Task BatchNumberExistsAsync_Should_Return_True_For_Existing()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);
        var batchNumber = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Act
        var exists = await _service.BatchNumberExistsAsync(batchNumber);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BatchNumberExistsAsync_Should_Return_False_For_NonExisting()
    {
        // Arrange - Valid format but sequence doesn't exist
        var nonExistingBatch = "999025519999";

        // Act
        var exists = await _service.BatchNumberExistsAsync(nonExistingBatch);

        // Assert
        exists.Should().BeFalse();
    }

    // ============================================================
    // EDGE CASES & ERROR HANDLING
    // ============================================================

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task GenerateBatchNumberAsync_Should_Throw_For_Invalid_SiteId(int siteId)
    {
        // Arrange
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);

        // Act
        var act = async () => await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Persist_Sequence_Record()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);
        var expectedWeekDate = ISOWeek.ToDateTime(2025, 51, DayOfWeek.Monday);

        // Act
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        var sequences = await _context.BatchNumberSequences.ToListAsync();
        sequences.Should().HaveCount(1);
        sequences[0].SiteId.Should().Be(siteId);
        sequences[0].BatchType.Should().Be(batchType);
        sequences[0].BatchDate.Should().Be(expectedWeekDate); // Week start date
        sequences[0].CurrentSequence.Should().Be(1);
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Set_Audit_Fields()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 15);

        // Act
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        var sequence = await _context.BatchNumberSequences.FirstAsync();
        sequence.CreatedBy.Should().Be(TestUser);
        sequence.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ============================================================
    // VISUAL IDENTIFICATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateBatchNumber_Should_Be_Visually_Identifiable()
    {
        // This test documents the visual identification capability of the new format
        // Format: SS TT YY WW NNNN
        //         01 10 25 51 0001
        //         │  │  │  │  └── Batch #1 this week
        //         │  │  │  └───── Week 51
        //         │  │  └──────── 2025
        //         │  └─────────── Production batch (10)
        //         └────────────── Site 01

        var date = new DateTime(2025, 12, 15);
        var batch = await _service.GenerateBatchNumberAsync(1, BatchType.Production, date, TestUser);

        // Parse visually
        batch.Substring(0, 2).Should().Be("01", "Site ID should be 01");
        batch.Substring(2, 2).Should().Be("10", "Type should be Production (10)");
        batch.Substring(4, 2).Should().Be("25", "Year should be 25");
        batch.Substring(6, 2).Should().Be("51", "Week should be 51");
        batch.Substring(8, 4).Should().Be("0001", "Sequence should be 0001");
    }
}
