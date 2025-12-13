using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Project420.Shared.Core.Entities;

namespace Project420.Shared.Core.Abstractions;

/// <summary>
/// Interface for business database context containing unified transaction and movement tables.
/// </summary>
/// <remarks>
/// This interface allows services in Shared.Database to access business data tables
/// without creating a circular dependency on module-specific DbContexts like PosDbContext.
///
/// Architecture:
/// - Interface defined in Shared.Core (no dependencies)
/// - Implemented by PosDbContext in POS.DAL (has all entity references)
/// - Services in Shared.Database depend on this interface, not concrete DbContext
/// - DI container resolves interface to PosDbContext at runtime
///
/// Tables included:
/// - TransactionDetails: Unified transaction line items (all modules)
/// - Movements: Inventory movement ledger (SOH source of truth)
/// - BatchNumberSequences: Batch number generation tracking
/// - SerialNumberSequences: Serial number generation tracking
/// - SerialNumbers: Master serial number records
/// </remarks>
public interface IBusinessDbContext : IDisposable
{
    /// <summary>
    /// Provides access to database-related information and operations.
    /// Required for transaction management and database operations.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Unified transaction details table - All transaction line items.
    /// </summary>
    DbSet<TransactionDetail> TransactionDetails { get; }

    /// <summary>
    /// Movement ledger table - Source of truth for SOH calculations.
    /// </summary>
    DbSet<Movement> Movements { get; }

    /// <summary>
    /// Batch number sequences table - Tracks daily sequences per site/type/date.
    /// </summary>
    DbSet<BatchNumberSequence> BatchNumberSequences { get; }

    /// <summary>
    /// Serial number sequences table - Tracks unit and daily sequences.
    /// </summary>
    DbSet<SerialNumberSequence> SerialNumberSequences { get; }

    /// <summary>
    /// Serial numbers table - Master record of all generated serial numbers.
    /// </summary>
    DbSet<SerialNumber> SerialNumbers { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
