using Project420.Shared.Core.Enums;

namespace Project420.Shared.Infrastructure.Interfaces;

/// <summary>
/// Service for generating unique transaction numbers for all transaction types.
/// Ensures uniqueness and follows consistent naming patterns across the system.
/// </summary>
/// <remarks>
/// **NEW FORMAT (Database-backed, Production-Ready)**:
/// Transaction Number Format: {PREFIX}-{SEQUENCE}
///
/// Where:
/// - PREFIX: User-defined or TransactionType enum value (e.g., INV, SALE, 1, 2)
/// - SEQUENCE: Auto-incrementing number with padding (e.g., 00001, 00123)
///
/// Features:
/// - **NO daily resets** - continuous numbering
/// - **Persistent** - survives application restarts (database-backed)
/// - **Thread-safe** - supports multi-user/multi-instance environments
/// - **User-defined prefixes** - letters-only OR numbers-only
/// - **User-defined starting numbers** - start from any sequence
///
/// Examples:
/// - INV-00001 (Invoice #1)
/// - INV-00002 (Invoice #2)
/// - SALE-00001 (Sale #1)
/// - 1-00001 (Numeric prefix for invoices)
/// - 2-00001 (Numeric prefix for credits)
///
/// Special Formats (NUMERIC ONLY):
/// 1. **Batch Numbers**: YYYYMMDDNNNNN (13 digits: date + sequence)
///    Example: 2025120600001 (Dec 6, 2025, batch #1)
///
/// 2. **Serial Numbers**: YYYYMMDD{Strain}{BatchSeq}{UnitSeq} (19 digits: embedded traceability)
///    Example: 2025120610000100001 (Dec 6, 2025, Sativa strain #1, batch #1, unit #1)
///
/// **Backwards Compatibility**:
/// Old Format: TYPE-YYYYMMDD-XXX (with daily reset, deprecated)
/// - ParseTransactionNumber() supports both old and new formats
/// - Old format is maintained for historical data only
///
/// Compliance:
/// - Cannabis Act: Unique batch/serial numbers for traceability
/// - SARS: Sequential invoice numbering for tax compliance
/// - POPIA: Audit trail for all number generation
/// </remarks>
public interface ITransactionNumberGeneratorService
{
    /// <summary>
    /// Generates a unique transaction number for the specified type.
    /// </summary>
    /// <param name="type">The transaction type code.</param>
    /// <returns>A unique transaction number (e.g., "INV-00001").</returns>
    /// <example>
    /// <code>
    /// var transactionNumber = await service.GenerateAsync(TransactionTypeCode.INV);
    /// // Result: "INV-00001"
    /// </code>
    /// </example>
    /// <remarks>
    /// - Uses database-backed sequence (persistent across restarts)
    /// - Thread-safe via database row-level locking
    /// - Prefix from sequence configuration (user-defined or enum value)
    /// - Continuous numbering (no daily reset)
    /// </remarks>
    Task<string> GenerateAsync(TransactionTypeCode type);

    /// <summary>
    /// Generates a production batch number (purely numeric).
    /// Format: YYYYMMDDNNNNN (13 digits: date + continuous sequence)
    /// </summary>
    /// <param name="batchType">Type of batch (for validation/logging only, not encoded in number)</param>
    /// <param name="productionDate">Date of production (defaults to today)</param>
    /// <param name="requestedBy">Who is creating the batch (for audit)</param>
    /// <returns>Unique batch number (e.g., 2025120600001)</returns>
    /// <example>
    /// <code>
    /// var batchNumber = await service.GenerateBatchNumberAsync("FLOWER");
    /// // Result: "2025120600001" (Dec 6, 2025, sequence #1)
    ///
    /// var customBatch = await service.GenerateBatchNumberAsync(
    ///     "EXTRACT",
    ///     new DateTime(2025, 12, 01),
    ///     "user@example.com"
    /// );
    /// // Result: "2025120100001" (Dec 1, 2025, sequence #1)
    /// </code>
    /// </example>
    /// <remarks>
    /// **NUMERIC FORMAT**: Pure numbers for barcode/RFID compatibility
    /// - First 8 digits: Production date (YYYYMMDD)
    /// - Last 5 digits: Continuous global sequence (never resets)
    ///
    /// Cannabis Compliance:
    /// - Required for seed-to-sale traceability
    /// - Batch numbers must be unique and sequential
    /// - Date component helps with FIFO/FEFO inventory management
    /// - Barcode/RFID compatible (numeric only)
    ///
    /// Traceability:
    /// - Batch type stored in database (not encoded in number)
    /// - Use batchType parameter for validation and database storage
    /// - Lookup batch details via database query
    ///
    /// Technical:
    /// - Uses TransactionTypeCode.ADJ sequence internally
    /// - Batch type must be uppercase letters/numbers only
    /// - Continuous sequence (no daily reset)
    /// </remarks>
    Task<string> GenerateBatchNumberAsync(
        string batchType,
        DateTime? productionDate = null,
        string? requestedBy = null);

