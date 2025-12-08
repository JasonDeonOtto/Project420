using Project420.Shared.Core.Compliance.Validators;

namespace Project420.Shared.Core.Compliance.Services;

/// <summary>
/// Service for cannabis compliance business logic.
/// Centralizes all compliance-related operations used across modules.
/// </summary>
public class CannabisComplianceService : ICannabisComplianceService
{
    // ============================================================
    // AGE VERIFICATION
    // ============================================================

    /// <summary>
    /// Verifies if customer meets minimum age requirement (18+).
    /// </summary>
    public bool IsAgeVerified(string idNumber)
    {
        return AgeVerificationValidator.IsAtLeast18YearsOld(idNumber);
    }

    /// <summary>
    /// Verifies if customer meets minimum age requirement from date of birth.
    /// </summary>
    public bool IsAgeVerified(DateTime dateOfBirth)
    {
        return AgeVerificationValidator.IsAtLeast18YearsOld(dateOfBirth);
    }

    /// <summary>
    /// Calculates age from SA ID number.
    /// </summary>
    public int? CalculateAge(string idNumber)
    {
        try
        {
            var dateOfBirth = AgeVerificationValidator.ExtractDateOfBirth(idNumber);
            return AgeVerificationValidator.CalculateAge(dateOfBirth);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts date of birth from SA ID number.
    /// </summary>
    public DateTime? ExtractDateOfBirth(string idNumber)
    {
        try
        {
            return AgeVerificationValidator.ExtractDateOfBirth(idNumber);
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // CANNABINOID CONTENT VALIDATION
    // ============================================================

    /// <summary>
    /// Validates cannabinoid content format (THC or CBD).
    /// </summary>
    public bool IsValidCannabinoidContent(string? content)
    {
        return CannabisContentValidator.IsValidCannabinoidFormat(content);
    }

    /// <summary>
    /// Checks if product has required cannabinoid labeling (THC and/or CBD).
    /// </summary>
    public bool HasRequiredCannabinoidLabeling(string? thcContent, string? cbdContent)
    {
        return CannabisContentValidator.HasRequiredCannabinoidContent(thcContent, cbdContent);
    }

    // ============================================================
    // BATCH NUMBER VALIDATION & GENERATION
    // ============================================================

    /// <summary>
    /// Validates batch number format.
    /// </summary>
    public bool IsValidBatchNumber(string? batchNumber)
    {
        return BatchNumberValidator.IsValidBatchNumber(batchNumber);
    }

    /// <summary>
    /// Generates a compliant batch number in standard format.
    /// Format: BATCH-YYYY-MM-XXX
    /// </summary>
    public string GenerateBatchNumber(int sequenceNumber)
    {
        return BatchNumberValidator.GenerateBatchNumber(sequenceNumber);
    }

    /// <summary>
    /// Extracts batch date from batch number (if in standard format).
    /// </summary>
    public DateTime? ExtractBatchDate(string? batchNumber)
    {
        return BatchNumberValidator.ExtractBatchDate(batchNumber);
    }

    // ============================================================
    // LAB TEST VALIDATION
    // ============================================================

    /// <summary>
    /// Validates that lab test date is not in the future.
    /// </summary>
    public bool IsValidLabTestDate(DateTime? labTestDate)
    {
        return LabTestDateValidator.IsValidLabTestDate(labTestDate);
    }

    /// <summary>
    /// Checks if lab test results are still current/valid.
    /// </summary>
    public bool IsLabTestCurrent(DateTime? labTestDate, int? validityDays = null)
    {
        var days = validityDays ?? CannabisComplianceConstants.LAB_TEST_VALIDITY_DAYS;
        return LabTestDateValidator.IsLabTestCurrent(labTestDate, days);
    }

    /// <summary>
    /// Checks if lab test results are expiring soon.
    /// </summary>
    public bool IsLabTestExpiringSoon(DateTime? labTestDate, int? alertDays = null)
    {
        var days = alertDays ?? CannabisComplianceConstants.EXPIRY_ALERT_DAYS;
        return LabTestDateValidator.IsLabTestExpiringSoon(labTestDate, days);
    }

    // ============================================================
    // PRODUCT EXPIRY VALIDATION
    // ============================================================

    /// <summary>
    /// Validates that product expiry date is in the future.
    /// </summary>
    public bool IsValidExpiryDate(DateTime? expiryDate)
    {
        return LabTestDateValidator.IsValidExpiryDate(expiryDate);
    }

    /// <summary>
    /// Checks if product is expiring soon.
    /// </summary>
    public bool IsProductExpiringSoon(DateTime? expiryDate, int? alertDays = null)
    {
        var days = alertDays ?? CannabisComplianceConstants.EXPIRY_ALERT_DAYS;
        return LabTestDateValidator.IsProductExpiringSoon(expiryDate, days);
    }

    /// <summary>
    /// Checks if product has already expired.
    /// </summary>
    public bool IsProductExpired(DateTime? expiryDate)
    {
        return LabTestDateValidator.IsProductExpired(expiryDate);
    }

    // ============================================================
    // COMPLIANCE STATUS
    // ============================================================

    /// <summary>
    /// Checks if a product requires age verification.
    /// All cannabis products (with THC or CBD) require age verification.
    /// </summary>
    public bool RequiresAgeVerification(string? thcContent, string? cbdContent)
    {
        return CannabisContentValidator.HasRequiredCannabinoidContent(thcContent, cbdContent);
    }

    /// <summary>
    /// Validates overall product compliance status.
    /// </summary>
    public bool IsProductCompliant(string? thcContent, string? cbdContent, string? batchNumber, DateTime? labTestDate)
    {
        // Must have cannabinoid content (THC and/or CBD)
        if (!HasRequiredCannabinoidLabeling(thcContent, cbdContent))
            return false;

        // Batch number is optional but must be valid if provided
        if (!string.IsNullOrWhiteSpace(batchNumber) && !IsValidBatchNumber(batchNumber))
            return false;

        // Lab test date is optional but must be valid if provided
        if (labTestDate.HasValue && !IsValidLabTestDate(labTestDate))
            return false;

        return true;
    }
}
