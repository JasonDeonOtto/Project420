using Microsoft.EntityFrameworkCore;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Database.Repositories;

/// <summary>
/// Repository implementation for atomic transaction number sequence operations.
/// Uses database transactions and row-level locking for thread safety.
/// </summary>
/// <remarks>
/// Thread Safety Strategy:
/// - Uses explicit database transactions for atomic operations
/// - Row-level locking via FromSqlRaw with ROWLOCK, UPDLOCK hints (SQL Server)
/// - Ensures sequential consistency across multiple application instances
///
/// Performance Considerations:
/// - Locks are held for minimal duration (single row, single operation)
/// - No lock contention between different transaction types
/// - Indexed lookups for fast sequence retrieval
///
/// Database Support:
/// - Currently optimized for SQL Server (WITH (ROWLOCK, UPDLOCK))
/// - For PostgreSQL, use SELECT FOR UPDATE
/// - For MySQL, use SELECT FOR UPDATE
/// </remarks>
public class TransactionNumberRepository : ITransactionNumberRepository
{
    private readonly SharedDbContext _context;

    public TransactionNumberRepository(SharedDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<long> GetNextSequenceAsync(TransactionTypeCode transactionType, string updatedBy)
    {
        // Use explicit transaction for atomicity
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Lock the row for update (prevents concurrent access)
            // SQL Server: WITH (ROWLOCK, UPDLOCK)
            // This ensures no other transaction can read/modify this row until we're done
            var sequence = await _context.TransactionNumberSequences
                .FromSqlRaw(
                    "SELECT * FROM TransactionNumberSequences WITH (ROWLOCK, UPDLOCK) WHERE TransactionType = {0}",
                    (int)transactionType
                )
                .FirstOrDefaultAsync();

            if (sequence == null)
            {
                throw new InvalidOperationException(
                    $"Transaction number sequence for type '{transactionType}' does not exist. " +
                    "Please create the sequence before generating numbers."
                );
            }

            if (!sequence.IsActive)
            {
                throw new InvalidOperationException(
                    $"Transaction number sequence for type '{transactionType}' is deactivated. " +
                    "Please reactivate the sequence or create a new one."
                );
            }

            // Atomic increment
            sequence.CurrentSequence++;
            sequence.LastGeneratedAt = DateTime.UtcNow;
            sequence.LastUpdatedAt = DateTime.UtcNow;
            sequence.LastUpdatedBy = updatedBy;

            // Save changes within transaction
            await _context.SaveChangesAsync();

            // Commit transaction (releases lock)
            await transaction.CommitAsync();

            return sequence.CurrentSequence;
        }
        catch
        {
            // Rollback on any error (releases lock)
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TransactionNumberSequence?> GetSequenceAsync(TransactionTypeCode transactionType)
    {
        return await _context.TransactionNumberSequences
            .AsNoTracking() // Read-only query (no change tracking overhead)
            .FirstOrDefaultAsync(s => s.TransactionType == transactionType);
    }

    /// <inheritdoc />
    public async Task<List<TransactionNumberSequence>> GetAllActiveSequencesAsync()
    {
        return await _context.TransactionNumberSequences
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.TransactionType)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TransactionNumberSequence> CreateSequenceAsync(TransactionNumberSequence sequence)
    {
        // Validation
        if (sequence == null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        // Check if sequence already exists
        var existing = await _context.TransactionNumberSequences
            .AnyAsync(s => s.TransactionType == sequence.TransactionType);

        if (existing)
        {
            throw new InvalidOperationException(
                $"A sequence for transaction type '{sequence.TransactionType}' already exists. " +
                "Each transaction type can only have one sequence."
            );
        }

        // Validate prefix format
        if (!sequence.IsValidPrefix())
        {
            throw new ArgumentException(
                $"Invalid prefix '{sequence.Prefix}'. Prefix must be letters-only (A-Z) OR numbers-only (0-9), not mixed.",
                nameof(sequence)
            );
        }

        // Validate sequence range
        if (sequence.StartingSequence < 1)
        {
            throw new ArgumentException(
                "StartingSequence must be at least 1.",
                nameof(sequence)
            );
        }

        if (sequence.CurrentSequence < 0)
        {
            sequence.CurrentSequence = 0; // Auto-fix: start before first number
        }

        if (!sequence.IsValidSequenceRange())
        {
            throw new ArgumentException(
                $"CurrentSequence ({sequence.CurrentSequence}) cannot be less than StartingSequence ({sequence.StartingSequence}).",
                nameof(sequence)
            );
        }

        // Set audit fields
        sequence.CreatedAt = DateTime.UtcNow;

        // Add to database
        await _context.TransactionNumberSequences.AddAsync(sequence);
        await _context.SaveChangesAsync();

        return sequence;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateSequenceAsync(TransactionNumberSequence sequence)
    {
        if (sequence == null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        var existing = await _context.TransactionNumberSequences
            .FirstOrDefaultAsync(s => s.SequenceId == sequence.SequenceId);

        if (existing == null)
        {
            return false; // Not found
        }

        // Validate prefix if changed
        if (!sequence.IsValidPrefix())
        {
            throw new ArgumentException(
                $"Invalid prefix '{sequence.Prefix}'. Prefix must be letters-only (A-Z) OR numbers-only (0-9), not mixed.",
                nameof(sequence)
            );
        }

        // Prevent decreasing CurrentSequence (data integrity)
        if (sequence.CurrentSequence < existing.CurrentSequence)
        {
            throw new InvalidOperationException(
                $"Cannot decrease CurrentSequence from {existing.CurrentSequence} to {sequence.CurrentSequence}. " +
                "This would create duplicate transaction numbers."
            );
        }

        // Update fields
        existing.Prefix = sequence.Prefix;
        existing.PaddingLength = sequence.PaddingLength;
        existing.Description = sequence.Description;
        existing.IsActive = sequence.IsActive;
        existing.LastUpdatedAt = DateTime.UtcNow;
        existing.LastUpdatedBy = sequence.LastUpdatedBy;

        // Note: StartingSequence and CurrentSequence are intentionally not updated
        // to prevent breaking existing number sequences

        await _context.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateSequenceAsync(TransactionTypeCode transactionType, string deactivatedBy)
    {
        var sequence = await _context.TransactionNumberSequences
            .FirstOrDefaultAsync(s => s.TransactionType == transactionType);

        if (sequence == null)
        {
            return false; // Not found
        }

        sequence.IsActive = false;
        sequence.LastUpdatedAt = DateTime.UtcNow;
        sequence.LastUpdatedBy = deactivatedBy;

        await _context.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReactivateSequenceAsync(TransactionTypeCode transactionType, string reactivatedBy)
    {
        var sequence = await _context.TransactionNumberSequences
            .FirstOrDefaultAsync(s => s.TransactionType == transactionType);

        if (sequence == null)
        {
            return false; // Not found
        }

        sequence.IsActive = true;
        sequence.LastUpdatedAt = DateTime.UtcNow;
        sequence.LastUpdatedBy = reactivatedBy;

        await _context.SaveChangesAsync();

        return true;
    }
}
