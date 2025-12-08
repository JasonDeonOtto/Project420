using Project420.Management.DAL;

namespace Project420.Management.Tests.Infrastructure;

/// <summary>
/// Base class for DAL repository tests.
/// Provides in-memory database context for isolated repository testing.
/// </summary>
public abstract class RepositoryTestBase : IDisposable
{
    /// <summary>
    /// In-memory database context for testing.
    /// Automatically created for each test.
    /// </summary>
    protected ManagementDbContext DbContext { get; private set; }

    /// <summary>
    /// Unique database name for this test instance.
    /// Ensures test isolation.
    /// </summary>
    protected string DatabaseName { get; private set; }

    /// <summary>
    /// Constructor - creates fresh test infrastructure for each test.
    /// </summary>
    protected RepositoryTestBase()
    {
        // Generate unique database name for test isolation
        DatabaseName = Guid.NewGuid().ToString();

        // Create in-memory database context
        DbContext = TestDbContextFactory.CreateManagementDbContext(DatabaseName);
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
