using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project420.Shared.Database;

/// <summary>
/// Design-time factory for creating SharedDbContext instances during migrations.
/// This is required for EF Core Tools (migrations, database update, etc.) to work properly.
/// </summary>
/// <remarks>
/// This factory is ONLY used at design time (when running dotnet ef commands).
/// At runtime, the DbContext is created through dependency injection with
/// the actual connection string from appsettings.json.
///
/// The connection string here is a placeholder for migration generation.
/// Migrations are database-agnostic and work with any connection string.
/// </remarks>
public class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
{
    public SharedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SharedDbContext>();

        // Design-time connection string (for migrations only)
        // This matches the connection string in the seed data
        optionsBuilder.UseSqlServer(
            "Server=JASON\\SQLDEVED;Database=Project420_Shared;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
            b => b.MigrationsAssembly("Project420.Shared.Database")
        );

        return new SharedDbContext(optionsBuilder.Options);
    }
}
