using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Represents a database-backed sequence for generating unique transaction numbers.
/// Ensures persistent, continuous numbering across application restarts.
/// </summary>
/// <remarks>
/// Design Requirements:
/// - No daily resets (continuous numbering)
/// - User-defined prefixes (letters-only OR numbers-only)
/// - Database-backed for production persistence
/// - Thread-safe increment operations
/// - Support for different numbering formats (invoices, batches, serial numbers)
///
/// Transaction Number Formats:
/// 1. Invoice/Credit/Retail/Refund: {UserPrefix}-{Sequence}
///    Examples: INV-00001, 1-00001, CRN-00123
///
/// 2. Production Batches: YYYYMMDD-{BatchType}-{Sequence}
///    Examples: 20251206-FLOWER-001, 20251206-EXTRACT-015
///
/// 3. Serial Numbers: SN-{STRAIN}-{BATCH}-{Sequence}
///    Examples: SN-BLUEDREAM-B001-00001
///
/// Validation Rules:
/// - Prefix must be letters-only (A-Z) OR numbers-only (0-9)
/// - Prefix max length: 10 characters
/// - Sequence starts from user-defined value (default 1)
/// - Current sequence must be >= starting sequence
///
/// POPIA/Compliance:
/// - Audit trail for all sequence updates
/// - Immutable historical transaction numbers
/// - 7-year retention for transaction traceability
/// </remarks>
public class TransactionNumberSequence
{
    /// <summary>
    /// Unique identifier for this sequence
    /// </summary>
    [Key]
    public int SequenceId { get; set; }

    /// <summary>
    /// Transaction type code this sequence is for
    /// </summary>
    /// <remarks>
    /// Examples: SALE, INV, CRN, GRV, ADJ
    /// Each type can have its own numbering sequence
    /// </remarks>
    [Required]
    public TransactionTypeCode TransactionType { get; set; }

    /// <summary>
    /// User-defined prefix for this transaction type
    /// </summary>
    /// <remarks>
    /// Validation:
    /// - Must be letters-only (A-Z, a-z) OR numbers-only (0-9)
    /// - Maximum 10 characters
    /// - Cannot mix letters and numbers
    /// - Case-insensitive (stored uppercase)
    ///
    /// Examples:
    /// - "INV" (invoice)
    /// - "1" (numeric code for invoices)
    /// - "CRN" (credit note)
    /// - "2" (numeric code for credits)
    /// - "" (empty = use TransactionType enum value)
    ///
    /// Default behavior:
    /// - If null/empty, uses TransactionType enum name (e.g., SALE, GRV)
    /// </remarks>
    [MaxLength(10)]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Current sequence number (last generated)
    /// </summary>
    /// <remarks>
    /// - Increments with each new transaction number
    /// - Never resets (continuous numbering)
    /// - Thread-safe increments via database transactions
    /// - Must be >= StartingSequence
    /// </remarks>
    [Required]
    public long CurrentSequence { get; set; } = 0;

    /// <summary>
    /// Starting sequence number (user-defined)
    /// </summary>
    /// <remarks>
    /// Allows users to start numbering from a specific value.
    /// Examples:
    /// - Start invoices at 1000 (legal requirement)
    /// - Start at 1 for new businesses
    /// - Continue from previous system (migration)
    ///
    /// Default: 1
    /// </remarks>
    [Required]
    public long StartingSequence { get; set; } = 1;

    /// <summary>
    /// Sequence number padding length (leading zeros)
    /// </summary>
    /// <remarks>
    /// Determines how many digits to display.
    /// Examples:
    /// - 3: 001, 002, 999
    /// - 5: 00001, 00123, 99999
    /// - 6: 000001 (default for serial numbers)
    ///
    /// Default: 5 (standard for invoices/transactions)
    /// </remarks>
    [Required]
    [Range(3, 10)]
    public int PaddingLength { get; set; } = 5;

    /// <summary>
    /// Optional description for this sequence
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "Main store invoices"
    /// - "Retail cash sales"
    /// - "Credit notes for returns"
    /// </remarks>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this sequence is active
    /// </summary>
    /// <remarks>
    /// Inactive sequences cannot generate new numbers.
    /// Use for deprecating old numbering systems.
    /// </remarks>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this sequence was created (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this sequence
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = "SYSTEM";

    /// <summary>
    /// When this sequence was last updated (UTC)
    /// </summary>
    public DateTime? LastUpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this sequence
    /// </summary>
    [MaxLength(100)]
    public string? LastUpdatedBy { get; set; }

    /// <summary>
    /// When the last transaction number was generated
    /// </summary>
    /// <remarks>
    /// Useful for auditing and identifying inactive sequences
    /// </remarks>
    public DateTime? LastGeneratedAt { get; set; }

    // ========================================
    // VALIDATION METHODS
    // ========================================

    /// <summary>
    /// Validates the prefix format (letters-only OR numbers-only)
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidPrefix()
    {
        // Empty prefix is valid (uses TransactionType enum value)
        if (string.IsNullOrWhiteSpace(Prefix))
            return true;

        // Check if letters-only (A-Z, a-z)
        bool isLettersOnly = Regex.IsMatch(Prefix, @"^[A-Za-z]+$");

        // Check if numbers-only (0-9)
        bool isNumbersOnly = Regex.IsMatch(Prefix, @"^[0-9]+$");

        // Must be one or the other (not mixed)
        return isLettersOnly || isNumbersOnly;
    }

    /// <summary>
    /// Gets the effective prefix to use for transaction number generation
    /// </summary>
    /// <returns>Uppercase prefix (user-defined or TransactionType enum value)</returns>
    public string GetEffectivePrefix()
    {
        if (string.IsNullOrWhiteSpace(Prefix))
        {
            // Use TransactionType enum name if no prefix defined
            return TransactionType.ToString().ToUpperInvariant();
        }

        return Prefix.ToUpperInvariant();
    }

    /// <summary>
    /// Validates that CurrentSequence is >= StartingSequence
    /// </summary>
    public bool IsValidSequenceRange()
    {
        return CurrentSequence >= StartingSequence;
    }

    /// <summary>
    /// Formats a sequence number with leading zeros
    /// </summary>
    /// <param name="sequenceNumber">The sequence number to format</param>
    /// <returns>Formatted sequence with padding (e.g., 00001, 00123)</returns>
    public string FormatSequence(long sequenceNumber)
    {
        // Create format string: D3 (3 digits), D5 (5 digits), etc.
        string format = $"D{PaddingLength}";
        return sequenceNumber.ToString(format);
    }

    /// <summary>
    /// Generates the next transaction number (without incrementing the database)
    /// </summary>
    /// <remarks>
    /// This is a preview method. Actual generation happens via TransactionNumberGeneratorService
    /// which handles database increment atomicity.
    /// </remarks>
    public string PreviewNextNumber()
    {
        long nextSequence = CurrentSequence + 1;
        string prefix = GetEffectivePrefix();
        string formattedSequence = FormatSequence(nextSequence);

        return $"{prefix}-{formattedSequence}";
    }
}
