using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Production.DAL;

/// <summary>
/// Design-time factory for creating ProductionDbContext instances.
/// Used by EF Core tools for migrations and scaffolding.
/// </summary>
public class ProductionDbContextFactory : IDesignTimeDbContextFactory<ProductionDbContext>
{
    public ProductionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductionDbContext>();

        // Use SQL Server for development/migrations
        optionsBuilder.UseSqlServer("Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new ProductionDbContext(optionsBuilder.Options);
    }
}
