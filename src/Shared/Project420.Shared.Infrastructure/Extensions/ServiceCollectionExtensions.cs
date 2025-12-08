using Microsoft.Extensions.DependencyInjection;
using Project420.Shared.Infrastructure.Interfaces;
using Project420.Shared.Infrastructure.Services;

namespace Project420.Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering shared infrastructure services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all shared infrastructure services (VAT calculation, audit logging, transaction numbering).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="currentUser">Current user for transaction number audit trail (optional, defaults to "SYSTEM")</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Registered Services:
    /// - IVATCalculationService (Singleton) - Universal VAT calculation logic
    /// - IAuditLogService (Scoped) - POPIA/Cannabis Act/SARS compliance audit logging
    /// - ITransactionNumberGeneratorService (Scoped) - Database-backed transaction numbering
    ///
    /// Usage:
    /// <code>
    /// // In Program.cs or Startup.cs:
    /// builder.Services.AddSharedInfrastructureServices();
    ///
    /// // Or with custom user:
    /// builder.Services.AddSharedInfrastructureServices("user@example.com");
    /// </code>
    ///
    /// Prerequisites:
    /// - Shared.Database services must also be registered (AddSharedDatabaseServices)
    /// - Connection string for SharedDbContext must be configured
    /// </remarks>
    public static IServiceCollection AddSharedInfrastructureServices(
        this IServiceCollection services,
        string currentUser = "SYSTEM")
    {
        // VAT Calculation Service (Singleton - stateless, universal logic)
        services.AddSingleton<IVATCalculationService, VATCalculationService>();

        // Audit Log Service (Scoped - per request/transaction)
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Transaction Number Generator Service (Scoped - per request)
        // Inject current user for audit trail
        services.AddScoped<ITransactionNumberGeneratorService>(sp =>
        {
            var repository = sp.GetRequiredService<ITransactionNumberRepository>();
            return new TransactionNumberGeneratorService(repository, currentUser);
        });

        return services;
    }
}
