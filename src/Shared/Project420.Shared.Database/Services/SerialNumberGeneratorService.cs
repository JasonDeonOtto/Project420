using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Utilities;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating unique serial numbers in two formats:
/// - Full Serial Number (31 digits including check) for QR codes
/// - Short Serial Number (13 digits) for barcodes
/// </summary>
/// <remarks>
/// Full Serial Format (31 digits):
/// SS(2) + SSS(3) + TT(2) + YYYYMMDD(8) + BBBB(4) + UUUUU(5) + WWWW(4) + Q(1) + C(1) = 30 + 1 check
///
/// Short Serial Format (13 digits):
/// SS(2) + YYMMDD(6) + NNNNN(5) = 13
///
/// Thread Safety:
/// - Uses database transactions for atomic sequence increments
/// - Safe for multi-instance deployments
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
    private readonly string _defaultUser;

    public SerialNumberGeneratorService(
        IBusinessDbContext context,
        ILogger<SerialNumberGeneratorService> logger,
        string defaultUser = "SYSTEM")
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultUser = defaultUser;
    }

    /// <inheritdoc />
    public async Task<SerialNumberResult> GenerateSerialNumberAsync(
        int siteId,
        int strainCode,
        BatchType batchType,
        DateTime? productionDate = null,
        int? batchSequence = null,
        decimal weightGrams = 0m,
        int packQty = 1,
        string? requestedBy = null)
    {
        // Validate inputs
        ValidateInputs(siteId, strainCode, weightGrams, packQty);

        var date = productionDate?.Date ?? DateTime.Today;
        var user = requestedBy ?? _defaultUser;
        var batchSeq = batchSequence ?? 1;

        _logger.LogDebug(
            "Generating serial number for Site {SiteId}, Strain {StrainCode}, Type {BatchType}, Date {Date}",
            siteId, strainCode, batchType, date);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get next unit sequence for this batch
            var unitSequence = await GetNextUnitSequenceAsync(siteId, batchType, date, batchSeq, user);

            // Get next daily sequence for short SN
            var dailySequence = await GetNextDailySequenceAsync(siteId, date, user);

            // Build the serial numbers
            var fullSN = BuildFullSerialNumber(
                siteId, strainCode, batchType, date, batchSeq, unitSequence, weightGrams, packQty);
            var shortSN = BuildShortSerialNumber(siteId, date, dailySequence);

            // Store the serial number record
            var serialRecord = new SerialNumber
            {
                FullSerialNumber = fullSN,
                ShortSerialNumber = shortSN,
                SiteId = siteId,
                StrainCode = strainCode,
                BatchType = batchType,
                ProductionDate = date,
                BatchSequence = batchSeq,
                UnitSequence = unitSequence,
                WeightGrams = weightGrams,
                PackQty = packQty,
                Status = SerialStatus.Created,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };

            _context.SerialNumbers.Add(serialRecord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = new SerialNumberResult
            {
                FullSerialNumber = fullSN,
                ShortSerialNumber = shortSN,
                SiteId = siteId,
                StrainCode = strainCode,
                StrainType = GetStrainType(strainCode),
                BatchType = batchType,
                ProductionDate = date,
                BatchSequence = batchSeq,
                UnitSequence = unitSequence,
                WeightGrams = weightGrams,
                PackQty = packQty,
                CheckDigit = LuhnCheckDigit.ExtractCheckDigit(fullSN)
            };

            _logger.LogInformation(
                "Generated serial number {ShortSN} (Full: {FullSN}) for Site {SiteId}",
                shortSN, fullSN, siteId);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Failed to generate serial number for Site {SiteId}, Strain {StrainCode}",
                siteId, strainCode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<SerialNumberResult>> GenerateBulkSerialNumbersAsync(
        int count,
        int siteId,
        int strainCode,
        BatchType batchType,
        DateTime? productionDate = null,
        int? batchSequence = null,
        decimal weightGrams = 0m,
        int packQty = 1,
        string? requestedBy = null)
    {
        if (count < 1 || count > 10000)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be between 1 and 10,000.");
        }

        ValidateInputs(siteId, strainCode, weightGrams, packQty);

        var results = new List<SerialNumberResult>(count);

        _logger.LogInformation(
            "Generating {Count} serial numbers for Site {SiteId}, Strain {StrainCode}",
            count, siteId, strainCode);

        for (int i = 0; i < count; i++)
        {
            var result = await GenerateSerialNumberAsync(
                siteId, strainCode, batchType, productionDate,
                batchSequence, weightGrams, packQty, requestedBy);
            results.Add(result);
        }

        _logger.LogInformation(
            "Generated {Count} serial numbers successfully", count);

        return results;
    }

    /// <inheritdoc />
    public bool ValidateFullSerialNumber(string fullSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(fullSerialNumber))
            return false;

        // Must be exactly 30 digits: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1)
        if (fullSerialNumber.Length != 30)
            return false;

        // Must be all numeric
        if (!IsNumeric(fullSerialNumber))
            return false;

        // Validate Luhn check digit
        if (!LuhnCheckDigit.Validate(fullSerialNumber))
            return false;

        // Parse and validate components
        try
        {
            var components = ParseFullSerialNumber(fullSerialNumber);
            return components.IsCheckDigitValid;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool ValidateShortSerialNumber(string shortSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(shortSerialNumber))
            return false;

        // Must be exactly 13 digits
        if (shortSerialNumber.Length != 13)
            return false;

        // Must be all numeric
        if (!IsNumeric(shortSerialNumber))
            return false;

        // Parse and validate components
        try
        {
            ParseShortSerialNumber(shortSerialNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public FullSerialNumberComponents ParseFullSerialNumber(string fullSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(fullSerialNumber))
            throw new ArgumentException("Serial number cannot be null or empty.", nameof(fullSerialNumber));

        // Format: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1) = 30 digits
        if (fullSerialNumber.Length != 30)
            throw new ArgumentException($"Full serial number must be 30 digits. Got {fullSerialNumber.Length}.", nameof(fullSerialNumber));

        if (!IsNumeric(fullSerialNumber))
            throw new ArgumentException("Serial number must be all numeric.", nameof(fullSerialNumber));

        // Parse components: SS(2)+SSS(3)+TT(2)+YYYYMMDD(8)+BBBB(4)+UUUUU(5)+WWWW(4)+Q(1)+C(1)
        int pos = 0;

        // Site ID (2 digits)
        var siteId = int.Parse(fullSerialNumber.Substring(pos, 2));
        pos += 2;

        // Strain Code (3 digits)
        var strainCode = int.Parse(fullSerialNumber.Substring(pos, 3));
        pos += 3;

        // Batch Type (2 digits)
        var batchTypeInt = int.Parse(fullSerialNumber.Substring(pos, 2));
        pos += 2;

        if (!Enum.IsDefined(typeof(BatchType), batchTypeInt))
            throw new ArgumentException($"Unknown batch type code: {batchTypeInt}");
        var batchType = (BatchType)batchTypeInt;

        // Date (8 digits)
        var dateStr = fullSerialNumber.Substring(pos, 8);
        pos += 8;

        if (!DateTime.TryParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var productionDate))
            throw new ArgumentException($"Invalid date: {dateStr}");

        // Batch Sequence (4 digits)
        var batchSequence = int.Parse(fullSerialNumber.Substring(pos, 4));
        pos += 4;

        // Unit Sequence (5 digits)
        var unitSequence = int.Parse(fullSerialNumber.Substring(pos, 5));
        pos += 5;

        // Weight (4 digits, in tenths of grams)
        var weightTenths = int.Parse(fullSerialNumber.Substring(pos, 4));
        var weightGrams = weightTenths / 10m;
        pos += 4;

        // Pack Qty (1 digit)
        var packQty = int.Parse(fullSerialNumber.Substring(pos, 1));
        pos += 1;

        // Check Digit (1 digit)
        var checkDigit = int.Parse(fullSerialNumber.Substring(pos, 1));

        // Validate check digit
        var isValid = LuhnCheckDigit.Validate(fullSerialNumber);

        return new FullSerialNumberComponents
        {
            SiteId = siteId,
            StrainCode = strainCode,
            StrainType = GetStrainType(strainCode),
            BatchType = batchType,
            ProductionDate = productionDate,
            BatchSequence = batchSequence,
            UnitSequence = unitSequence,
            WeightGrams = weightGrams,
            PackQty = packQty,
            CheckDigit = checkDigit,
            OriginalSerialNumber = fullSerialNumber,
            IsCheckDigitValid = isValid
        };
    }

    /// <inheritdoc />
    public ShortSerialNumberComponents ParseShortSerialNumber(string shortSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(shortSerialNumber))
            throw new ArgumentException("Serial number cannot be null or empty.", nameof(shortSerialNumber));

        if (shortSerialNumber.Length != 13)
            throw new ArgumentException($"Short serial number must be 13 digits. Got {shortSerialNumber.Length}.", nameof(shortSerialNumber));

        if (!IsNumeric(shortSerialNumber))
            throw new ArgumentException("Serial number must be all numeric.", nameof(shortSerialNumber));

        // Parse components: SS(2)+YYMMDD(6)+NNNNN(5)
        int pos = 0;

        // Site ID (2 digits)
        var siteId = int.Parse(shortSerialNumber.Substring(pos, 2));
        pos += 2;

        // Date (6 digits: YYMMDD)
        var dateStr = shortSerialNumber.Substring(pos, 6);
        pos += 6;

        if (!DateTime.TryParseExact(dateStr, "yyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var productionDate))
            throw new ArgumentException($"Invalid date: {dateStr}");

        // Sequence (5 digits)
        var sequence = int.Parse(shortSerialNumber.Substring(pos, 5));

        return new ShortSerialNumberComponents
        {
            SiteId = siteId,
            ProductionDate = productionDate,
            Sequence = sequence,
            OriginalSerialNumber = shortSerialNumber
        };
    }

    /// <inheritdoc />
    public string GetStrainType(int strainCode)
    {
        if (strainCode < 100 || strainCode > 999)
            return "Unknown";

        int typeDigit = strainCode / 100;
        return typeDigit switch
        {
            1 => "Sativa",
            2 => "Indica",
            3 => "Hybrid",
            4 => "CBD",
            _ => "Unknown"
        };
    }

    // ==========================================
    // Private Helper Methods
    // ==========================================

    private void ValidateInputs(int siteId, int strainCode, decimal weightGrams, int packQty)
    {
        if (siteId < 1 || siteId > 99)
            throw new ArgumentOutOfRangeException(nameof(siteId), "Site ID must be 1-99.");

        if (strainCode < 100 || strainCode > 999)
            throw new ArgumentOutOfRangeException(nameof(strainCode), "Strain code must be 100-999.");

        if (weightGrams < 0 || weightGrams > 999.9m)
            throw new ArgumentOutOfRangeException(nameof(weightGrams), "Weight must be 0-999.9 grams.");

        if (packQty < 0 || packQty > 9)
            throw new ArgumentOutOfRangeException(nameof(packQty), "Pack quantity must be 0-9.");
    }

    private async Task<int> GetNextUnitSequenceAsync(
        int siteId, BatchType batchType, DateTime date, int batchSeq, string user)
    {
        var sequence = await _context.SerialNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == siteId &&
                s.SequenceType == "Unit" &&
                s.BatchType == batchType &&
                s.ProductionDate == date &&
                s.BatchSequence == batchSeq &&
                !s.IsDeleted);

        if (sequence == null)
        {
            sequence = new SerialNumberSequence
            {
                SiteId = siteId,
                SequenceType = "Unit",
                BatchType = batchType,
                ProductionDate = date,
                BatchSequence = batchSeq,
                CurrentSequence = 0,
                MaxSequence = 99999,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };
            _context.SerialNumberSequences.Add(sequence);
        }

        if (sequence.CurrentSequence >= sequence.MaxSequence)
        {
            throw new InvalidOperationException(
                $"Maximum unit sequence ({sequence.MaxSequence}) reached for " +
                $"Site {siteId}, Type {batchType}, Date {date:yyyy-MM-dd}, Batch {batchSeq}.");
        }

        sequence.CurrentSequence++;
        sequence.LastGeneratedAt = DateTime.UtcNow;
        sequence.LastGeneratedBy = user;
        sequence.ModifiedAt = DateTime.UtcNow;
        sequence.ModifiedBy = user;

        return sequence.CurrentSequence;
    }

    private async Task<int> GetNextDailySequenceAsync(int siteId, DateTime date, string user)
    {
        var sequence = await _context.SerialNumberSequences
            .FirstOrDefaultAsync(s =>
                s.SiteId == siteId &&
                s.SequenceType == "Daily" &&
                s.ProductionDate == date &&
                !s.IsDeleted);

        if (sequence == null)
        {
            sequence = new SerialNumberSequence
            {
                SiteId = siteId,
                SequenceType = "Daily",
                ProductionDate = date,
                CurrentSequence = 0,
                MaxSequence = 99999,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };
            _context.SerialNumberSequences.Add(sequence);
        }

        if (sequence.CurrentSequence >= sequence.MaxSequence)
        {
            throw new InvalidOperationException(
                $"Maximum daily sequence ({sequence.MaxSequence}) reached for " +
                $"Site {siteId}, Date {date:yyyy-MM-dd}.");
        }

        sequence.CurrentSequence++;
        sequence.LastGeneratedAt = DateTime.UtcNow;
        sequence.LastGeneratedBy = user;
        sequence.ModifiedAt = DateTime.UtcNow;
        sequence.ModifiedBy = user;

        return sequence.CurrentSequence;
    }

    private string BuildFullSerialNumber(
        int siteId, int strainCode, BatchType batchType, DateTime date,
        int batchSeq, int unitSeq, decimal weightGrams, int packQty)
    {
        // Convert weight to tenths of grams (4 digits)
        var weightTenths = (int)(weightGrams * 10);
        if (weightTenths > 9999) weightTenths = 9999;

        // Build the 30-digit base (before check digit)
        var baseSN = $"{siteId:D2}" +           // SS: Site (2)
                     $"{strainCode:D3}" +        // SSS: Strain (3)
                     $"{(int)batchType:D2}" +    // TT: Type (2)
                     $"{date:yyyyMMdd}" +        // YYYYMMDD (8)
                     $"{batchSeq:D4}" +          // BBBB: Batch (4)
                     $"{unitSeq:D5}" +           // UUUUU: Unit (5)
                     $"{weightTenths:D4}" +      // WWWW: Weight (4)
                     $"{packQty}";               // Q: Pack (1)

        // Add Luhn check digit
        return LuhnCheckDigit.AppendCheckDigit(baseSN);
    }

    private string BuildShortSerialNumber(int siteId, DateTime date, int sequence)
    {
        // Format: SS + YYMMDD + NNNNN (13 digits)
        return $"{siteId:D2}" +
               $"{date:yyMMdd}" +
               $"{sequence:D5}";
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