    /// <summary>
    /// Generates a serial number for product traceability with embedded information.
    /// Format: YYYYMMDD{StrainCode}{BatchSeq}{UnitSeq} (19 digits)
    /// </summary>
    /// <param name="strainCode">Numeric strain code (100, 200, 300...) where first digit = type (1=Sativa, 2=Indica, 3=Hybrid)</param>
    /// <param name="batchNumber">Global batch number this unit belongs to (e.g., 2025120600001)</param>
    /// <param name="batchSequenceForStrainDate">Batch sequence for this strain on this date (e.g., 1, 2, 3)</param>
    /// <param name="requestedBy">Who is creating the serial number (for audit)</param>
    /// <returns>Unique serial number (e.g., 2025120610000100001)</returns>
    /// <example>
    /// <code>
    /// // First batch of Sativa strain #1 on Dec 6, 2025
    /// var serialNumber = await service.GenerateSerialNumberAsync(
    ///     strainCode: 100,              // Sativa strain #1 (1xx = Sativa)
    ///     batchNumber: "2025120600001", // Dec 6, global batch #1
    ///     batchSequenceForStrainDate: 1 // First batch of strain 100 today
    /// );
    /// // Result: "2025120610000100001"
    /// //          20251206 = Date
    /// //          100 = Sativa strain #1
    /// //          001 = First batch of this strain today
    /// //          00001 = First unit in this batch
    ///
    /// // Second batch of Indica strain #1 on same day
    /// var serialNumber2 = await service.GenerateSerialNumberAsync(
    ///     strainCode: 200,              // Indica strain #1 (2xx = Indica)
    ///     batchNumber: "2025120600002", // Dec 6, global batch #2
    ///     batchSequenceForStrainDate: 1 // First batch of strain 200 today
    /// );
    /// // Result: "2025120620000100001"
    /// </code>
    /// </example>
    /// <remarks>
    /// **EMBEDDED TRACEABILITY FORMAT**: 19 digits encoding full product lineage
    /// - Digits 1-8: Production date (YYYYMMDD)
    /// - Digits 9-11: Strain code (100, 200, 300...) - First digit = strain type
    /// - Digits 12-14: Batch sequence for this strain on this date (001, 002, 003...)
    /// - Digits 15-19: Unit sequence within this batch (00001, 00002, 00003...)
    ///
    /// Strain Code Encoding:
    /// - 1xx (100-199): Sativa strains (e.g., 100, 110, 120)
    /// - 2xx (200-299): Indica strains (e.g., 200, 210, 220)
    /// - 3xx (300-399): Hybrid strains (e.g., 300, 310, 320)
    /// - 4xx+: Reserved for future types
    ///
    /// Cannabis Compliance:
    /// - Required for unit-level traceability (SAHPRA requirements)
    /// - Instant visual identification of production date, strain type, batch, and unit
    /// - Critical for recalls and quality control
    /// - Barcode/RFID compatible (purely numeric)
    ///
    /// Traceability:
    /// - All information encoded in the serial number itself
    /// - No database lookup needed for basic identification
    /// - Parse serial to extract date, strain, batch sequence, unit number
    ///
    /// Technical:
    /// - Uses TransactionTypeCode.TRF sequence for unit numbers
    /// - Unit sequence is PER BATCH (resets per batch)
    /// - Batch sequence for strain/date must be tracked externally by caller
    /// </remarks>
    Task<string> GenerateSerialNumberAsync(
        int strainCode,
        string batchNumber,
        int batchSequenceForStrainDate,
        string? requestedBy = null);

    /// <summary>
    /// Checks if a transaction number has already been used.
    /// </summary>
    /// <param name="transactionNumber">The transaction number to check.</param>
    /// <returns>True if the number is unique (not yet used), otherwise false.</returns>
    /// <remarks>
    /// This is useful for validation before saving a transaction to the database.
    /// Currently validates format only. For production, should query transaction tables.
    /// </remarks>
    Task<bool> IsUniqueAsync(string transactionNumber);

    /// <summary>
    /// Gets the next sequence number for a specific transaction type.
    /// </summary>
    /// <param name="type">The transaction type code.</param>
    /// <param name="date">The transaction date (for backwards compatibility, not used in new format).</param>
    /// <returns>The next sequence number (e.g., 1, 2, 3, ...).</returns>
    /// <remarks>
    /// This is useful for preview purposes or manual number generation.
    /// Does NOT increment the sequence counter.
    /// </remarks>
    Task<int> GetNextSequenceAsync(TransactionTypeCode type, DateTime date);

    /// <summary>
    /// Parses a transaction number and extracts its components.
    /// </summary>
    /// <param name="transactionNumber">The transaction number to parse.</param>
    /// <returns>A tuple containing the type code, date, and sequence number.</returns>
    /// <exception cref="ArgumentException">Thrown if the transaction number format is invalid.</exception>
    /// <example>
    /// <code>
    /// // New format (continuous numbering)
    /// var (type, date, sequence) = service.ParseTransactionNumber("INV-00001");
    /// // type = TransactionTypeCode.INV
    /// // date = DateTime.Now.Date (current date, format doesn't include date)
    /// // sequence = 1
    ///
    /// // Old format (backwards compatibility)
    /// var (type2, date2, sequence2) = service.ParseTransactionNumber("SALE-20251205-001");
    /// // type2 = TransactionTypeCode.SALE
    /// // date2 = DateTime(2025, 12, 05)
    /// // sequence2 = 1
    /// </code>
    /// </example>
    /// <remarks>
    /// Supports both:
    /// - New format: TYPE-XXXXX (continuous numbering)
    /// - Old format: TYPE-YYYYMMDD-XXX (backwards compatibility)
    /// </remarks>
    (TransactionTypeCode Type, DateTime Date, int Sequence) ParseTransactionNumber(string transactionNumber);
}
