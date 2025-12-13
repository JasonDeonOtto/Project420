using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique serial numbers in two formats:
/// - Full Serial Number (30 digits + check digit) for QR codes
/// - Short Serial Number (13 digits) for barcodes/EAN-13
/// </summary>
/// <remarks>
/// Full Serial Number Format (30+1 digits): SS+SSS+TT+YYYYMMDD+BBBB+UUUUU+WWWW+Q+C
/// - SS: Site ID (2 digits, 01-99)
/// - SSS: Strain Code (3 digits, 100-999)
/// - TT: Batch Type (2 digits, from BatchType enum)
/// - YYYYMMDD: Production date (8 digits)
/// - BBBB: Batch sequence (4 digits)
/// - UUUUU: Unit sequence within batch (5 digits)
/// - WWWW: Weight in tenths of grams (4 digits, e.g., 0035 = 3.5g)
/// - Q: Quantity/Pack code (1 digit, 1=single, 2=pack of 2, etc.)
/// - C: Luhn check digit (1 digit)
///
/// Short Serial Number Format (13 digits): SSYYMMDDNNNNN
/// - SS: Site ID (2 digits)
/// - YYMMDD: Date in short format (6 digits)
/// - NNNNN: Daily sequence (5 digits)
///
/// Strain Code Encoding:
/// - 100-199: Sativa strains
/// - 200-299: Indica strains
/// - 300-399: Hybrid strains
/// - 400-499: CBD-only strains
/// - 500-999: Reserved for future use
///
/// Cannabis Compliance:
/// - SAHPRA unit-level traceability (seed-to-sale)
/// - Instant visual identification of production details
/// - Critical for recalls and quality control
/// - Barcode/QR compatible (all numeric)
/// </remarks>
public interface ISerialNumberGeneratorService
{
    /// <summary>
    /// Generates a full serial number (31 digits including check digit).
    /// </summary>
    /// <param name="siteId">Site ID (1-99)</param>
    /// <param name="strainCode">Strain code (100-999, first digit indicates type)</param>
    /// <param name="batchType">Type of batch</param>
    /// <param name="productionDate">Production date (defaults to today)</param>
    /// <param name="batchSequence">Batch sequence for this site/type/date (1-9999)</param>
    /// <param name="weightGrams">Weight in grams (0.1 to 999.9)</param>
    /// <param name="packQty">Pack quantity code (0-9, where 0=bulk, 1=single, 2=pack of 2, etc.)</param>
    /// <param name="requestedBy">User requesting serial number (for audit)</param>
    /// <returns>Full serial number generation result with full SN, short SN, and components</returns>
    Task<SerialNumberResult> GenerateSerialNumberAsync(
        int siteId,
        int strainCode,
        BatchType batchType,
        DateTime? productionDate = null,
        int? batchSequence = null,
        decimal weightGrams = 0m,
        int packQty = 1,
        string? requestedBy = null);

    /// <summary>
    /// Generates multiple serial numbers for a batch (bulk packaging).
    /// </summary>
    /// <param name="count">Number of serial numbers to generate</param>
    /// <param name="siteId">Site ID (1-99)</param>
    /// <param name="strainCode">Strain code (100-999)</param>
    /// <param name="batchType">Type of batch</param>
    /// <param name="productionDate">Production date</param>
    /// <param name="batchSequence">Batch sequence</param>
    /// <param name="weightGrams">Weight per unit</param>
    /// <param name="packQty">Pack quantity code</param>
    /// <param name="requestedBy">User requesting serial numbers</param>
    /// <returns>List of serial number results</returns>
    Task<List<SerialNumberResult>> GenerateBulkSerialNumbersAsync(
        int count,
        int siteId,
        int strainCode,
        BatchType batchType,
        DateTime? productionDate = null,
        int? batchSequence = null,
        decimal weightGrams = 0m,
        int packQty = 1,
        string? requestedBy = null);

    /// <summary>
    /// Validates a full serial number (including Luhn check digit).
    /// </summary>
    /// <param name="fullSerialNumber">The full serial number to validate</param>
    /// <returns>True if valid format and check digit, false otherwise</returns>
    bool ValidateFullSerialNumber(string fullSerialNumber);

