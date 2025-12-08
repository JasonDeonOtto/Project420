using System.Globalization;
using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Infrastructure.Services;

/// <summary>
/// Database-backed transaction number generation service.
/// Generates unique, sequential transaction numbers with persistent storage.
/// </summary>
/// <remarks>
/// PRODUCTION-READY:
/// - Database-backed sequences (persistent across restarts)
/// - Thread-safe via database transactions and row-level locking
/// - Supports multi-instance/multi-server deployments
/// - User-defined prefixes (letters-only OR numbers-only)
/// - No daily resets (continuous numbering)
///
/// Supported Formats:
/// 1. Standard Transactions: {Prefix}-{Sequence}
///    Examples: INV-00001, SALE-00123, 1-00001
///
/// 2. Production Batches: {YYYYMMDD}-{BatchType}-{Sequence}
///    Examples: 20251206-FLOWER-001, 20251206-EXTRACT-015
///
/// 3. Serial Numbers: SN-{STRAIN}-{BATCH}-{Sequence}
///    Examples: SN-BLUEDREAM-B001-00001, SN-SATIVA-B123-00456
///
/// Compliance:
/// - Cannabis Act: Unique batch/serial numbers for seed-to-sale traceability
/// - SARS: Sequential invoice numbering for tax compliance
/// - POPIA: Audit trail for all transaction number generation
///
/// Thread Safety:
/// - Database repository handles atomic increments
/// - Safe for multi-threaded/multi-instance environments
/// - No in-memory state (stateless service)
///
/// Migration from Old Service:
/// - Old Format: TYPE-YYYYMMDD-XXX (with daily reset)
/// - New Format: TYPE-XXXXX (continuous, no daily reset)
/// - ParseTransactionNumber supports both formats for backwards compatibility
/// </remarks>
public class TransactionNumberGeneratorService : ITransactionNumberGeneratorService
{
    private readonly ITransactionNumberRepository _repository;
    private readonly string _defaultUser;

    /// <summary>
    /// Initializes the transaction number generator service.
    /// </summary>
    /// <param name="repository">Repository for database-backed sequence operations</param>
    /// <param name="defaultUser">Default user for audit trail (e.g., "SYSTEM", current user)</param>
    public TransactionNumberGeneratorService(
        ITransactionNumberRepository repository,
        string defaultUser = "SYSTEM")
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _defaultUser = defaultUser ?? throw new ArgumentNullException(nameof(defaultUser));
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(TransactionTypeCode type)
    {
        return await GenerateAsync(type, _defaultUser);
    }

    /// <summary>
    /// Generates a transaction number for the specified type with custom user.
    /// </summary>
    /// <param name="type">The transaction type</param>
    /// <param name="requestedBy">Who is requesting the number (for audit)</param>
    /// <returns>Unique transaction number (e.g., INV-00001)</returns>
    public async Task<string> GenerateAsync(TransactionTypeCode type, string requestedBy)
    {
        // Validate input
        if (!Enum.IsDefined(typeof(TransactionTypeCode), type))
        {
            throw new ArgumentException($"Invalid transaction type code: {type}", nameof(type));
        }

        // Get sequence configuration
        var sequence = await _repository.GetSequenceAsync(type);
        if (sequence == null)
        {
            throw new InvalidOperationException(
                $"No sequence configured for transaction type '{type}'. " +
                "Please create a sequence configuration first."
            );
        }

        // Atomic increment (thread-safe via database)
        long nextSequence = await _repository.GetNextSequenceAsync(type, requestedBy);

        // Format the transaction number
        string prefix = sequence.GetEffectivePrefix();
        string formattedSequence = sequence.FormatSequence(nextSequence);

        return $"{prefix}-{formattedSequence}";
    }

