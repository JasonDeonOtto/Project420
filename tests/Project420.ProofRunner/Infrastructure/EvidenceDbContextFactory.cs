using Microsoft.EntityFrameworkCore;
using Project420.Inventory.DAL;
using Project420.Shared.Database;

namespace Project420.ProofRunner.Infrastructure;

/// <summary>
/// Factory for creating in-memory databases for evidence demonstrations.
/// Isolated instance per evidence check to prevent cross-contamination.
/// </summary>
public static class EvidenceDbContextFactory
{
    /// <summary>
    /// Creates an in-memory InventoryDbContext for stock movement evidence.
    /// </summary>
    public static InventoryDbContext CreateInventoryContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new InventoryDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates an in-memory SharedDbContext for Movement ledger evidence.
    /// </summary>
    public static SharedDbContext CreateSharedContext()
    {
        var options = new DbContextOptionsBuilder<SharedDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new SharedDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
