using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Shared.Tests.Services;

/// <summary>
/// Unit tests for MovementService (Movement Architecture - Option A).
/// Tests SOH calculation, movement generation, and reversal.
/// </summary>
/// <remarks>
/// Uses TestBusinessDbContext which implements IBusinessDbContext for testing.
/// This allows testing the MovementService without requiring the full PosDbContext.
/// </remarks>
public class MovementServiceTests : IDisposable
{
    private readonly TestBusinessDbContext _context;
    private readonly Mock<ILogger<MovementService>> _loggerMock;
    private readonly MovementService _service;

    public MovementServiceTests()
    {
        // Create in-memory database for each test
        var options = new DbContextOptionsBuilder<TestBusinessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestBusinessDbContext(options);
        _loggerMock = new Mock<ILogger<MovementService>>();
        _service = new MovementService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // MOVEMENT GENERATION TESTS
    // ============================================================

    [Fact]
    public async Task GenerateMovementsAsync_Sale_Should_Create_OUT_Movements()
    {
        // Arrange
        var headerId = 1;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.Sale, productId: 100, quantity: 5);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.Sale, headerId);

        // Assert
        count.Should().Be(1);

        var movements = await _context.Movements.Where(m => m.HeaderId == headerId).ToListAsync();
        movements.Should().HaveCount(1);
        movements[0].Direction.Should().Be(MovementDirection.Out);
        movements[0].Quantity.Should().Be(5);
        movements[0].ProductId.Should().Be(100);
        movements[0].MovementType.Should().Be("Retail Sale");
    }

    [Fact]
    public async Task GenerateMovementsAsync_GRV_Should_Create_IN_Movements()
    {
        // Arrange
        var headerId = 2;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.GRV, productId: 101, quantity: 20);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.GRV, headerId);

        // Assert
        count.Should().Be(1);

        var movements = await _context.Movements.Where(m => m.HeaderId == headerId).ToListAsync();
        movements.Should().HaveCount(1);
        movements[0].Direction.Should().Be(MovementDirection.In);
        movements[0].Quantity.Should().Be(20);
        movements[0].MovementType.Should().Be("Goods Received");
    }

    [Fact]
    public async Task GenerateMovementsAsync_Refund_Should_Create_IN_Movements()
    {
        // Arrange
        var headerId = 3;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.Refund, productId: 102, quantity: 2);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.Refund, headerId);

        // Assert
        count.Should().Be(1);

        var movements = await _context.Movements.Where(m => m.HeaderId == headerId).ToListAsync();
        movements.Should().HaveCount(1);
        movements[0].Direction.Should().Be(MovementDirection.In);
        movements[0].MovementType.Should().Be("Customer Refund");
    }

    [Fact]
    public async Task GenerateMovementsAsync_ProductionInput_Should_Create_OUT_Movements()
    {
        // Arrange
        var headerId = 4;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.ProductionInput, productId: 103, quantity: 100);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.ProductionInput, headerId);

        // Assert
        count.Should().Be(1);

        var movements = await _context.Movements.Where(m => m.HeaderId == headerId).ToListAsync();
        movements[0].Direction.Should().Be(MovementDirection.Out);
        movements[0].MovementType.Should().Be("Production Input");
    }

    [Fact]
    public async Task GenerateMovementsAsync_ProductionOutput_Should_Create_IN_Movements()
    {
        // Arrange
        var headerId = 5;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.ProductionOutput, productId: 104, quantity: 50);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.ProductionOutput, headerId);

        // Assert
        count.Should().Be(1);

        var movements = await _context.Movements.Where(m => m.HeaderId == headerId).ToListAsync();
        movements[0].Direction.Should().Be(MovementDirection.In);
        movements[0].MovementType.Should().Be("Production Output");
    }

    [Fact]
    public async Task GenerateMovementsAsync_NoDetails_Should_Return_Zero()
    {
        // Arrange - no transaction details exist

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.Sale, 999);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GenerateMovementsAsync_AccountPayment_Should_Skip_NonStockTransaction()
    {
        // Arrange
        var headerId = 6;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.AccountPayment, productId: 105, quantity: 1);
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.AccountPayment, headerId);

        // Assert - Account payments don't generate movements
        count.Should().Be(0);
    }

    [Fact]
    public async Task GenerateMovementsAsync_WithBatchNumber_Should_Link_Correctly()
    {
        // Arrange
        var headerId = 7;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.GRV, productId: 106, quantity: 10);
        transactionDetail.BatchNumber = "0101202512110001";
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.GRV, headerId);

        // Assert
        var movement = await _context.Movements.FirstAsync(m => m.HeaderId == headerId);
        movement.BatchNumber.Should().Be("0101202512110001");
    }

    [Fact]
    public async Task GenerateMovementsAsync_WithSerialNumber_Should_Link_Correctly()
    {
        // Arrange
        var headerId = 8;
        var transactionDetail = CreateTransactionDetail(headerId, TransactionType.Sale, productId: 107, quantity: 1);
        transactionDetail.SerialNumber = "SN-12345-67890";
        _context.TransactionDetails.Add(transactionDetail);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GenerateMovementsAsync(TransactionType.Sale, headerId);

        // Assert
        var movement = await _context.Movements.FirstAsync(m => m.HeaderId == headerId);
        movement.SerialNumber.Should().Be("SN-12345-67890");
    }

    // ============================================================
    // MOVEMENT REVERSAL TESTS
    // ============================================================

    [Fact]
    public async Task ReverseMovementsAsync_Should_Soft_Delete_Movements()
    {
        // Arrange - Create a movement first
        var headerId = 10;
        var movement = CreateMovement(productId: 200, headerId: headerId, transactionType: TransactionType.Sale);
        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.ReverseMovementsAsync(TransactionType.Sale, headerId, "Order cancelled by customer");

        // Assert
        count.Should().Be(1);

        // Movement should be soft deleted
        var reversedMovement = await _context.Movements
            .IgnoreQueryFilters()
            .FirstAsync(m => m.HeaderId == headerId);

        reversedMovement.IsDeleted.Should().BeTrue();
        reversedMovement.DeletedAt.Should().NotBeNull();
        reversedMovement.MovementReason.Should().Contain("REVERSED");
    }

    [Fact]
    public async Task ReverseMovementsAsync_NoMovements_Should_Return_Zero()
    {
        // Act
        var count = await _service.ReverseMovementsAsync(TransactionType.Sale, 9999, "Test reason");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void ReverseMovementsAsync_EmptyReason_Should_Throw()
    {
        // Act & Assert
        var act = async () => await _service.ReverseMovementsAsync(TransactionType.Sale, 1, "");

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Reason is required*");
    }

    // ============================================================
    // SOH CALCULATION TESTS
    // ============================================================

    [Fact]
    public async Task CalculateSOHAsync_Should_Return_Correct_SOH()
    {
        // Arrange - Product 300: GRV 100, Sale 30, Sale 20 = SOH 50
        var productId = 300;

        _context.Movements.AddRange(
            CreateMovement(productId, 1, TransactionType.GRV, MovementDirection.In, 100),
            CreateMovement(productId, 2, TransactionType.Sale, MovementDirection.Out, 30),
            CreateMovement(productId, 3, TransactionType.Sale, MovementDirection.Out, 20)
        );
        await _context.SaveChangesAsync();

        // Act
        var soh = await _service.CalculateSOHAsync(productId);

        // Assert
        soh.Should().Be(50); // 100 - 30 - 20 = 50
    }

    [Fact]
    public async Task CalculateSOHAsync_WithAsOfDate_Should_Return_Historical_SOH()
    {
        // Arrange
        var productId = 301;
        var baseDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

        _context.Movements.AddRange(
            CreateMovementWithDate(productId, 1, TransactionType.GRV, MovementDirection.In, 100, baseDate.AddDays(-5)),
            CreateMovementWithDate(productId, 2, TransactionType.Sale, MovementDirection.Out, 30, baseDate.AddDays(-3)),
            CreateMovementWithDate(productId, 3, TransactionType.Sale, MovementDirection.Out, 20, baseDate.AddDays(2)) // Future
        );
        await _context.SaveChangesAsync();

        // Act - Calculate SOH as of baseDate (before the last sale)
        var soh = await _service.CalculateSOHAsync(productId, asOfDate: baseDate);

        // Assert
        soh.Should().Be(70); // 100 - 30 = 70 (excluding future sale of 20)
    }

    [Fact]
    public async Task CalculateSOHAsync_Should_Exclude_Deleted_Movements()
    {
        // Arrange
        var productId = 302;

        var activeMovement = CreateMovement(productId, 1, TransactionType.GRV, MovementDirection.In, 100);
        var deletedMovement = CreateMovement(productId, 2, TransactionType.Sale, MovementDirection.Out, 50);
        deletedMovement.IsDeleted = true;

        _context.Movements.AddRange(activeMovement, deletedMovement);
        await _context.SaveChangesAsync();

        // Act
        var soh = await _service.CalculateSOHAsync(productId);

        // Assert - Deleted movement should be excluded
        soh.Should().Be(100); // Only the GRV counts
    }

    [Fact]
    public async Task CalculateSOHAsync_NoMovements_Should_Return_Zero()
    {
        // Act
        var soh = await _service.CalculateSOHAsync(productId: 9999);

        // Assert
        soh.Should().Be(0);
    }

    [Fact]
    public async Task CalculateBatchSOHAsync_Should_Return_Batch_Specific_SOH()
    {
        // Arrange
        var productId = 303;
        var batchNumber = "BATCH-001";

        _context.Movements.AddRange(
            CreateMovementWithBatch(productId, 1, TransactionType.GRV, MovementDirection.In, 50, batchNumber),
            CreateMovementWithBatch(productId, 2, TransactionType.Sale, MovementDirection.Out, 10, batchNumber),
            CreateMovementWithBatch(productId, 3, TransactionType.GRV, MovementDirection.In, 30, "BATCH-002") // Different batch
        );
        await _context.SaveChangesAsync();

        // Act
        var soh = await _service.CalculateBatchSOHAsync(productId, batchNumber);

        // Assert
        soh.Should().Be(40); // 50 - 10 = 40 (only BATCH-001)
    }

    [Fact]
    public async Task CalculateSOHBatchAsync_Should_Return_Dictionary_Of_SOH()
    {
        // Arrange - Multiple products
        var product1 = 400;
        var product2 = 401;
        var product3 = 402;

        _context.Movements.AddRange(
            CreateMovement(product1, 1, TransactionType.GRV, MovementDirection.In, 100),
            CreateMovement(product1, 2, TransactionType.Sale, MovementDirection.Out, 25),
            CreateMovement(product2, 3, TransactionType.GRV, MovementDirection.In, 50),
            CreateMovement(product3, 4, TransactionType.GRV, MovementDirection.In, 200),
            CreateMovement(product3, 5, TransactionType.Sale, MovementDirection.Out, 150)
        );
        await _context.SaveChangesAsync();

        // Act
        var sohDict = await _service.CalculateSOHBatchAsync(new[] { product1, product2, product3 });

        // Assert
        sohDict.Should().HaveCount(3);
        sohDict[product1].Should().Be(75);  // 100 - 25
        sohDict[product2].Should().Be(50);  // 50
        sohDict[product3].Should().Be(50);  // 200 - 150
    }

    [Fact]
    public async Task CalculateSOHBatchAsync_EmptyList_Should_Return_Empty_Dictionary()
    {
        // Act
        var sohDict = await _service.CalculateSOHBatchAsync(Array.Empty<int>());

        // Assert
        sohDict.Should().BeEmpty();
    }

    // ============================================================
    // MOVEMENT QUERY TESTS
    // ============================================================

    [Fact]
    public async Task GetMovementHistoryAsync_Should_Return_Movements_In_Date_Range()
    {
        // Arrange
        var productId = 500;
        var startDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        _context.Movements.AddRange(
            CreateMovementWithDate(productId, 1, TransactionType.GRV, MovementDirection.In, 10, startDate.AddDays(5)),
            CreateMovementWithDate(productId, 2, TransactionType.Sale, MovementDirection.Out, 5, startDate.AddDays(10)),
            CreateMovementWithDate(productId, 3, TransactionType.GRV, MovementDirection.In, 20, startDate.AddDays(-10)) // Before range
        );
        await _context.SaveChangesAsync();

        // Act
        var movements = await _service.GetMovementHistoryAsync(productId, startDate, endDate);

        // Assert
        movements.Should().HaveCount(2); // Only movements within date range
    }

    [Fact]
    public async Task GetMovementsByBatchAsync_Should_Return_All_Batch_Movements()
    {
        // Arrange
        var batchNumber = "BATCH-TEST-001";

        _context.Movements.AddRange(
            CreateMovementWithBatch(501, 1, TransactionType.GRV, MovementDirection.In, 100, batchNumber),
            CreateMovementWithBatch(502, 2, TransactionType.Sale, MovementDirection.Out, 20, batchNumber),
            CreateMovementWithBatch(503, 3, TransactionType.GRV, MovementDirection.In, 50, "OTHER-BATCH")
        );
        await _context.SaveChangesAsync();

        // Act
        var movements = await _service.GetMovementsByBatchAsync(batchNumber);

        // Assert
        movements.Should().HaveCount(2);
        movements.All(m => m.BatchNumber == batchNumber).Should().BeTrue();
    }

    [Fact]
    public async Task GetMovementsBySerialNumberAsync_Should_Return_Serial_Movements()
    {
        // Arrange
        var serialNumber = "SN-001-ABCDE";

        var movement = CreateMovement(600, 1, TransactionType.Sale, MovementDirection.Out, 1);
        movement.SerialNumber = serialNumber;
        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        // Act
        var movements = await _service.GetMovementsBySerialNumberAsync(serialNumber);

        // Assert
        movements.Should().HaveCount(1);
        movements.First().SerialNumber.Should().Be(serialNumber);
    }

    [Fact]
    public async Task GetMovementsByTransactionAsync_Should_Return_Transaction_Movements()
    {
        // Arrange
        var headerId = 700;

        _context.Movements.AddRange(
            CreateMovement(701, headerId, TransactionType.Sale, MovementDirection.Out, 5),
            CreateMovement(702, headerId, TransactionType.Sale, MovementDirection.Out, 3),
            CreateMovement(703, 999, TransactionType.Sale, MovementDirection.Out, 10) // Different header
        );
        await _context.SaveChangesAsync();

        // Act
        var movements = await _service.GetMovementsByTransactionAsync(TransactionType.Sale, headerId);

        // Assert
        movements.Should().HaveCount(2);
        movements.All(m => m.HeaderId == headerId).Should().BeTrue();
    }

    // ============================================================
    // UTILITY METHOD TESTS
    // ============================================================

    [Theory]
    [InlineData(TransactionType.GRV, MovementDirection.In)]
    [InlineData(TransactionType.Refund, MovementDirection.In)]
    [InlineData(TransactionType.WholesaleRefund, MovementDirection.In)]
    [InlineData(TransactionType.ProductionOutput, MovementDirection.In)]
    [InlineData(TransactionType.TransferIn, MovementDirection.In)]
    [InlineData(TransactionType.AdjustmentIn, MovementDirection.In)]
    [InlineData(TransactionType.Sale, MovementDirection.Out)]
    [InlineData(TransactionType.RTS, MovementDirection.Out)]
    [InlineData(TransactionType.WholesaleSale, MovementDirection.Out)]
    [InlineData(TransactionType.ProductionInput, MovementDirection.Out)]
    [InlineData(TransactionType.TransferOut, MovementDirection.Out)]
    [InlineData(TransactionType.AdjustmentOut, MovementDirection.Out)]
    public void GetMovementDirection_Should_Map_All_Transaction_Types(
        TransactionType transactionType, MovementDirection expectedDirection)
    {
        // Act
        var direction = _service.GetMovementDirection(transactionType);

        // Assert
        direction.Should().Be(expectedDirection);
    }

    [Theory]
    [InlineData(TransactionType.Sale, "Retail Sale")]
    [InlineData(TransactionType.GRV, "Goods Received")]
    [InlineData(TransactionType.Refund, "Customer Refund")]
    [InlineData(TransactionType.RTS, "Return to Supplier")]
    [InlineData(TransactionType.ProductionInput, "Production Input")]
    [InlineData(TransactionType.ProductionOutput, "Production Output")]
    public void GetMovementTypeName_Should_Return_Descriptive_Names(
        TransactionType transactionType, string expectedName)
    {
        // Act
        var name = _service.GetMovementTypeName(transactionType);

        // Assert
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(TransactionType.Sale, true)]
    [InlineData(TransactionType.GRV, true)]
    [InlineData(TransactionType.Refund, true)]
    [InlineData(TransactionType.AccountPayment, false)]
    [InlineData(TransactionType.Quote, false)]
    public void IsStockAffectingTransaction_Should_Correctly_Identify_Stock_Transactions(
        TransactionType transactionType, bool shouldAffectStock)
    {
        // Act
        var result = _service.IsStockAffectingTransaction(transactionType);

        // Assert
        result.Should().Be(shouldAffectStock);
    }

    // ============================================================
    // DIRECT MOVEMENT CREATION TESTS
    // ============================================================

    [Fact]
    public async Task CreateMovementAsync_Should_Create_Movement_Successfully()
    {
        // Arrange
        var movement = new Movement
        {
            ProductId = 800,
            ProductSKU = "SKU-800",
            ProductName = "Test Product",
            MovementType = "Stock Adjustment",
            Direction = MovementDirection.In,
            Quantity = 25,
            TransactionType = TransactionType.AdjustmentIn,
            HeaderId = 800,
            DetailId = 8001,
            MovementReason = "Annual stocktake correction"
        };

        // Act
        var created = await _service.CreateMovementAsync(movement);

        // Assert
        created.Id.Should().BeGreaterThan(0);
        created.TransactionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateMovementAsync_NullMovement_Should_Throw()
    {
        // Act & Assert
        var act = async () => await _service.CreateMovementAsync(null!);

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void CreateMovementAsync_InvalidProductId_Should_Throw()
    {
        // Arrange
        var movement = new Movement
        {
            ProductId = 0, // Invalid
            MovementType = "Test",
            Direction = MovementDirection.In,
            Quantity = 10,
            MovementReason = "Test"
        };

        // Act & Assert
        var act = async () => await _service.CreateMovementAsync(movement);

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ProductId must be greater than 0*");
    }

    [Fact]
    public void CreateMovementAsync_ZeroQuantity_Should_Throw()
    {
        // Arrange
        var movement = new Movement
        {
            ProductId = 1,
            MovementType = "Test",
            Direction = MovementDirection.In,
            Quantity = 0, // Invalid
            MovementReason = "Test"
        };

        // Act & Assert
        var act = async () => await _service.CreateMovementAsync(movement);

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Quantity must be greater than 0*");
    }

    [Fact]
    public void CreateMovementAsync_EmptyReason_Should_Throw()
    {
        // Arrange
        var movement = new Movement
        {
            ProductId = 1,
            MovementType = "Test",
            Direction = MovementDirection.In,
            Quantity = 10,
            MovementReason = "" // Invalid for audit compliance
        };

        // Act & Assert
        var act = async () => await _service.CreateMovementAsync(movement);

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*MovementReason is required*");
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private static TransactionDetail CreateTransactionDetail(
        int headerId,
        TransactionType transactionType,
        int productId,
        decimal quantity,
        decimal unitPrice = 100m)
    {
        return new TransactionDetail
        {
            HeaderId = headerId,
            TransactionType = transactionType,
            ProductId = productId,
            ProductSKU = $"SKU-{productId}",
            ProductName = $"Product {productId}",
            Quantity = quantity,
            UnitPrice = unitPrice,
            VATAmount = unitPrice * quantity * 0.15m / 1.15m,
            LineTotal = unitPrice * quantity,
            CreatedBy = "TEST"
        };
    }

    private static Movement CreateMovement(
        int productId,
        int headerId,
        TransactionType transactionType,
        MovementDirection? direction = null,
        decimal quantity = 10)
    {
        return new Movement
        {
            ProductId = productId,
            ProductSKU = $"SKU-{productId}",
            ProductName = $"Product {productId}",
            MovementType = transactionType.ToString(),
            Direction = direction ?? (transactionType == TransactionType.GRV ? MovementDirection.In : MovementDirection.Out),
            Quantity = quantity,
            TransactionType = transactionType,
            HeaderId = headerId,
            DetailId = headerId * 10,
            MovementReason = $"Test movement for {transactionType}",
            TransactionDate = DateTime.UtcNow,
            CreatedBy = "TEST"
        };
    }

    private static Movement CreateMovementWithDate(
        int productId,
        int headerId,
        TransactionType transactionType,
        MovementDirection direction,
        decimal quantity,
        DateTime transactionDate)
    {
        var movement = CreateMovement(productId, headerId, transactionType, direction, quantity);
        movement.TransactionDate = transactionDate;
        return movement;
    }

    private static Movement CreateMovementWithBatch(
        int productId,
        int headerId,
        TransactionType transactionType,
        MovementDirection direction,
        decimal quantity,
        string batchNumber)
    {
        var movement = CreateMovement(productId, headerId, transactionType, direction, quantity);
        movement.BatchNumber = batchNumber;
        return movement;
    }
}
