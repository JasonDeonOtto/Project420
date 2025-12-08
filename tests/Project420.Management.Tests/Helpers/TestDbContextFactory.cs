using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL;

namespace Project420.Management.Tests.Helpers;

/// <summary>
/// Factory for creating in-memory test databases.
/// Each test gets its own isolated database to prevent conflicts.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new ManagementDbContext with an in-memory database.
    /// Each database has a unique name to ensure test isolation.
    /// </summary>
    public static ManagementDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var context = new ManagementDbContext(options);

        // Ensure database is created
        context.Database.EnsureCreated();

        return context;
    }
}
