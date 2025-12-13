using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique batch numbers following the 16-digit format.
/// Format: SSTTYYYYMMDDNNNN
/// </summary>
/// <remarks>
/// Batch Number Format (16 digits): SSTTYYYYMMDDNNNN
/// - SS: Site ID (01-99)
/// - TT: Batch Type code (10=Production, 20=Transfer, etc.)
/// - YYYYMMDD: Batch date
/// - NNNN: Daily sequence per site/type (0001-9999)
///
/// Example: 0110202512060001
/// - Site 01, Production (10), Dec 6 2025, batch #1
///
/// Key Features:
/// - Site isolation (each site has independent sequences)
/// - Type visible (instant identification of batch purpose)
/// - Date embedded (supports FIFO/FEFO inventory management)
/// - Daily reset per type/site (keeps numbers manageable)
/// - 16 digits (barcode-friendly, all numeric)
///
/// Cannabis Compliance:
/// - SAHPRA seed-to-sale traceability
/// - Unique batch identification for audit trail
/// - Supports recall management and quality control
/// </remarks>
public interface IBatchNumberGeneratorService
{
    /// <summary>
    /// Generates a new unique batch number for the specified parameters.
    /// </summary>
    /// <param name="siteId">Site ID (1-99, will be zero-padded to 2 digits)</param>
    /// <param name="batchType">Type of batch (determines the TT component)</param>
    /// <param name="batchDate">Date of the batch (defaults to today if not specified)</param>
    /// <param name="requestedBy">User requesting the batch number (for audit trail)</param>
    /// <returns>16-digit batch number string (SSTTYYYYMMDDNNNN)</returns>
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
    /// <returns>True if valid 16-digit format, false otherwise</returns>
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
/// Parsed components of a batch number.
/// </summary>
public record BatchNumberComponents
{
    /// <summary>Site ID (1-99)</summary>
    public int SiteId { get; init; }

    /// <summary>Batch type (Production, Transfer, etc.)</summary>
    public BatchType BatchType { get; init; }

    /// <summary>Batch date</summary>
    public DateTime BatchDate { get; init; }

    /// <summary>Daily sequence number (1-9999)</summary>
    public int Sequence { get; init; }

    /// <summary>Original batch number string</summary>
    public string OriginalBatchNumber { get; init; } = string.Empty;
}
