namespace Project420.Shared.Core.Compliance.Services;

/// <summary>
/// Service interface for cannabis compliance business logic.
/// Centralizes all compliance-related operations used across modules.
/// </summary>
/// <remarks>
/// This service provides:
/// - Age verification (18+ requirement)
/// - Cannabinoid content validation (THC/CBD)
/// - Batch number validation and generation
/// - Lab test date validation
/// - Product expiry checks
/// - Compliance status reporting
/// </remarks>
public interface ICannabisComplianceService
{
    // ============================================================
    // AGE VERIFICATION
    // ============================================================

    /// <summary>
    /// Verifies if customer meets minimum age requirement (18+).
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>True if 18 or older, false otherwise</returns>
    bool IsAgeVerified(string idNumber);

    /// <summary>
    /// Verifies if customer meets minimum age requirement from date of birth.
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>True if 18 or older, false otherwise</returns>
    bool IsAgeVerified(DateTime dateOfBirth);

    /// <summary>
    /// Calculates age from SA ID number.
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>Age in years, null if invalid ID</returns>
    int? CalculateAge(string idNumber);

    /// <summary>
    /// Extracts date of birth from SA ID number.
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>Date of birth, null if invalid ID</returns>
    DateTime? ExtractDateOfBirth(string idNumber);

    // ============================================================
    // CANNABINOID CONTENT VALIDATION
    // ============================================================

    /// <summary>
    /// Validates cannabinoid content format (THC or CBD).
    /// </summary>
    /// <param name="content">Cannabinoid content (e.g., "15%", "150mg")</param>
    /// <returns>True if valid format, false otherwise</returns>
    bool IsValidCannabinoidContent(string? content);

    /// <summary>
    /// Checks if product has required cannabinoid labeling (THC and/or CBD).
    /// </summary>
    /// <param name="thcContent">THC content</param>
    /// <param name="cbdContent">CBD content</param>
    /// <returns>True if at least one cannabinoid is specified</returns>
    bool HasRequiredCannabinoidLabeling(string? thcContent, string? cbdContent);

    // ============================================================
    // BATCH NUMBER VALIDATION & GENERATION
    // ============================================================

    /// <summary>
    /// Validates batch number format.
    /// </summary>
    /// <param name="batchNumber">Batch or lot number</param>
    /// <returns>True if valid format, false otherwise</returns>
    bool IsValidBatchNumber(string? batchNumber);

    /// <summary>
    /// Generates a compliant batch number in standard format.
    /// Format: BATCH-YYYY-MM-XXX
    /// </summary>
    /// <param name="sequenceNumber">Sequential number for this month</param>
    /// <returns>Generated batch number</returns>
    string GenerateBatchNumber(int sequenceNumber);

    /// <summary>
    /// Extracts batch date from batch number (if in standard format).
    /// </summary>
    /// <param name="batchNumber">Batch number</param>
    /// <returns>Batch date if parseable, null otherwise</returns>
    DateTime? ExtractBatchDate(string? batchNumber);

    // ============================================================
    // LAB TEST VALIDATION
    // ============================================================

    /// <summary>
    /// Validates that lab test date is not in the future.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <returns>True if valid (not in future), false otherwise</returns>
    bool IsValidLabTestDate(DateTime? labTestDate);

    /// <summary>
    /// Checks if lab test results are still current/valid.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <param name="validityDays">Number of days test results remain valid (default 180)</param>
    /// <returns>True if still valid, false if expired or missing</returns>
    bool IsLabTestCurrent(DateTime? labTestDate, int? validityDays = null);

    /// <summary>
    /// Checks if lab test results are expiring soon.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <param name="alertDays">Days before expiry to trigger alert (default 30)</param>
    /// <returns>True if expiring within alert window</returns>
    bool IsLabTestExpiringSoon(DateTime? labTestDate, int? alertDays = null);

    // ============================================================
    // PRODUCT EXPIRY VALIDATION
    // ============================================================

    /// <summary>
    /// Validates that product expiry date is in the future.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <returns>True if valid (in future), false otherwise</returns>
    bool IsValidExpiryDate(DateTime? expiryDate);

    /// <summary>
    /// Checks if product is expiring soon.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <param name="alertDays">Days before expiry to trigger alert (default 30)</param>
    /// <returns>True if expiring within alert window</returns>
    bool IsProductExpiringSoon(DateTime? expiryDate, int? alertDays = null);

    /// <summary>
    /// Checks if product has already expired.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <returns>True if expired</returns>
    bool IsProductExpired(DateTime? expiryDate);

    // ============================================================
    // COMPLIANCE STATUS
    // ============================================================

    /// <summary>
    /// Checks if a product requires age verification.
    /// All cannabis products (with THC or CBD) require age verification.
    /// </summary>
    /// <param name="thcContent">THC content</param>
    /// <param name="cbdContent">CBD content</param>
    /// <returns>True if age verification required</returns>
    bool RequiresAgeVerification(string? thcContent, string? cbdContent);

    /// <summary>
    /// Validates overall product compliance status.
    /// </summary>
    /// <param name="thcContent">THC content</param>
    /// <param name="cbdContent">CBD content</param>
    /// <param name="batchNumber">Batch number</param>
    /// <param name="labTestDate">Lab test date</param>
    /// <returns>True if all compliance requirements met</returns>
    bool IsProductCompliant(string? thcContent, string? cbdContent, string? batchNumber, DateTime? labTestDate);
}
