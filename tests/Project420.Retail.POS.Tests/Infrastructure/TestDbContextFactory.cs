using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.DAL;

namespace Project420.Retail.POS.Tests.Infrastructure;

/// <summary>
/// Factory for creating in-memory test databases.
/// Ensures each test gets an isolated database instance.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Create an in-memory POS database context for testing
    /// </summary>
    /// <param name="databaseName">Unique database name for test isolation</param>
    /// <returns>Configured PosDbContext with in-memory database</returns>
    public static PosDbContext CreatePosDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new PosDbContext(options);

        // Ensure database is created
        context.Database.EnsureCreated();

        return context;
    }
}
