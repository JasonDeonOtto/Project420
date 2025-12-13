using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project420.Shared.Database.Repositories;
using Project420.Shared.Database.Services;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Database.Extensions;

/// <summary>
/// Extension methods for registering shared database services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SharedDbContext and shared infrastructure repositories.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">Connection string for the shared database</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Registered Services:
    /// - SharedDbContext (Scoped) - Database context for shared infrastructure tables
    /// - ITransactionNumberRepository (Scoped) - Transaction number sequence repository
    ///
    /// Database Tables (SharedDbContext → Project420_Shared):
    /// - ErrorLogs - Centralized error logging
    /// - AuditLogs - POPIA compliance audit trail
    /// - StationConnections - Multi-tenant station routing
    /// - TransactionNumberSequences - Persistent transaction numbering
    ///
    /// NOTE: Business data services (IMovementService, IBatchNumberGeneratorService, ISerialNumberGeneratorService)
    /// now use IBusinessDbContext interface and should be registered separately with PosDbContext.
    /// See Program.cs for proper registration pattern.
    ///
    /// Usage:
    /// <code>
    /// // In Program.cs or Startup.cs:
    /// var sharedConnection = builder.Configuration.GetConnectionString("SharedConnection");
    /// builder.Services.AddSharedDatabaseServices(sharedConnection);
    ///
    /// // Also register business data services with IBusinessDbContext:
    /// builder.Services.AddScoped&lt;IBusinessDbContext&gt;(sp => sp.GetRequiredService&lt;PosDbContext&gt;());
    /// builder.Services.AddScoped&lt;IMovementService, MovementService&gt;();
    /// builder.Services.AddScoped&lt;IBatchNumberGeneratorService, BatchNumberGeneratorService&gt;();
    /// builder.Services.AddScoped&lt;ISerialNumberGeneratorService, SerialNumberGeneratorService&gt;();
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

        // Register SharedDbContext (infrastructure tables only)
        services.AddDbContext<SharedDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Infrastructure Repositories
        services.AddScoped<ITransactionNumberRepository, TransactionNumberRepository>();

        // NOTE: Business data services (MovementService, BatchNumberGeneratorService, SerialNumberGeneratorService)
        // require IBusinessDbContext and should be registered separately in Program.cs with PosDbContext.

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

        // Register Movement Architecture Services (Option A)
        services.AddScoped<IMovementService, MovementService>();

        return services;
    }
}