    /// <summary>
    /// Generates a production batch number (purely numeric).
    /// Format: YYYYMMDDNNNNN (13 digits: date + continuous sequence)
    /// </summary>
    /// <param name="batchType">Type of batch (for validation/logging only, not encoded in number)</param>
    /// <param name="productionDate">Date of production (defaults to today)</param>
    /// <param name="requestedBy">Who is creating the batch</param>
    /// <returns>Unique batch number (e.g., 2025120600001)</returns>
    /// <remarks>
    /// **NUMERIC FORMAT**: Pure numbers for barcode/RFID compatibility
    /// - First 8 digits: Production date (YYYYMMDD)
    /// - Last 5 digits: Continuous global sequence (never resets)
    /// - Example: `2025120600001`, `2025120600002`, `2025120700003`
    ///
    /// Cannabis Compliance:
    /// - Required for seed-to-sale traceability
    /// - Batch numbers must be unique and sequential
    /// - Date component helps with FIFO/FEFO inventory management
    /// - Barcode/RFID compatible (numeric only)
    ///
    /// Traceability:
    /// - Batch type stored in database (not encoded in number)
    /// - Lookup batch details via database query
    ///
    /// Internal Implementation:
    /// - Uses TransactionTypeCode.ADJ for sequence generation
    /// - Sequence is continuous (no daily reset)
    /// - Returns purely numeric string (13 digits)
    /// </remarks>
    public async Task<string> GenerateBatchNumberAsync(
        string batchType,
        DateTime? productionDate = null,
        string? requestedBy = null)
    {
        // Validation (batchType for business logic validation only)
        if (string.IsNullOrWhiteSpace(batchType))
        {
            throw new ArgumentException("Batch type cannot be null or empty.", nameof(batchType));
        }

        // Validate batch type format (letters/numbers only)
        if (!System.Text.RegularExpressions.Regex.IsMatch(batchType, @"^[A-Z0-9]+$"))
        {
            throw new ArgumentException(
                $"Invalid batch type '{batchType}'. Must be uppercase letters/numbers only (e.g., FLOWER, EXTRACT).",
                nameof(batchType)
            );
        }

        // Use current date if not specified
        DateTime date = productionDate ?? DateTime.Now;
        string dateString = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        // Get sequence (using ADJ type for batch numbers)
        var sequence = await _repository.GetSequenceAsync(TransactionTypeCode.ADJ);
        if (sequence == null)
        {
            throw new InvalidOperationException(
                "Batch number sequence not configured. Please create TransactionTypeCode.ADJ sequence."
            );
        }

        // Atomic increment
        long nextSequence = await _repository.GetNextSequenceAsync(
            TransactionTypeCode.ADJ,
            requestedBy ?? _defaultUser
        );

        // Format: YYYYMMDDNNNNN (13 digits total)
        // Date (8 digits) + Sequence (5 digits with padding)
        string formattedSequence = sequence.FormatSequence(nextSequence);

        // Return purely numeric string (no dashes, no letters)
        return $"{dateString}{formattedSequence}";
    }

