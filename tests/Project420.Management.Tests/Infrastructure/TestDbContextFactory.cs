using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL;

namespace Project420.Management.Tests.Infrastructure;

/// <summary>
/// Factory for creating in-memory database contexts for testing.
/// Uses EF Core InMemory provider for fast, isolated testing.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory ManagementDbContext for testing.
    /// Each test gets an isolated database instance.
    /// </summary>
    /// <param name="databaseName">Unique database name (typically test method name)</param>
    /// <returns>ManagementDbContext configured for in-memory testing</returns>
    public static ManagementDbContext CreateManagementDbContext(string? databaseName = null)
    {
        // Use provided name or generate unique name
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        // Configure InMemory database
        var options = new DbContextOptionsBuilder<ManagementDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging() // Helpful for debugging tests
            .Options;

        var context = new ManagementDbContext(options);

        // Ensure database is created
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Seeds a ManagementDbContext with test data.
    /// Useful for integration tests that need pre-populated data.
    /// </summary>
    /// <param name="context">The context to seed</param>
    public static void SeedTestData(ManagementDbContext context)
    {
        // Add seed data here if needed for integration tests
        // Example:
        // context.Products.AddRange(GetTestProducts());
        // context.Debtors.AddRange(GetTestCustomers());
        // context.SaveChanges();
    }
}