    /// <summary>
    /// Validates a short serial number format.
    /// </summary>
    /// <param name="shortSerialNumber">The short serial number to validate</param>
    /// <returns>True if valid format, false otherwise</returns>
    bool ValidateShortSerialNumber(string shortSerialNumber);

    /// <summary>
    /// Parses a full serial number into its components.
    /// </summary>
    /// <param name="fullSerialNumber">The full serial number to parse</param>
    /// <returns>Parsed components</returns>
    /// <exception cref="ArgumentException">If format is invalid</exception>
    FullSerialNumberComponents ParseFullSerialNumber(string fullSerialNumber);

    /// <summary>
    /// Parses a short serial number into its components.
    /// </summary>
    /// <param name="shortSerialNumber">The short serial number to parse</param>
    /// <returns>Parsed components</returns>
    /// <exception cref="ArgumentException">If format is invalid</exception>
    ShortSerialNumberComponents ParseShortSerialNumber(string shortSerialNumber);

    /// <summary>
    /// Gets the strain type description from a strain code.
    /// </summary>
    /// <param name="strainCode">Strain code (100-999)</param>
    /// <returns>Strain type (Sativa, Indica, Hybrid, CBD, Unknown)</returns>
    string GetStrainType(int strainCode);
}

/// <summary>
/// Result of serial number generation containing both full and short formats.
/// </summary>
public record SerialNumberResult
{
    /// <summary>Full serial number (31 digits with check digit)</summary>
    public string FullSerialNumber { get; init; } = string.Empty;

    /// <summary>Short serial number (13 digits for barcode)</summary>
    public string ShortSerialNumber { get; init; } = string.Empty;

    /// <summary>Site ID</summary>
    public int SiteId { get; init; }

    /// <summary>Strain code</summary>
    public int StrainCode { get; init; }

    /// <summary>Strain type description</summary>
    public string StrainType { get; init; } = string.Empty;

    /// <summary>Batch type</summary>
    public BatchType BatchType { get; init; }

    /// <summary>Production date</summary>
    public DateTime ProductionDate { get; init; }

    /// <summary>Batch sequence for this site/type/date</summary>
    public int BatchSequence { get; init; }

    /// <summary>Unit sequence within the batch</summary>
    public int UnitSequence { get; init; }

    /// <summary>Weight in grams</summary>
    public decimal WeightGrams { get; init; }

    /// <summary>Pack quantity</summary>
    public int PackQty { get; init; }

    /// <summary>Luhn check digit</summary>
    public int CheckDigit { get; init; }
}

/// <summary>
/// Parsed components of a full serial number.
/// </summary>
public record FullSerialNumberComponents
{
    /// <summary>Site ID (1-99)</summary>
    public int SiteId { get; init; }

    /// <summary>Strain code (100-999)</summary>
    public int StrainCode { get; init; }

    /// <summary>Strain type description</summary>
    public string StrainType { get; init; } = string.Empty;

    /// <summary>Batch type</summary>
    public BatchType BatchType { get; init; }

    /// <summary>Production date</summary>
    public DateTime ProductionDate { get; init; }

    /// <summary>Batch sequence</summary>
    public int BatchSequence { get; init; }

    /// <summary>Unit sequence</summary>
    public int UnitSequence { get; init; }

    /// <summary>Weight in grams (decoded from tenths)</summary>
    public decimal WeightGrams { get; init; }

    /// <summary>Pack quantity code</summary>
    public int PackQty { get; init; }

    /// <summary>Check digit</summary>
    public int CheckDigit { get; init; }

    /// <summary>Original full serial number</summary>
    public string OriginalSerialNumber { get; init; } = string.Empty;

    /// <summary>Whether the check digit is valid</summary>
    public bool IsCheckDigitValid { get; init; }
}

/// <summary>
/// Parsed components of a short serial number.
/// </summary>
public record ShortSerialNumberComponents
{
    /// <summary>Site ID (1-99)</summary>
    public int SiteId { get; init; }

    /// <summary>Production date</summary>
    public DateTime ProductionDate { get; init; }

    /// <summary>Daily sequence</summary>
    public int Sequence { get; init; }

    /// <summary>Original short serial number</summary>
    public string OriginalSerialNumber { get; init; } = string.Empty;
}