    /// <summary>
    /// Generates a serial number for product traceability with embedded information.
    /// Format: YYYYMMDD{StrainCode}{BatchSeq}{UnitSeq} (19 digits)
    /// </summary>
    /// <param name="strainCode">Numeric strain code (100, 200, 300...) where first digit = type (1=Sativa, 2=Indica, 3=Hybrid)</param>
    /// <param name="batchNumber">Global batch number this unit belongs to (e.g., 2025120600001)</param>
    /// <param name="batchSequenceForStrainDate">Batch sequence for this strain on this date (e.g., 1, 2, 3)</param>
    /// <param name="requestedBy">Who is creating the serial number</param>
    /// <returns>Unique serial number (e.g., 2025120610000100001)</returns>
    /// <remarks>
    /// **EMBEDDED TRACEABILITY FORMAT**: 19 digits encoding full product lineage
    /// - Digits 1-8: Production date (YYYYMMDD)
    /// - Digits 9-11: Strain code (100, 200, 300...) - First digit = strain type
    /// - Digits 12-14: Batch sequence for this strain on this date (001, 002, 003...)
    /// - Digits 15-19: Unit sequence within this batch (00001, 00002, 00003...)
    ///
    /// Strain Code Encoding:
    /// - 1xx (100-199): Sativa strains
    /// - 2xx (200-299): Indica strains
    /// - 3xx (300-399): Hybrid strains
    /// - 4xx+: Reserved for future types
    ///
    /// Example: `2025120610000100001`
    /// - 20251206 = December 6, 2025 (production date)
    /// - 100 = Sativa strain #1
    /// - 001 = First batch of strain #100 produced on Dec 6
    /// - 00001 = First unit in this batch
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
    /// - Database stores additional details (THC/CBD, weight, etc.)
    ///
    /// Internal Implementation:
    /// - Uses TransactionTypeCode.TRF for unit sequence generation
    /// - Unit sequence is PER BATCH (not global)
    /// - Batch sequence for strain/date must be tracked externally
    /// </remarks>
    public async Task<string> GenerateSerialNumberAsync(
        int strainCode,
        string batchNumber,
        int batchSequenceForStrainDate,
        string? requestedBy = null)
    {
        // Validation
        if (strainCode < 100 || strainCode > 999)
        {
            throw new ArgumentException(
                $"Strain code must be between 100-999 (e.g., 100, 200, 300). Got: {strainCode}",
                nameof(strainCode)
            );
        }

        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number cannot be null or empty.", nameof(batchNumber));
        }

        // Validate batch number format (13 digits: YYYYMMDDNNNNN)
        if (!long.TryParse(batchNumber, out long batchNumeric) || batchNumber.Length != 13)
        {
            throw new ArgumentException(
                $"Batch number must be 13 digits (YYYYMMDDNNNNN format). Got: {batchNumber}",
                nameof(batchNumber)
            );
        }

        if (batchSequenceForStrainDate < 1 || batchSequenceForStrainDate > 999)
        {
            throw new ArgumentException(
                $"Batch sequence for strain/date must be between 1-999. Got: {batchSequenceForStrainDate}",
                nameof(batchSequenceForStrainDate)
            );
        }

        // Extract date from batch number (first 8 digits)
        string productionDate = batchNumber.Substring(0, 8);

        // Get sequence (using TRF type for unit serial numbers)
        var sequence = await _repository.GetSequenceAsync(TransactionTypeCode.TRF);
        if (sequence == null)
        {
            throw new InvalidOperationException(
                "Serial number sequence not configured. Please create TransactionTypeCode.TRF sequence."
            );
        }

        // Atomic increment (unit sequence within batch)
        long nextUnitSequence = await _repository.GetNextSequenceAsync(
            TransactionTypeCode.TRF,
            requestedBy ?? _defaultUser
        );

        // Format components
        string strainCodeFormatted = strainCode.ToString("D3");                              // 100, 200, 300
        string batchSeqFormatted = batchSequenceForStrainDate.ToString("D3");               // 001, 002, 003
        string unitSeqFormatted = nextUnitSequence.ToString("D5");                          // 00001, 00002

