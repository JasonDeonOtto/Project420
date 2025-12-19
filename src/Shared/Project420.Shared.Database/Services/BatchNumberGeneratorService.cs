using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique batch numbers following the 12-digit week-based format: SSTTYYYWWNNNN
/// </summary>
/// <remarks>
/// Batch Number Format (12 digits): SSTTYYYWWNNNN
/// - SS: Site ID (01-99)
/// - TT: Batch Type code (10=Production, 20=GRV, 30=Transfer, etc.)
/// - YY: Year (2-digit, e.g., 25 for 2025)
/// - WW: ISO week number (01-53)
/// - NNNN: Weekly sequence per site/type (0001-9999)
///
/// Visual Identification Example:
/// 011025510001 = Site 01, Production (10), 2025, Week 51, Batch #1
///
/// Thread Safety:
/// - Uses database-level locking via transactions
/// - Safe for multi-instance deployments
///
/// Cannabis Compliance:
/// - SAHPRA seed-to-sale traceability
/// - Unique batch identification for audit trail
/// - Week component enables FIFO/FEFO inventory management
/// - Serial numbers embed batch reference (YYWWNNNN) for full traceability
///
/// Architecture:
/// - Uses IBusinessDbContext interface to access business data tables
/// - IBusinessDbContext is implemented by PosDbContext in POS.DAL
/// - This avoids circular dependency between Shared.Database and POS.DAL
/// </remarks>
public class BatchNumberGeneratorService : IBatchNumberGeneratorService
{
    private readonly IBusinessDbContext _context;
    private readonly ILogger<BatchNumberGeneratorService> _logger;
    private readonly string _defaultUser;

