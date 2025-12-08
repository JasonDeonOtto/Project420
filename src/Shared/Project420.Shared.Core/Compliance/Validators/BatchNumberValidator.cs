using System.Text.RegularExpressions;

namespace Project420.Shared.Core.Compliance.Validators;

/// <summary>
/// Validator for cannabis batch/lot numbers.
/// Ensures compliance with SAHPRA seed-to-sale traceability requirements.
/// </summary>
public static class BatchNumberValidator
{
    /// <summary>
    /// Validates batch number format and length.
    /// </summary>
    /// <param name="batchNumber">Batch or lot number</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <remarks>
    /// SAHPRA Requirement: Unique batch/lot number for seed-to-sale traceability.
    /// Suggested format: BATCH-YYYY-MM-XXX (e.g., "BATCH-2024-12-001")
    /// </remarks>
    public static bool IsValidBatchNumber(string? batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        if (batchNumber.Length > CannabisComplianceConstants.BATCH_NUMBER_MAX_LENGTH)
            return false;

        // Allow any alphanumeric with hyphens/underscores (flexible format)
        return Regex.IsMatch(batchNumber, @"^[A-Z0-9\-_]+$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Checks if batch number follows the suggested standard format.
    /// Format: BATCH-YYYY-MM-XXX
    /// </summary>
    /// <param name="batchNumber">Batch or lot number</param>
    /// <returns>True if follows suggested format, false otherwise</returns>
    public static bool FollowsSuggestedFormat(string? batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        return Regex.IsMatch(batchNumber, CannabisComplianceConstants.BATCH_NUMBER_SUGGESTED_FORMAT, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Generates a batch number following the suggested format.
    /// Format: BATCH-YYYY-MM-XXX
    /// </summary>
    /// <param name="sequenceNumber">Sequential number for this month</param>
    /// <returns>Generated batch number</returns>
    /// <example>
    /// GenerateBatchNumber(1) → "BATCH-2024-12-001"
    /// </example>
    public static string GenerateBatchNumber(int sequenceNumber)
    {
        var now = DateTime.UtcNow;
        return $"BATCH-{now:yyyy-MM}-{sequenceNumber:D3}";
    }

    /// <summary>
    /// Extracts date information from batch number (if in suggested format).
    /// </summary>
    /// <param name="batchNumber">Batch number in suggested format</param>
    /// <returns>Date if parseable, null otherwise</returns>
    public static DateTime? ExtractBatchDate(string? batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return null;

        var match = Regex.Match(batchNumber, @"BATCH-(\d{4})-(\d{2})-\d+", RegexOptions.IgnoreCase);

        if (match.Success && int.TryParse(match.Groups[1].Value, out var year) && int.TryParse(match.Groups[2].Value, out var month))
        {
            try
            {
                return new DateTime(year, month, 1);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