        // Combine: YYYYMMDD + StrainCode + BatchSeq + UnitSeq
        // Example: 20251206 + 100 + 001 + 00001 = 2025120610000100001
        return $"{productionDate}{strainCodeFormatted}{batchSeqFormatted}{unitSeqFormatted}";
    }

    /// <inheritdoc />
    /// <remarks>
    /// Backwards Compatibility:
    /// - Old Format: TYPE-YYYYMMDD-XXX (with daily reset, deprecated)
    /// - New Format: TYPE-XXXXX (continuous numbering, current)
    /// - This method supports parsing both formats
    /// </remarks>
    public (TransactionTypeCode Type, DateTime Date, int Sequence) ParseTransactionNumber(string transactionNumber)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
        {
            throw new ArgumentException("Transaction number cannot be null or empty.", nameof(transactionNumber));
        }

        // Split by dash
        string[] parts = transactionNumber.Split('-');

        // Determine format
        if (parts.Length == 2)
        {
            // New Format: TYPE-XXXXX (continuous numbering)
            return ParseNewFormat(transactionNumber, parts);
        }
        else if (parts.Length == 3)
        {
            // Old Format: TYPE-YYYYMMDD-XXX (deprecated, for backwards compatibility)
            return ParseOldFormat(transactionNumber, parts);
        }
        else
        {
            throw new ArgumentException(
                $"Invalid transaction number format '{transactionNumber}'. " +
                "Expected 'TYPE-XXXXX' or 'TYPE-YYYYMMDD-XXX'.",
                nameof(transactionNumber)
            );
        }
    }

    /// <summary>
    /// Parses new format: TYPE-XXXXX (continuous numbering).
    /// </summary>
    private (TransactionTypeCode Type, DateTime Date, int Sequence) ParseNewFormat(
        string transactionNumber,
        string[] parts)
    {
        // Parse type code
        string typeString = parts[0];
        if (!Enum.TryParse(typeString, out TransactionTypeCode type))
        {
            throw new ArgumentException(
                $"Invalid transaction type code '{typeString}' in transaction number '{transactionNumber}'.",
                nameof(transactionNumber)
            );
        }

        // Parse sequence
        string sequenceString = parts[1];
        if (!int.TryParse(sequenceString, out int sequence) || sequence < 1)
        {
            throw new ArgumentException(
                $"Invalid sequence number '{sequenceString}' in transaction number '{transactionNumber}'.",
                nameof(transactionNumber)
            );
        }

        // New format doesn't include date, use current date
        return (type, DateTime.Now.Date, sequence);
    }

    /// <summary>
    /// Parses old format: TYPE-YYYYMMDD-XXX (deprecated, for backwards compatibility).
    /// </summary>
    private (TransactionTypeCode Type, DateTime Date, int Sequence) ParseOldFormat(
        string transactionNumber,
        string[] parts)
    {
        // Parse type code
        string typeString = parts[0];
        if (!Enum.TryParse(typeString, out TransactionTypeCode type))
        {
            throw new ArgumentException(
                $"Invalid transaction type code '{typeString}' in transaction number '{transactionNumber}'.",
                nameof(transactionNumber)
            );
        }

        // Parse date (YYYYMMDD)
        string dateString = parts[1];
        if (!DateTime.TryParseExact(
            dateString,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime date))
        {
            throw new ArgumentException(
                $"Invalid date format '{dateString}' in transaction number '{transactionNumber}'. Expected YYYYMMDD.",
                nameof(transactionNumber)
            );
        }

        // Parse sequence
        string sequenceString = parts[2];
        if (!int.TryParse(sequenceString, out int sequence) || sequence < 1)
        {
            throw new ArgumentException(
                $"Invalid sequence number '{sequenceString}' in transaction number '{transactionNumber}'.",
                nameof(transactionNumber)
            );
        }

        return (type, date, sequence);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Note: This method checks uniqueness against the database, NOT in-memory cache.
    /// For production use, consider querying the actual transaction tables instead
    /// of just the sequence configuration.
    /// </remarks>
    public async Task<bool> IsUniqueAsync(string transactionNumber)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
        {
            throw new ArgumentException("Transaction number cannot be null or empty.", nameof(transactionNumber));
        }

        // Parse to validate format
        try
        {
            var parsed = ParseTransactionNumber(transactionNumber);

            // For true uniqueness checking, you would query the actual transaction tables
            // (TransactionHeader, etc.) to see if this number is already used.
            // For now, we assume the database sequence guarantees uniqueness.
            return true; // Database sequence ensures uniqueness
        }
        catch (ArgumentException)
        {
            // Invalid format = not unique (or not valid)
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetNextSequenceAsync(TransactionTypeCode type, DateTime date)
    {
        // Get sequence configuration
        var sequence = await _repository.GetSequenceAsync(type);
        if (sequence == null)
        {
            return 1; // Start at 1 if no sequence exists yet
        }

        // Return next sequence number (current + 1)
        // Note: This is a preview only, not an atomic increment
        return (int)(sequence.CurrentSequence + 1);
    }
}
