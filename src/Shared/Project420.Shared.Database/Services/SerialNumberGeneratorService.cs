using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique serial numbers in the 16-digit batch-linked format: TTYYYWWBBBBSSSSSS
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
/// Thread Safety:
/// - Uses database transactions for atomic sequence increments
/// - Safe for multi-instance deployments
///
/// Cannabis Compliance:
/// - SAHPRA unit-level traceability (seed-to-sale)
/// - Batch linkage enables recall management
/// - Week-based tracking aligns with production cycles
///
/// Architecture:
/// - Uses IBusinessDbContext interface to access business data tables
/// - IBusinessDbContext is implemented by PosDbContext in POS.DAL
/// - This avoids circular dependency between Shared.Database and POS.DAL
/// </remarks>
public class SerialNumberGeneratorService : ISerialNumberGeneratorService
{
    private readonly IBusinessDbContext _context;
    private readonly ILogger<SerialNumberGeneratorService> _logger;
    private readonly IBatchNumberGeneratorService _batchService;
    private readonly string _defaultUser;

    public SerialNumberGeneratorService(
        IBusinessDbContext context,
        ILogger<SerialNumberGeneratorService> logger,
        IBatchNumberGeneratorService batchService,
        string defaultUser = "SYSTEM")
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
        _defaultUser = defaultUser;
    }

    /// <inheritdoc />
    public async Task<SerialNumberResult> GenerateSerialNumberAsync(
        int siteId,
        SerialType serialType,
        string batchNumber,
        string? requestedBy = null)
    {
        // Validate inputs
        if (siteId < 1 || siteId > 99)
            throw new ArgumentOutOfRangeException(nameof(siteId), "Site ID must be 1-99.");

        if (!_batchService.ValidateBatchNumber(batchNumber))
            throw new ArgumentException($"Invalid batch number format: {batchNumber}", nameof(batchNumber));

        // Parse batch number to extract YYWW and BBBB
        var batchComponents = _batchService.ParseBatchNumber(batchNumber);
        var year = batchComponents.Year;
        var week = batchComponents.Week;
        var batchSequence = batchComponents.Sequence;
        var user = requestedBy ?? _defaultUser;

        _logger.LogDebug(
            "Generating serial for Site {SiteId}, Type {SerialType}, Batch {BatchNumber}",
            siteId, serialType, batchNumber);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get next serial sequence for this batch
            var serialSequence = await GetNextSerialSequenceAsync(
                siteId, serialType, year, week, batchSequence, user);

            // Build the serial number: TTYYYWWBBBBSSSSSS (16 digits)
            var serialNumber = FormatSerialNumber(serialType, year, week, batchSequence, serialSequence);

            // Store the serial number record
            var serialRecord = new SerialNumber
            {
                FullSerialNumber = serialNumber,
                ShortSerialNumber = serialNumber, // Same format now
                SiteId = siteId,
                StrainCode = 0, // Not used in new format
                BatchType = batchComponents.BatchType,
                ProductionDate = batchComponents.ApproximateDate,
                BatchSequence = batchSequence,
                UnitSequence = serialSequence,
                WeightGrams = 0, // Not embedded in new format
                PackQty = 1,
                Status = SerialStatus.Created,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };

            _context.SerialNumbers.Add(serialRecord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = new SerialNumberResult
            {
                SerialNumber = serialNumber,
                SiteId = siteId,
                SerialType = serialType,
                SerialTypeName = GetSerialTypeName(serialType),
                Year = year,
                Week = week,
                BatchSequence = batchSequence,
                Sequence = serialSequence,
                ParentBatchNumber = batchNumber
            };

            _logger.LogInformation(
                "Generated serial {SerialNumber} for Batch {BatchNumber}",
                serialNumber, batchNumber);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Failed to generate serial for Site {SiteId}, Type {SerialType}, Batch {BatchNumber}",
                siteId, serialType, batchNumber);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<SerialNumberResult>> GenerateBulkSerialNumbersAsync(
        int count,
        int siteId,
        SerialType serialType,
        string batchNumber,
        string? requestedBy = null)
    {
        if (count < 1 || count > 999999)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be between 1 and 999,999.");
        }

        var results = new List<SerialNumberResult>(count);

        _logger.LogInformation(
            "Generating {Count} serial numbers for Batch {BatchNumber}",
            count, batchNumber);

        for (int i = 0; i < count; i++)
        {
            var result = await GenerateSerialNumberAsync(
                siteId, serialType, batchNumber, requestedBy);
            results.Add(result);
        }

        _logger.LogInformation(
            "Generated {Count} serial numbers successfully", count);

        return results;
    }

    /// <inheritdoc />
    public bool ValidateSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return false;

        // Must be exactly 16 digits
        if (serialNumber.Length != 16)
            return false;

        // Must be all numeric
        if (!IsNumeric(serialNumber))
            return false;

        try
        {
            var components = ParseSerialNumber(serialNumber);

            // Validate serial type (must be defined enum value)
            if (!Enum.IsDefined(typeof(SerialType), components.SerialType))
                return false;

            // Validate year (reasonable range: 20-99 for 2020-2099)
            if (components.Year < 20 || components.Year > 99)
                return false;

            // Validate week (01-53)
            if (components.Week < 1 || components.Week > 53)
                return false;

            // Validate batch sequence (0001-9999)
            if (components.BatchSequence < 1 || components.BatchSequence > 9999)
                return false;

            // Validate serial sequence (000001-999999)
            if (components.Sequence < 1 || components.Sequence > 999999)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public SerialNumberComponents ParseSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be null or empty.", nameof(serialNumber));

        if (serialNumber.Length != 16)
            throw new ArgumentException($"Serial number must be 16 digits. Got {serialNumber.Length}.", nameof(serialNumber));

        if (!IsNumeric(serialNumber))
            throw new ArgumentException("Serial number must be all numeric.", nameof(serialNumber));

        // Extract components: TTYYYWWBBBBSSSSSS (16 digits)
        // TT: Serial Type (positions 0-1)
        // YY: Year (positions 2-3)
        // WW: Week (positions 4-5)
        // BBBB: Batch sequence (positions 6-9)
        // SSSSSS: Serial sequence (positions 10-15)

        var serialTypeStr = serialNumber.Substring(0, 2);
        var yearStr = serialNumber.Substring(2, 2);
        var weekStr = serialNumber.Substring(4, 2);
        var batchSeqStr = serialNumber.Substring(6, 4);
        var serialSeqStr = serialNumber.Substring(10, 6);

        // Parse serial type
        if (!int.TryParse(serialTypeStr, out int serialTypeInt))
            throw new ArgumentException($"Invalid serial type '{serialTypeStr}'.", nameof(serialNumber));

        if (!Enum.IsDefined(typeof(SerialType), serialTypeInt))
            throw new ArgumentException($"Unknown serial type code '{serialTypeInt}'.", nameof(serialNumber));

        var serialType = (SerialType)serialTypeInt;

        // Parse year
        if (!int.TryParse(yearStr, out int year) || year < 20 || year > 99)
            throw new ArgumentException($"Invalid year '{yearStr}'. Must be 20-99.", nameof(serialNumber));

        // Parse week
        if (!int.TryParse(weekStr, out int week) || week < 1 || week > 53)
            throw new ArgumentException($"Invalid week '{weekStr}'. Must be 01-53.", nameof(serialNumber));

        // Parse batch sequence
        if (!int.TryParse(batchSeqStr, out int batchSequence) || batchSequence < 1)
            throw new ArgumentException($"Invalid batch sequence '{batchSeqStr}'.", nameof(serialNumber));

        // Parse serial sequence
        if (!int.TryParse(serialSeqStr, out int serialSequence) || serialSequence < 1)
            throw new ArgumentException($"Invalid serial sequence '{serialSeqStr}'.", nameof(serialNumber));

        return new SerialNumberComponents
        {
            SerialType = serialType,
            SerialTypeName = GetSerialTypeName(serialType),
            Year = year,
            Week = week,
            BatchSequence = batchSequence,
            Sequence = serialSequence,
            OriginalSerialNumber = serialNumber
        };
    }

    /// <inheritdoc />
    public string GetSerialTypeName(SerialType serialType)
    {
        return serialType switch
        {
            SerialType.Production => "Production",
            SerialType.GRV => "Goods Received",
            SerialType.Retail => "Retail Sub",
            SerialType.Bucking => "Bucking",
            SerialType.Transfer => "Transfer",
            SerialType.Adjustment => "Adjustment",
            SerialType.Packaging => "Packaging",
            SerialType.QCSample => "QC Sample",
            SerialType.Destruction => "Destruction",
            _ => "Unknown"
        };
    }

    /// <inheritdoc />
    public string DeriveParentBatchNumber(string serialNumber, int siteId, BatchType batchType)
    {
        var components = ParseSerialNumber(serialNumber);

        // Reconstruct batch number: SSTTYYYWWNNNN
        return $"{siteId:D2}" +
               $"{(int)batchType:D2}" +
               $"{components.Year:D2}" +
               $"{components.Week:D2}" +
               $"{components.BatchSequence:D4}";
    }

    // ==========================================
    // Private Helper Methods
    // ==========================================

    private async Task<int> GetNextSerialSequenceAsync(
        int siteId, SerialType serialType, int year, int week, int batchSequence, string user)
    {
        // Create a unique key for this batch's serial sequence
        // We use the week start date for storage
        var weekDate = ISOWeek.ToDateTime(2000 + year, week, DayOfWeek.Monday);

        var sequence = await _context.SerialNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == siteId &&
                s.SequenceType == "Serial" &&
                s.ProductionDate == weekDate &&
                s.BatchSequence == batchSequence &&
                !s.IsDeleted);

        if (sequence == null)
        {
            sequence = new SerialNumberSequence
            {
                SiteId = siteId,
                SequenceType = "Serial",
                ProductionDate = weekDate,
                BatchSequence = batchSequence,
                CurrentSequence = 0,
                MaxSequence = 999999,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };
            _context.SerialNumberSequences.Add(sequence);
        }

        if (sequence.CurrentSequence >= sequence.MaxSequence)
        {
            throw new InvalidOperationException(
                $"Maximum serial sequence ({sequence.MaxSequence}) reached for " +
                $"Site {siteId}, Year {year}, Week {week}, Batch {batchSequence}.");
        }

        sequence.CurrentSequence++;
        sequence.LastGeneratedAt = DateTime.UtcNow;
        sequence.LastGeneratedBy = user;
        sequence.ModifiedAt = DateTime.UtcNow;
        sequence.ModifiedBy = user;

        return sequence.CurrentSequence;
    }

    private static string FormatSerialNumber(
        SerialType serialType, int year, int week, int batchSequence, int serialSequence)
    {
        // Format: TTYYYWWBBBBSSSSSS (16 digits)
        return $"{(int)serialType:D2}" +      // TT: Serial Type (2 digits)
               $"{year:D2}" +                  // YY: Year (2 digits)
               $"{week:D2}" +                  // WW: Week (2 digits)
               $"{batchSequence:D4}" +         // BBBB: Batch sequence (4 digits)
               $"{serialSequence:D6}";         // SSSSSS: Serial sequence (6 digits)
    }

    private static bool IsNumeric(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
}
