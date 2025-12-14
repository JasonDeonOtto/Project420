using Moq;
using Project420.Retail.POS.BLL.Services;
using Project420.Retail.POS.DAL;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Shared.Database.Services;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Retail.POS.Tests.Infrastructure;

/// <summary>
/// Base class for BLL service tests.
/// Provides common mocked dependencies and test infrastructure.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    /// <summary>
    /// In-memory database context for testing.
    /// Automatically created for each test.
    /// </summary>
    protected PosDbContext DbContext { get; private set; }

    /// <summary>
    /// Mocked Transaction Repository
    /// </summary>
    protected Mock<ITransactionRepository> MockTransactionRepository { get; private set; }

    /// <summary>
    /// Mocked VAT Calculation Service
    /// </summary>
    protected Mock<IVATCalculationService> MockVATService { get; private set; }

    /// <summary>
    /// Mocked Transaction Number Generator Service
    /// </summary>
    protected Mock<ITransactionNumberGeneratorService> MockTransactionNumberService { get; private set; }

    /// <summary>
    /// Mocked Movement Service
    /// </summary>
    protected Mock<IMovementService> MockMovementService { get; private set; }

    /// <summary>
    /// Mocked POS Calculation Service (Phase 9.2: Discount calculations)
    /// </summary>
    protected Mock<IPOSCalculationService> MockPOSCalculationService { get; private set; }

    /// <summary>
    /// Unique database name for this test instance.
    /// Ensures test isolation.
    /// </summary>
    protected string DatabaseName { get; private set; }

    /// <summary>
    /// Constructor - creates fresh test infrastructure for each test.
    /// </summary>
    protected ServiceTestBase()
    {
        // Generate unique database name for test isolation
        DatabaseName = Guid.NewGuid().ToString();

        // Create in-memory database context
        DbContext = TestDbContextFactory.CreatePosDbContext(DatabaseName);

        // Create mocks for common services
        MockTransactionRepository = new Mock<ITransactionRepository>();
        MockVATService = new Mock<IVATCalculationService>();
        MockTransactionNumberService = new Mock<ITransactionNumberGeneratorService>();
        MockMovementService = new Mock<IMovementService>();
        MockPOSCalculationService = new Mock<IPOSCalculationService>();

        // Setup default mock behaviors
        SetupDefaultMockBehaviors();
    }

    /// <summary>
    /// Setup default mock behaviors that are commonly used across tests.
    /// Override in derived classes to customize.
    /// </summary>
    protected virtual void SetupDefaultMockBehaviors()
    {
        // Default: Generate simple transaction numbers
        MockTransactionNumberService
            .Setup(x => x.GenerateAsync(It.IsAny<Project420.Shared.Core.Enums.TransactionTypeCode>()))
            .ReturnsAsync((Project420.Shared.Core.Enums.TransactionTypeCode type) =>
            {
                return $"{type}-{DateTime.Now:yyyyMMdd}-001";
            });
    }

    /// <summary>
    /// Clean up test resources.
    /// Disposes database context after each test.
    /// </summary>
    public virtual void Dispose()
    {
        DbContext?.Dispose();
    }
}
