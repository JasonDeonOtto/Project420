using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique batch numbers following the 16-digit format: SSTTYYYYMMDDNNNN
/// </summary>
/// <remarks>
/// Batch Number Format (16 digits): SSTTYYYYMMDDNNNN
/// - SS: Site ID (01-99)
/// - TT: Batch Type code (10=Production, 20=Transfer, etc.)
/// - YYYYMMDD: Batch date
/// - NNNN: Daily sequence per site/type (0001-9999)
///
/// Thread Safety:
/// - Uses database-level locking via transactions
/// - Safe for multi-instance deployments
///
/// Cannabis Compliance:
/// - SAHPRA seed-to-sale traceability
/// - Unique batch identification for audit trail
/// - Date component enables FIFO/FEFO inventory management
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
        var user = requestedBy ?? _defaultUser;

        _logger.LogDebug(
            "Generating batch number for Site {SiteId}, Type {BatchType}, Date {Date}",
            siteId, batchType, date);

        // Use a transaction for atomic increment
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Find or create sequence record for this site/type/date
            var sequence = await _context.BatchNumberSequences
                .FirstOrDefaultAsync(s =>
                    s.SiteId == siteId &&
                    s.BatchType == batchType &&
                    s.BatchDate == date &&
                    !s.IsDeleted);

            if (sequence == null)
            {
                // Create new sequence for this combination
                sequence = new BatchNumberSequence
                {
                    SiteId = siteId,
                    BatchType = batchType,
                    BatchDate = date,
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
                    $"Site {siteId}, Type {batchType}, Date {date:yyyy-MM-dd}. " +
                    "Cannot generate more batch numbers for this combination today.");
            }

            // Increment sequence
            sequence.CurrentSequence++;
            sequence.LastGeneratedAt = DateTime.UtcNow;
            sequence.LastGeneratedBy = user;
            sequence.ModifiedAt = DateTime.UtcNow;
            sequence.ModifiedBy = user;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Format batch number: SSTTYYYYMMDDNNNN
            var batchNumber = FormatBatchNumber(siteId, batchType, date, sequence.CurrentSequence);

            _logger.LogInformation(
                "Generated batch number {BatchNumber} for Site {SiteId}, Type {BatchType}",
                batchNumber, siteId, batchType);

            return batchNumber;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Failed to generate batch number for Site {SiteId}, Type {BatchType}, Date {Date}",
                siteId, batchType, date);
            throw;
        }
    }

    /// <inheritdoc />
    public bool ValidateBatchNumber(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        // Must be exactly 16 digits
        if (batchNumber.Length != 16)
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

            // Validate sequence (0001-9999)
            if (components.Sequence < 1 || components.Sequence > 9999)
                return false;

            // Validate date (reasonable range)
            if (components.BatchDate < new DateTime(2020, 1, 1) ||
                components.BatchDate > DateTime.Today.AddYears(1))
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

        if (batchNumber.Length != 16)
        {
            throw new ArgumentException(
                $"Batch number must be exactly 16 digits. Got {batchNumber.Length} characters.",
                nameof(batchNumber));
        }

        if (!long.TryParse(batchNumber, out _))
        {
            throw new ArgumentException(
                "Batch number must be all numeric.",
                nameof(batchNumber));
        }

        // Extract components: SSTTYYYYMMDDNNNN
        // SS: Site ID (positions 0-1)
        // TT: Batch Type (positions 2-3)
        // YYYYMMDD: Date (positions 4-11)
        // NNNN: Sequence (positions 12-15)

        var siteIdStr = batchNumber.Substring(0, 2);
        var batchTypeStr = batchNumber.Substring(2, 2);
        var dateStr = batchNumber.Substring(4, 8);
        var sequenceStr = batchNumber.Substring(12, 4);

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

        // Parse date
        if (!DateTime.TryParseExact(dateStr, "yyyyMMdd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime batchDate))
        {
            throw new ArgumentException(
                $"Invalid date '{dateStr}' in batch number. Expected YYYYMMDD format.",
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
            BatchDate = batchDate,
            Sequence = sequence,
            OriginalBatchNumber = batchNumber
        };
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentSequenceAsync(int siteId, BatchType batchType, DateTime batchDate)
    {
        var date = batchDate.Date;

        var sequence = await _context.BatchNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == siteId &&
                s.BatchType == batchType &&
                s.BatchDate == date &&
                !s.IsDeleted);

        return sequence?.CurrentSequence ?? 0;
    }

    /// <inheritdoc />
    public async Task<bool> BatchNumberExistsAsync(string batchNumber)
    {
        if (!ValidateBatchNumber(batchNumber))
            return false;

        var components = ParseBatchNumber(batchNumber);

        var sequence = await _context.BatchNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == components.SiteId &&
                s.BatchType == components.BatchType &&
                s.BatchDate == components.BatchDate &&
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
    private static string FormatBatchNumber(int siteId, BatchType batchType, DateTime date, int sequence)
    {
        // Format: SSTTYYYYMMDDNNNN (16 digits)
        return $"{siteId:D2}" +                              // SS: Site ID (2 digits)
               $"{(int)batchType:D2}" +                      // TT: Batch Type (2 digits)
               $"{date:yyyyMMdd}" +                          // YYYYMMDD: Date (8 digits)
               $"{sequence:D4}";                             // NNNN: Sequence (4 digits)
    }
}
