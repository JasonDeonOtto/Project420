using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Project420.Shared.Database.Repositories;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Database.Extensions;

/// <summary>
/// Extension methods for registering shared database services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SharedDbContext and all shared database repositories.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">Connection string for the shared database</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Registered Services:
    /// - SharedDbContext (Scoped) - Database context for shared tables
    /// - ITransactionNumberRepository (Scoped) - Transaction number sequence repository
    ///
    /// Database Tables:
    /// - ErrorLogs - Centralized error logging
    /// - AuditLogs - POPIA compliance audit trail
    /// - StationConnections - Multi-tenant station routing
    /// - TransactionNumberSequences - Persistent transaction numbering
    ///
    /// Usage:
    /// <code>
    /// // In Program.cs or Startup.cs:
    /// var connectionString = builder.Configuration.GetConnectionString("SharedConnection");
    /// builder.Services.AddSharedDatabaseServices(connectionString);
    /// </code>
    ///
    /// Prerequisites:
    /// - SQL Server connection string configured in appsettings.json
    /// - Database migrations applied (dotnet ef database update)
    /// </remarks>
    public static IServiceCollection AddSharedDatabaseServices(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Connection string cannot be null or empty",
                nameof(connectionString)
            );
        }

        // Register SharedDbContext
        services.AddDbContext<SharedDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Repositories
        services.AddScoped<ITransactionNumberRepository, TransactionNumberRepository>();

        return services;
    }

    /// <summary>
    /// Registers the SharedDbContext and repositories with DbContextOptions configuration callback.
    /// Allows for advanced configuration (connection resiliency, query logging, etc.).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="optionsAction">Configuration callback for DbContextOptions</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Advanced Usage:
    /// <code>
    /// builder.Services.AddSharedDatabaseServices(options =>
    /// {
    ///     options.UseSqlServer(
    ///         connectionString,
    ///         sqlOptions =>
    ///         {
    ///             sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    ///             sqlOptions.CommandTimeout(30);
    ///         }
    ///     );
    ///     options.LogTo(Console.WriteLine, LogLevel.Information);
    ///     options.EnableSensitiveDataLogging(isDevelopment);
    /// });
    /// </code>
    /// </remarks>
    public static IServiceCollection AddSharedDatabaseServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        if (optionsAction == null)
        {
            throw new ArgumentNullException(nameof(optionsAction));
        }

        // Register SharedDbContext with configuration callback
        services.AddDbContext<SharedDbContext>(optionsAction);

        // Register Repositories
        services.AddScoped<ITransactionNumberRepository, TransactionNumberRepository>();

        return services;
    }
}
