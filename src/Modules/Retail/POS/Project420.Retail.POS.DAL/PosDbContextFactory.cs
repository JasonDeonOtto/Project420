using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Retail.POS.DAL;

/// <summary>
/// Design-time factory for creating PosDbContext instances.
/// Used by EF Core tools for migrations and scaffolding.
/// </summary>
public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    /// <summary>
    /// Creates a new instance of PosDbContext with design-time configuration.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Configured PosDbContext instance</returns>
    public PosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();

        // Use SQL Server for development/migrations
        // Connection string will be overridden by appsettings.json at runtime
        // IMPORTANT: Use same database as Management to share reference tables (Debtors, Products, Pricelists)
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Project420_Management;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new PosDbContext(optionsBuilder.Options);
    }
}
