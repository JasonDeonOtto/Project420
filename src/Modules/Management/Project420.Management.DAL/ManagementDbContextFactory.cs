using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Management.DAL;

/// <summary>
/// Design-time factory for creating ManagementDbContext instances.
/// Used by EF Core tools for migrations and scaffolding.
/// </summary>
public class ManagementDbContextFactory : IDesignTimeDbContextFactory<ManagementDbContext>
{
    /// <summary>
    /// Creates a new instance of ManagementDbContext with design-time configuration.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Configured ManagementDbContext instance</returns>
    public ManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ManagementDbContext>();

        // Use SQL Server for development/migrations
        // Connection string will be overridden by appsettings.json at runtime
        optionsBuilder.UseSqlServer("Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new ManagementDbContext(optionsBuilder.Options);
    }
}
