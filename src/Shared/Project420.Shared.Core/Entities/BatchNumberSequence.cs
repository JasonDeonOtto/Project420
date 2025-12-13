using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Tracks batch number sequences per site/type/date combination.
/// Ensures unique, sequential batch numbers within each category.
/// </summary>
/// <remarks>
/// Purpose:
/// - Maintains daily sequence counters for batch number generation
/// - Each combination of SiteId + BatchType + Date has its own sequence
/// - Supports multi-site operations with isolated numbering
///
/// Thread Safety:
/// - Uses database-level locking for atomic increments
/// - Safe for multi-instance deployments
///
/// Batch Number Format: SSTTYYYYMMDDNNNN (16 digits)
/// - This entity tracks the NNNN (sequence) component
/// </remarks>
public class BatchNumberSequence : AuditableEntity
{
    /// <summary>
    /// Site ID (1-99). Each site has independent sequences.
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Type of batch (Production, Transfer, StockTake, etc.)
    /// </summary>
    public BatchType BatchType { get; set; }

    /// <summary>
    /// Date for which this sequence applies. Sequences reset daily.
    /// </summary>
    public DateTime BatchDate { get; set; }

    /// <summary>
    /// Current sequence number (last generated). Next batch uses CurrentSequence + 1.
    /// </summary>
    public int CurrentSequence { get; set; }

    /// <summary>
    /// Maximum sequence allowed per day (default 9999 for 4-digit format).
    /// </summary>
    public int MaxSequence { get; set; } = 9999;

    /// <summary>
    /// Timestamp of last batch number generation.
    /// </summary>
    public DateTime? LastGeneratedAt { get; set; }

    /// <summary>
    /// User who generated the last batch number.
    /// </summary>
    public string? LastGeneratedBy { get; set; }
}
