using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique serial numbers in the 16-digit batch-linked format.
/// Format: TTYYYWWBBBBSSSSSS
/// </summary>
/// <remarks>
/// Serial Number Format (16 digits): TTYYYWWBBBBSSSSSS
/// - TT: Serial Type (2 digits - operation type code)
/// - YY: Year (2 digits, e.g., 25 for 2025)
/// - WW: ISO week number (01-53)
/// - BBBB: Parent batch sequence (4 digits - links to batch's NNNN)
/// - SSSSSS: Serial sequence within batch (000001-999999)
///
/// Example: 1025510001000001
/// - Production type (10), 2025, Week 51, from Batch 0001, Serial #1
///
/// Visual Identification:
/// ┌──────────────────────────────────────────────┐
/// │ 10 25 51 0001 000001                         │
/// │  │  │  │  │    └── Serial #1 from this batch │
/// │  │  │  │  └────── From Batch 0001            │
/// │  │  │  └───────── Week 51                    │
/// │  │  └──────────── 2025                       │
/// │  └─────────────── Production type            │
/// └──────────────────────────────────────────────┘
///
/// Serial Type Codes (TT):
/// - 10: Production (manufacturing output)
/// - 20: GRV (goods received from supplier)
/// - 30: Retail (subbed for retail sale)
/// - 40: Bucking (cannabis processing)
/// - 50: Transfer (inter-location)
/// - 60: Adjustment (stocktake variance)
///
/// Batch Linkage:
/// - BBBB matches the batch's sequence (NNNN from SSTTYYYWWNNNN)
/// - Combined with YYWW, uniquely identifies parent batch
/// - Full traceability: Serial → Batch → Production origin
///
/// Cannabis Compliance:
/// - SAHPRA unit-level traceability (seed-to-sale)
/// - Batch linkage enables recall management
/// - Week-based tracking aligns with production cycles
/// - All numeric (barcode-friendly)
/// </remarks>
public interface ISerialNumberGeneratorService
{
    /// <summary>
    /// Generates a serial number (16 digits) linked to a parent batch.
    /// </summary>
    /// <param name="siteId">Site ID (1-99) - for sequence isolation</param>
    /// <param name="serialType">Type of serial (determines TT component)</param>
    /// <param name="batchNumber">Parent batch number (12-digit SSTTYYYWWNNNN format) - extracts YYWW and BBBB</param>
    /// <param name="requestedBy">User requesting serial number (for audit)</param>
    /// <returns>Serial number generation result with SN and components</returns>
    /// <exception cref="ArgumentException">If batch number format is invalid</exception>
    Task<SerialNumberResult> GenerateSerialNumberAsync(
        int siteId,
        SerialType serialType,
        string batchNumber,
        string? requestedBy = null);

    /// <summary>
    /// Generates multiple serial numbers for a batch (bulk packaging).
    /// </summary>
    /// <param name="count">Number of serial numbers to generate (1-999999)</param>
    /// <param name="siteId">Site ID (1-99)</param>
    /// <param name="serialType">Type of serial</param>
    /// <param name="batchNumber">Parent batch number</param>
    /// <param name="requestedBy">User requesting serial numbers</param>
    /// <returns>List of serial number results</returns>
    Task<List<SerialNumberResult>> GenerateBulkSerialNumbersAsync(
        int count,
        int siteId,
        SerialType serialType,
        string batchNumber,
        string? requestedBy = null);

    /// <summary>
    /// Validates a serial number format (16 digits).
    /// </summary>
    /// <param name="serialNumber">The serial number to validate</param>
    /// <returns>True if valid 16-digit format, false otherwise</returns>
    bool ValidateSerialNumber(string serialNumber);

    /// <summary>
    /// Parses a serial number into its components.
    /// </summary>
    /// <param name="serialNumber">The 16-digit serial number to parse</param>
    /// <returns>Parsed components</returns>
    /// <exception cref="ArgumentException">If format is invalid</exception>
    SerialNumberComponents ParseSerialNumber(string serialNumber);

