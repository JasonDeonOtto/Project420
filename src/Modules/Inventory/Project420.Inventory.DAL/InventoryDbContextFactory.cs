using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Inventory.DAL;

/// <summary>
/// Design-time factory for creating InventoryDbContext instances.
/// Used by EF Core tools for migrations and scaffolding.
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        // Use SQL Server for development/migrations
        optionsBuilder.UseSqlServer("Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
