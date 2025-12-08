using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Infrastructure.Interfaces;

/// <summary>
/// Repository interface for atomic transaction number sequence operations.
/// Provides thread-safe database access for generating unique transaction numbers.
/// </summary>
/// <remarks>
/// Purpose:
/// - Atomic increment of sequence numbers (thread-safe via database transactions)
/// - Retrieve sequence configuration for different transaction types
/// - Manage sequence lifecycle (create, update, deactivate)
///
/// Thread Safety:
/// - Database-level locking ensures thread safety across multiple instances/servers
/// - Uses transactions to prevent race conditions during increment operations
///
/// Design Pattern:
/// - Repository pattern for data access abstraction
/// - Supports future implementation swaps (SQL Server, PostgreSQL, etc.)
/// </remarks>
public interface ITransactionNumberRepository
{
    /// <summary>
    /// Gets the next sequence number for a transaction type (atomic increment).
    /// </summary>
    /// <param name="transactionType">The transaction type to generate for</param>
    /// <param name="updatedBy">Who is requesting the number (for audit)</param>
    /// <returns>The next sequence number (thread-safe, atomic)</returns>
    /// <remarks>
    /// This method performs an atomic increment using database transactions:
    /// 1. Locks the sequence row (prevents concurrent access)
    /// 2. Increments CurrentSequence
    /// 3. Updates LastGeneratedAt and LastUpdatedBy
    /// 4. Returns the new sequence number
    ///
    /// Thread Safety: Uses database row-level locking (SELECT FOR UPDATE or equivalent)
    /// to prevent race conditions in multi-threaded/multi-instance scenarios.
    /// </remarks>
    Task<long> GetNextSequenceAsync(TransactionTypeCode transactionType, string updatedBy);

    /// <summary>
    /// Gets the sequence configuration for a transaction type (read-only).
    /// </summary>
    /// <param name="transactionType">The transaction type to retrieve</param>
    /// <returns>The sequence configuration, or null if not found</returns>
    Task<TransactionNumberSequence?> GetSequenceAsync(TransactionTypeCode transactionType);

    /// <summary>
    /// Gets all active transaction number sequences.
    /// </summary>
    /// <returns>List of active sequences</returns>
    Task<List<TransactionNumberSequence>> GetAllActiveSequencesAsync();

    /// <summary>
    /// Creates a new transaction number sequence.
    /// </summary>
    /// <param name="sequence">The sequence configuration to create</param>
    /// <returns>The created sequence with ID assigned</returns>
    /// <remarks>
    /// Validation:
    /// - TransactionType must be unique
    /// - Prefix must be valid (letters-only OR numbers-only)
    /// - StartingSequence must be >= 1
    /// - PaddingLength must be between 3 and 10
    /// </remarks>
    Task<TransactionNumberSequence> CreateSequenceAsync(TransactionNumberSequence sequence);

    /// <summary>
    /// Updates an existing transaction number sequence configuration.
    /// </summary>
    /// <param name="sequence">The updated sequence configuration</param>
    /// <returns>True if updated, false if not found</returns>
    /// <remarks>
    /// Warning: Updating prefix or padding while sequences are in use may cause
    /// inconsistent transaction number formats. Consider creating a new sequence instead.
    ///
    /// CurrentSequence cannot be decreased (data integrity protection).
    /// </remarks>
    Task<bool> UpdateSequenceAsync(TransactionNumberSequence sequence);

    /// <summary>
    /// Deactivates a transaction number sequence (soft delete).
    /// </summary>
    /// <param name="transactionType">The transaction type to deactivate</param>
    /// <param name="deactivatedBy">Who deactivated the sequence</param>
    /// <returns>True if deactivated, false if not found</returns>
    /// <remarks>
    /// Deactivated sequences cannot generate new numbers but existing transactions
    /// retain their numbers. Use for deprecating old numbering systems.
    /// </remarks>
    Task<bool> DeactivateSequenceAsync(TransactionTypeCode transactionType, string deactivatedBy);

    /// <summary>
    /// Reactivates a previously deactivated sequence.
    /// </summary>
    /// <param name="transactionType">The transaction type to reactivate</param>
    /// <param name="reactivatedBy">Who reactivated the sequence</param>
    /// <returns>True if reactivated, false if not found</returns>
    Task<bool> ReactivateSequenceAsync(TransactionTypeCode transactionType, string reactivatedBy);
}