    /// <summary>
    /// Gets the serial type name from a serial type code.
    /// </summary>
    /// <param name="serialType">Serial type</param>
    /// <returns>Human-readable type name</returns>
    string GetSerialTypeName(SerialType serialType);

    /// <summary>
    /// Derives the parent batch number from a serial number.
    /// </summary>
    /// <param name="serialNumber">The 16-digit serial number</param>
    /// <param name="siteId">Site ID to reconstruct full batch number</param>
    /// <returns>12-digit parent batch number (SSTTYYYWWNNNN)</returns>
    /// <remarks>
    /// Note: This reconstructs the batch number but cannot determine the
    /// original batch type (TT in batch) from the serial alone. The serial's
    /// TT is the serial type, not the batch type. For full reconstruction,
    /// the batch type must be provided or looked up.
    /// </remarks>
    string DeriveParentBatchNumber(string serialNumber, int siteId, BatchType batchType);
}

/// <summary>
/// Serial type codes for unit-level tracking.
/// These codes are embedded in the serial number (TT component).
/// </summary>
public enum SerialType
{
    /// <summary>Production output (manufacturing)</summary>
    Production = 10,

    /// <summary>Goods Received Voucher (supplier receipt)</summary>
    GRV = 20,

    /// <summary>Retail sub (unit prepared for retail sale)</summary>
    Retail = 30,

    /// <summary>Bucking (cannabis processing step)</summary>
    Bucking = 40,

    /// <summary>Transfer (inter-location movement)</summary>
    Transfer = 50,

    /// <summary>Adjustment (stocktake variance)</summary>
    Adjustment = 60,

    /// <summary>Packaging (final packaging step)</summary>
    Packaging = 70,

    /// <summary>Quality Control sample</summary>
    QCSample = 80,

    /// <summary>Destruction (disposed/destroyed unit)</summary>
    Destruction = 90
}

/// <summary>
/// Result of serial number generation (16-digit format).
/// </summary>
public record SerialNumberResult
{
    /// <summary>16-digit serial number (TTYYYWWBBBBSSSSSS)</summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>Site ID used for sequence generation</summary>
    public int SiteId { get; init; }

    /// <summary>Serial type</summary>
    public SerialType SerialType { get; init; }

    /// <summary>Serial type name (human-readable)</summary>
    public string SerialTypeName { get; init; } = string.Empty;

    /// <summary>Year (2-digit, e.g., 25 for 2025)</summary>
    public int Year { get; init; }

    /// <summary>ISO week number (1-53)</summary>
    public int Week { get; init; }

    /// <summary>Parent batch sequence (1-9999)</summary>
    public int BatchSequence { get; init; }

    /// <summary>Serial sequence within the batch (1-999999)</summary>
    public int Sequence { get; init; }

    /// <summary>Parent batch number (12 digits)</summary>
    public string ParentBatchNumber { get; init; } = string.Empty;
}

/// <summary>
/// Parsed components of a serial number (16-digit format: TTYYYWWBBBBSSSSSS).
/// </summary>
public record SerialNumberComponents
{
    /// <summary>Serial type</summary>
    public SerialType SerialType { get; init; }

    /// <summary>Serial type name (human-readable)</summary>
    public string SerialTypeName { get; init; } = string.Empty;

    /// <summary>Year (2-digit, e.g., 25 for 2025)</summary>
    public int Year { get; init; }

    /// <summary>ISO week number (1-53)</summary>
    public int Week { get; init; }

    /// <summary>Parent batch sequence (1-9999)</summary>
    public int BatchSequence { get; init; }

    /// <summary>Serial sequence within the batch (1-999999)</summary>
    public int Sequence { get; init; }

    /// <summary>Original serial number string</summary>
    public string OriginalSerialNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the approximate first day of the serial's week.
    /// </summary>
    public DateTime ApproximateDate => System.Globalization.ISOWeek.ToDateTime(2000 + Year, Week, DayOfWeek.Monday);
}