    public BatchNumberGeneratorService(
        IBusinessDbContext context,
        ILogger<BatchNumberGeneratorService> logger,
        string defaultUser = "SYSTEM")
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultUser = defaultUser;
    }

    /// <inheritdoc />
    public async Task<string> GenerateBatchNumberAsync(
        int siteId,
        BatchType batchType,
        DateTime? batchDate = null,
        string? requestedBy = null)
    {
        // Validate site ID
        if (siteId < 1 || siteId > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(siteId), siteId,
                "Site ID must be between 1 and 99.");
        }

        var date = batchDate?.Date ?? DateTime.Today;
        var year = ISOWeek.GetYear(date);
        var week = ISOWeek.GetWeekOfYear(date);
        var user = requestedBy ?? _defaultUser;

        // Create a "week date" for storage (first day of the ISO week)
        var weekDate = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);

        _logger.LogDebug(
            "Generating batch number for Site {SiteId}, Type {BatchType}, Year {Year}, Week {Week}",
            siteId, batchType, year, week);

        // Use a transaction for atomic increment
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Find or create sequence record for this site/type/week
            var sequence = await _context.BatchNumberSequences
                .FirstOrDefaultAsync(s =>
                    s.SiteId == siteId &&
                    s.BatchType == batchType &&
                    s.BatchDate == weekDate &&
                    !s.IsDeleted);

            if (sequence == null)
            {
                // Create new sequence for this combination
                sequence = new BatchNumberSequence
                {
                    SiteId = siteId,
                    BatchType = batchType,
                    BatchDate = weekDate, // Store the week start date
                    CurrentSequence = 0,
                    MaxSequence = 9999,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user
                };
                _context.BatchNumberSequences.Add(sequence);
            }

            // Check if max sequence reached
            if (sequence.CurrentSequence >= sequence.MaxSequence)
            {
                throw new InvalidOperationException(
                    $"Maximum batch sequence ({sequence.MaxSequence}) reached for " +
                    $"Site {siteId}, Type {batchType}, Year {year}, Week {week}. " +
                    "Cannot generate more batch numbers for this combination this week.");
            }

            // Increment sequence
            sequence.CurrentSequence++;
            sequence.LastGeneratedAt = DateTime.UtcNow;
            sequence.LastGeneratedBy = user;
            sequence.ModifiedAt = DateTime.UtcNow;
            sequence.ModifiedBy = user;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Format batch number: SSTTYYYWWNNNN (12 digits)
            var batchNumber = FormatBatchNumber(siteId, batchType, year, week, sequence.CurrentSequence);

            _logger.LogInformation(
                "Generated batch number {BatchNumber} for Site {SiteId}, Type {BatchType}, Week {Year}-W{Week}",
                batchNumber, siteId, batchType, year, week);

            return batchNumber;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Failed to generate batch number for Site {SiteId}, Type {BatchType}, Year {Year}, Week {Week}",
                siteId, batchType, year, week);
            throw;
        }
    }

    /// <inheritdoc />
    public bool ValidateBatchNumber(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        // Must be exactly 12 digits
        if (batchNumber.Length != 12)
            return false;

        // Must be all numeric
        if (!long.TryParse(batchNumber, out _))
            return false;

        // Parse and validate components
        try
        {
            var components = ParseBatchNumber(batchNumber);

            // Validate site ID (01-99)
            if (components.SiteId < 1 || components.SiteId > 99)
                return false;

            // Validate batch type (must be defined enum value)
            if (!Enum.IsDefined(typeof(BatchType), components.BatchType))
                return false;

            // Validate year (reasonable range: 20-99 for 2020-2099)
            if (components.Year < 20 || components.Year > 99)
                return false;

            // Validate week (01-53)
            if (components.Week < 1 || components.Week > 53)
                return false;

            // Validate sequence (0001-9999)
            if (components.Sequence < 1 || components.Sequence > 9999)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public BatchNumberComponents ParseBatchNumber(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number cannot be null or empty.", nameof(batchNumber));
        }

        if (batchNumber.Length != 12)
        {
            throw new ArgumentException(
                $"Batch number must be exactly 12 digits. Got {batchNumber.Length} characters.",
                nameof(batchNumber));
        }

        if (!long.TryParse(batchNumber, out _))
        {
            throw new ArgumentException(
                "Batch number must be all numeric.",
                nameof(batchNumber));
        }

        // Extract components: SSTTYYYWWNNNN (12 digits)
        // SS: Site ID (positions 0-1)
        // TT: Batch Type (positions 2-3)
        // YY: Year (positions 4-5)
        // WW: Week (positions 6-7)
        // NNNN: Sequence (positions 8-11)

        var siteIdStr = batchNumber.Substring(0, 2);
        var batchTypeStr = batchNumber.Substring(2, 2);
        var yearStr = batchNumber.Substring(4, 2);
        var weekStr = batchNumber.Substring(6, 2);
        var sequenceStr = batchNumber.Substring(8, 4);

        // Parse site ID
        if (!int.TryParse(siteIdStr, out int siteId) || siteId < 1 || siteId > 99)
        {
            throw new ArgumentException(
                $"Invalid site ID '{siteIdStr}' in batch number. Must be 01-99.",
                nameof(batchNumber));
        }

        // Parse batch type
        if (!int.TryParse(batchTypeStr, out int batchTypeInt))
        {
            throw new ArgumentException(
                $"Invalid batch type '{batchTypeStr}' in batch number.",
                nameof(batchNumber));
        }

        if (!Enum.IsDefined(typeof(BatchType), batchTypeInt))
        {
            throw new ArgumentException(
                $"Unknown batch type code '{batchTypeInt}'. Valid codes: 10, 20, 30, 40, 50, 60, 70, 80, 90.",
                nameof(batchNumber));
        }

        var batchType = (BatchType)batchTypeInt;

        // Parse year (2-digit)
        if (!int.TryParse(yearStr, out int year) || year < 20 || year > 99)
        {
            throw new ArgumentException(
                $"Invalid year '{yearStr}' in batch number. Must be 20-99.",
                nameof(batchNumber));
        }

        // Parse week (01-53)
        if (!int.TryParse(weekStr, out int week) || week < 1 || week > 53)
        {
            throw new ArgumentException(
                $"Invalid week '{weekStr}' in batch number. Must be 01-53.",
                nameof(batchNumber));
        }

        // Parse sequence
        if (!int.TryParse(sequenceStr, out int sequence) || sequence < 1)
        {
            throw new ArgumentException(
                $"Invalid sequence '{sequenceStr}' in batch number. Must be 0001-9999.",
                nameof(batchNumber));
        }

        return new BatchNumberComponents
        {
            SiteId = siteId,
            BatchType = batchType,
            Year = year,
            Week = week,
            Sequence = sequence,
            OriginalBatchNumber = batchNumber
        };
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentSequenceAsync(int siteId, BatchType batchType, DateTime batchDate)
    {
        // Convert date to week start date
        var year = ISOWeek.GetYear(batchDate);
        var week = ISOWeek.GetWeekOfYear(batchDate);
        var weekDate = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);

        var sequence = await _context.BatchNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == siteId &&
                s.BatchType == batchType &&
                s.BatchDate == weekDate &&
                !s.IsDeleted);

        return sequence?.CurrentSequence ?? 0;
    }

    /// <inheritdoc />
    public async Task<bool> BatchNumberExistsAsync(string batchNumber)
    {
        if (!ValidateBatchNumber(batchNumber))
            return false;

        var components = ParseBatchNumber(batchNumber);

        // Convert year/week to week start date
        var weekDate = ISOWeek.ToDateTime(2000 + components.Year, components.Week, DayOfWeek.Monday);

        var sequence = await _context.BatchNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == components.SiteId &&
                s.BatchType == components.BatchType &&
                s.BatchDate == weekDate &&
                !s.IsDeleted);

        // If no sequence exists, batch number doesn't exist
        if (sequence == null)
            return false;

        // If the parsed sequence is <= current sequence, it was generated
        return components.Sequence <= sequence.CurrentSequence;
    }

    /// <summary>
    /// Formats a batch number from its components.
    /// </summary>
    /// <param name="siteId">Site ID (1-99)</param>
    /// <param name="batchType">Batch type</param>
    /// <param name="year">Full year (e.g., 2025)</param>
    /// <param name="week">ISO week number (1-53)</param>
    /// <param name="sequence">Weekly sequence (1-9999)</param>
    /// <returns>12-digit batch number (SSTTYYYWWNNNN)</returns>
    private static string FormatBatchNumber(int siteId, BatchType batchType, int year, int week, int sequence)
    {
        // Format: SSTTYYYWWNNNN (12 digits)
        // Use last 2 digits of year
        var yearShort = year % 100;

        return $"{siteId:D2}" +                              // SS: Site ID (2 digits)
               $"{(int)batchType:D2}" +                      // TT: Batch Type (2 digits)
               $"{yearShort:D2}" +                           // YY: Year (2 digits)
               $"{week:D2}" +                                // WW: Week (2 digits)
               $"{sequence:D4}";                             // NNNN: Sequence (4 digits)
    }
}
