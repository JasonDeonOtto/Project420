using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Tracks serial number sequences for unit-level serial generation.
/// Maintains both unit sequences (per batch) and daily sequences (for short SNs).
/// </summary>
/// <remarks>
/// Two types of sequences tracked:
///
/// 1. Unit Sequence (per batch):
///    - Used for UUUUU component of full serial number
///    - Resets for each new batch
///    - Key: SiteId + BatchType + BatchDate + BatchSequence
///
/// 2. Daily Sequence (for short SN):
///    - Used for NNNNN component of short serial number
///    - Resets daily per site
///    - Key: SiteId + ProductionDate
///
/// Thread Safety:
/// - Uses database-level locking for atomic increments
/// - Safe for multi-instance deployments
/// </remarks>
public class SerialNumberSequence : AuditableEntity
{
    /// <summary>
    /// Site ID (1-99). Each site has independent sequences.
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Type of sequence: "Unit" for per-batch unit sequences, "Daily" for short SN sequences.
    /// </summary>
    public string SequenceType { get; set; } = "Daily";

    /// <summary>
    /// Production/generation date for this sequence. Sequences reset daily.
    /// </summary>
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// Batch type (only applicable for Unit sequences).
    /// </summary>
    public BatchType? BatchType { get; set; }

    /// <summary>
    /// Batch sequence (only applicable for Unit sequences).
    /// </summary>
    public int? BatchSequence { get; set; }

    /// <summary>
    /// Current sequence number (last generated). Next serial uses CurrentSequence + 1.
    /// </summary>
    public int CurrentSequence { get; set; }

    /// <summary>
    /// Maximum sequence allowed (default 99999 for 5-digit format).
    /// </summary>
    public int MaxSequence { get; set; } = 99999;

    /// <summary>
    /// Timestamp of last serial number generation.
    /// </summary>
    public DateTime? LastGeneratedAt { get; set; }

    /// <summary>
    /// User who generated the last serial number.
    /// </summary>
    public string? LastGeneratedBy { get; set; }
}
