using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Cultivation.DAL;

/// <summary>
/// Design-time factory for creating CultivationDbContext instances.
/// Used by EF Core tools for migrations and scaffolding.
/// </summary>
public class CultivationDbContextFactory : IDesignTimeDbContextFactory<CultivationDbContext>
{
    public CultivationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CultivationDbContext>();

        // Use SQL Server for development/migrations
        optionsBuilder.UseSqlServer("Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new CultivationDbContext(optionsBuilder.Options);
    }
}
