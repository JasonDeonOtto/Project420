using System.Text.RegularExpressions;

namespace Project420.Shared.Core.Compliance.Validators;

/// <summary>
/// Validator for cannabis cannabinoid content (THC/CBD).
/// Ensures compliance with SA Cannabis Act labeling requirements.
/// </summary>
public static class CannabisContentValidator
{
    /// <summary>
    /// Validates cannabinoid content format (THC or CBD).
    /// </summary>
    /// <param name="content">Cannabinoid content string (e.g., "15%", "150mg", "15-18%")</param>
    /// <returns>True if valid format, false otherwise</returns>
    /// <remarks>
    /// Valid formats:
    /// - "15%" - Percentage
    /// - "150mg" - Milligrams
    /// - "15-18%" - Range percentage
    /// - "0.5%" - Decimal percentage
    /// </remarks>
    public static bool IsValidCannabinoidFormat(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        if (content.Length > CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH)
            return false;

        return Regex.IsMatch(content, CannabisComplianceConstants.CANNABINOID_CONTENT_PATTERN);
    }

    /// <summary>
    /// Checks if at least one cannabinoid (THC or CBD) is specified.
    /// Cannabis Act requirement: All cannabis products must have cannabinoid content labeled.
    /// </summary>
    /// <param name="thcContent">THC content</param>
    /// <param name="cbdContent">CBD content</param>
    /// <returns>True if at least one cannabinoid is specified</returns>
    public static bool HasRequiredCannabinoidContent(string? thcContent, string? cbdContent)
    {
        return !string.IsNullOrWhiteSpace(thcContent) || !string.IsNullOrWhiteSpace(cbdContent);
    }

    /// <summary>
    /// Validates that cannabinoid content is within reasonable limits.
    /// </summary>
    /// <param name="content">Cannabinoid content string</param>
    /// <param name="maxPercentage">Maximum allowed percentage (default 100%)</param>
    /// <returns>True if within limits, false otherwise</returns>
    /// <remarks>
    /// THC in cannabis flower: typically 10-30%
    /// CBD in hemp: typically 1-20%
    /// Concentrates can be higher (60-90%)
    /// </remarks>
    public static bool IsWithinReasonableLimits(string? content, decimal maxPercentage = 100m)
    {
        if (string.IsNullOrWhiteSpace(content))
            return true; // Optional field

        // Extract numeric value from content
        var numericPart = Regex.Match(content, @"[\d.]+").Value;

        if (decimal.TryParse(numericPart, out var value))
        {
            return value >= 0 && value <= maxPercentage;
        }

        return false;
    }
}
