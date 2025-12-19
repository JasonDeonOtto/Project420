using System.Globalization;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique batch numbers following the 12-digit week-based format.
/// Format: SSTTYYYWWNNNN
/// </summary>
/// <remarks>
/// Batch Number Format (12 digits): SSTTYYYWWNNNN
/// - SS: Site ID (01-99)
/// - TT: Batch Type code (10=Production, 20=GRV, 30=Transfer, etc.)
/// - YY: Year (2-digit, e.g., 25 for 2025)
/// - WW: ISO week number (01-53)
/// - NNNN: Weekly sequence per site/type (0001-9999)
///
/// Example: 011025510001
/// - Site 01, Production (10), 2025, Week 51, batch #1
///
/// Visual Identification:
/// ┌──────────────────────────────────────┐
/// │ 01 10 25 51 0001                     │
/// │  │  │  │  │  └── Batch #1 this week  │
/// │  │  │  │  └───── Week 51             │
/// │  │  │  └──────── 2025                │
/// │  │  └─────────── Production batch    │
/// │  └────────────── Site 01             │
/// └──────────────────────────────────────┘
///
/// Key Features:
/// - Site isolation (each site has independent sequences)
/// - Type visible (instant identification of batch purpose)
/// - Week-based (aligns with production cycles)
/// - Weekly reset per type/site (keeps sequences manageable)
/// - 12 digits (compact, barcode-friendly, all numeric)
/// - Serial numbers embed batch reference for traceability
///
/// Cannabis Compliance:
/// - SAHPRA seed-to-sale traceability
/// - Unique batch identification for audit trail
/// - Supports recall management and quality control
/// - Week granularity sufficient for compliance reporting
/// </remarks>
public interface IBatchNumberGeneratorService
{
    /// <summary>
    /// Generates a new unique batch number for the specified parameters.
    /// </summary>
    /// <param name="siteId">Site ID (1-99, will be zero-padded to 2 digits)</param>
    /// <param name="batchType">Type of batch (determines the TT component)</param>
    /// <param name="batchDate">Date of the batch (defaults to today if not specified). Used to derive week number.</param>
    /// <param name="requestedBy">User requesting the batch number (for audit trail)</param>
    /// <returns>12-digit batch number string (SSTTYYYWWNNNN)</returns>
    /// <exception cref="ArgumentOutOfRangeException">If siteId is not between 1-99</exception>
    Task<string> GenerateBatchNumberAsync(
        int siteId,
        BatchType batchType,
        DateTime? batchDate = null,
        string? requestedBy = null);

    /// <summary>
    /// Validates a batch number string for correct format.
    /// </summary>
    /// <param name="batchNumber">The batch number to validate</param>
    /// <returns>True if valid 12-digit format (SSTTYYYWWNNNN), false otherwise</returns>
    bool ValidateBatchNumber(string batchNumber);

    /// <summary>
    /// Parses a batch number to extract its components.
    /// </summary>
    /// <param name="batchNumber">The 16-digit batch number to parse</param>
    /// <returns>Parsed components (SiteId, BatchType, Date, Sequence)</returns>
    /// <exception cref="ArgumentException">If batch number format is invalid</exception>
    BatchNumberComponents ParseBatchNumber(string batchNumber);

    /// <summary>
    /// Gets the current sequence number for a site/type/date combination (without incrementing).
    /// </summary>
    /// <param name="siteId">Site ID</param>
    /// <param name="batchType">Batch type</param>
    /// <param name="batchDate">Batch date</param>
    /// <returns>Current sequence number (0 if none exist yet)</returns>
    Task<int> GetCurrentSequenceAsync(int siteId, BatchType batchType, DateTime batchDate);

    /// <summary>
    /// Checks if a batch number already exists.
    /// </summary>
    /// <param name="batchNumber">The batch number to check</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> BatchNumberExistsAsync(string batchNumber);
}

/// <summary>
/// Parsed components of a batch number (12-digit format: SSTTYYYWWNNNN).
/// </summary>
public record BatchNumberComponents
{
    /// <summary>Site ID (1-99)</summary>
    public int SiteId { get; init; }

    /// <summary>Batch type (Production, GRV, Transfer, etc.)</summary>
    public BatchType BatchType { get; init; }

    /// <summary>Year (2-digit, e.g., 25 for 2025)</summary>
    public int Year { get; init; }

    /// <summary>ISO week number (1-53)</summary>
    public int Week { get; init; }

    /// <summary>Weekly sequence number (1-9999)</summary>
    public int Sequence { get; init; }

    /// <summary>Original batch number string</summary>
    public string OriginalBatchNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the approximate first day of the batch week.
    /// </summary>
    public DateTime ApproximateDate => ISOWeek.ToDateTime(2000 + Year, Week, DayOfWeek.Monday);
}
