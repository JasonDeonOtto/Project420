using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Shared.Tests.Services;

/// <summary>
/// Unit tests for BatchNumberGeneratorService (Phase 8).
/// Tests batch number generation, validation, and parsing.
/// Format: SSTTYYYYMMDDNNNN (16 digits)
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
    public async Task GenerateBatchNumberAsync_Should_Generate_16_Digit_Number()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(16);
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Format_Correctly()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production; // 10
        var date = new DateTime(2025, 12, 12);

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        // Format: SSTTYYYYMMDDNNNN
        // SS = 01, TT = 10, YYYYMMDD = 20251212, NNNN = 0001
        result.Should().Be("0110202512120001");
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Increment_Sequence()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

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
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_Date()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date1 = new DateTime(2025, 12, 12);
        var date2 = new DateTime(2025, 12, 13);

        // Act
        var result1Day1 = await _service.GenerateBatchNumberAsync(siteId, batchType, date1, TestUser);
        var result2Day1 = await _service.GenerateBatchNumberAsync(siteId, batchType, date1, TestUser);
        var result1Day2 = await _service.GenerateBatchNumberAsync(siteId, batchType, date2, TestUser);

        // Assert
        result1Day1.Should().EndWith("0001");
        result2Day1.Should().EndWith("0002");
        result1Day2.Should().EndWith("0001"); // Resets for new date
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_BatchType()
    {
        // Arrange
        var siteId = 1;
        var date = new DateTime(2025, 12, 12);

        // Act
        var production = await _service.GenerateBatchNumberAsync(siteId, BatchType.Production, date, TestUser);
        var transfer = await _service.GenerateBatchNumberAsync(siteId, BatchType.Transfer, date, TestUser);

        // Assert
        production.Should().Contain("10"); // Production type code
        transfer.Should().Contain("20");   // Transfer type code
        production.Should().EndWith("0001");
        transfer.Should().EndWith("0001");
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Have_Separate_Sequences_Per_Site()
    {
        // Arrange
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

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
        var date = new DateTime(2025, 12, 12);

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

        // Act
        var result = await _service.GenerateBatchNumberAsync(siteId, batchType, requestedBy: TestUser);

        // Assert
        var expectedDate = today.ToString("yyyyMMdd");
        result.Substring(4, 8).Should().Be(expectedDate);
    }

    // ============================================================
    // VALIDATION TESTS
    // ============================================================

    [Theory]
    [InlineData("0110202512120001", true)]
    [InlineData("9990202512129999", true)]
    [InlineData("0120202312010100", true)]
    public void ValidateBatchNumber_Should_Return_True_For_Valid_Numbers(string batchNumber, bool expected)
    {
        // Act
        var result = _service.ValidateBatchNumber(batchNumber);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]           // Empty
    [InlineData(null)]         // Null
    [InlineData("01102025121")] // Too short (11 chars)
    [InlineData("011020251212000123")] // Too long (18 chars)
    [InlineData("ABCD202512120001")] // Non-numeric
    [InlineData("0099202512120001")] // Invalid batch type 99
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
        var batchNumber = "0110202512120001";

        // Act
        var components = _service.ParseBatchNumber(batchNumber);

        // Assert
        components.SiteId.Should().Be(1);
        components.BatchType.Should().Be(BatchType.Production);
        components.BatchDate.Should().Be(new DateTime(2025, 12, 12));
        components.Sequence.Should().Be(1);
    }

    [Fact]
    public void ParseBatchNumber_Should_Handle_Max_Values()
    {
        // Arrange
        var batchNumber = "9990202512319999";

        // Act
        var components = _service.ParseBatchNumber(batchNumber);

        // Assert
        components.SiteId.Should().Be(99);
        components.BatchType.Should().Be(BatchType.Reserved); // 90
        components.BatchDate.Should().Be(new DateTime(2025, 12, 31));
        components.Sequence.Should().Be(9999);
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
        var date = new DateTime(2025, 12, 12);

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
        var date = new DateTime(2025, 12, 12);

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
        var date = new DateTime(2025, 12, 12);
        var batchNumber = await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Act
        var exists = await _service.BatchNumberExistsAsync(batchNumber);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BatchNumberExistsAsync_Should_Return_False_For_NonExisting()
    {
        // Arrange
        var nonExistingBatch = "9999202512129999";

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
        var date = new DateTime(2025, 12, 12);

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
        var date = new DateTime(2025, 12, 12);

        // Act
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        var sequences = await _context.BatchNumberSequences.ToListAsync();
        sequences.Should().HaveCount(1);
        sequences[0].SiteId.Should().Be(siteId);
        sequences[0].BatchType.Should().Be(batchType);
        sequences[0].BatchDate.Should().Be(date);
        sequences[0].CurrentSequence.Should().Be(1);
    }

    [Fact]
    public async Task GenerateBatchNumberAsync_Should_Set_Audit_Fields()
    {
        // Arrange
        var siteId = 1;
        var batchType = BatchType.Production;
        var date = new DateTime(2025, 12, 12);

        // Act
        await _service.GenerateBatchNumberAsync(siteId, batchType, date, TestUser);

        // Assert
        var sequence = await _context.BatchNumberSequences.FirstAsync();
        sequence.CreatedBy.Should().Be(TestUser);
        sequence.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
