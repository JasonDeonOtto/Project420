namespace Project420.Shared.Core.Compliance.Validators;

/// <summary>
/// Validator for cannabis lab testing dates and Certificate of Analysis (COA).
/// Ensures compliance with SAHPRA testing requirements.
/// </summary>
public static class LabTestDateValidator
{
    /// <summary>
    /// Validates that lab test date is not in the future.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <returns>True if valid (not in future), false otherwise</returns>
    public static bool IsValidLabTestDate(DateTime? labTestDate)
    {
        if (!labTestDate.HasValue)
            return true; // Optional field

        return labTestDate.Value.Date <= DateTime.Today;
    }

    /// <summary>
    /// Checks if lab test results are still current/valid.
    /// SAHPRA: Certificate of Analysis (COA) must be current.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <param name="validityDays">Number of days test results remain valid (default 180 days)</param>
    /// <returns>True if still valid, false if expired or missing</returns>
    public static bool IsLabTestCurrent(DateTime? labTestDate, int validityDays = CannabisComplianceConstants.LAB_TEST_VALIDITY_DAYS)
    {
        if (!labTestDate.HasValue)
            return false;

        var expiryDate = labTestDate.Value.AddDays(validityDays);
        return DateTime.Today <= expiryDate;
    }

    /// <summary>
    /// Gets the number of days remaining until lab test results expire.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <param name="validityDays">Number of days test results remain valid (default 180 days)</param>
    /// <returns>Days remaining (negative if expired), null if no test date</returns>
    public static int? GetDaysUntilExpiry(DateTime? labTestDate, int validityDays = CannabisComplianceConstants.LAB_TEST_VALIDITY_DAYS)
    {
        if (!labTestDate.HasValue)
            return null;

        var expiryDate = labTestDate.Value.AddDays(validityDays);
        return (expiryDate - DateTime.Today).Days;
    }

    /// <summary>
    /// Checks if lab test results are expiring soon.
    /// </summary>
    /// <param name="labTestDate">Lab test date</param>
    /// <param name="alertDays">Number of days before expiry to trigger alert (default 30 days)</param>
    /// <param name="validityDays">Number of days test results remain valid (default 180 days)</param>
    /// <returns>True if expiring within alert window</returns>
    public static bool IsLabTestExpiringSoon(DateTime? labTestDate, int alertDays = CannabisComplianceConstants.EXPIRY_ALERT_DAYS, int validityDays = CannabisComplianceConstants.LAB_TEST_VALIDITY_DAYS)
    {
        var daysRemaining = GetDaysUntilExpiry(labTestDate, validityDays);

        if (!daysRemaining.HasValue)
            return false;

        return daysRemaining.Value > 0 && daysRemaining.Value <= alertDays;
    }

    /// <summary>
    /// Validates that expiry date is in the future.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <returns>True if valid (in future), false otherwise</returns>
    public static bool IsValidExpiryDate(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
            return true; // Optional field

        return expiryDate.Value.Date > DateTime.Today;
    }

    /// <summary>
    /// Checks if product is expiring soon.
    /// Important for edibles and cannabis oils.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <param name="alertDays">Number of days before expiry to trigger alert (default 30 days)</param>
    /// <returns>True if expiring within alert window</returns>
    public static bool IsProductExpiringSoon(DateTime? expiryDate, int alertDays = CannabisComplianceConstants.EXPIRY_ALERT_DAYS)
    {
        if (!expiryDate.HasValue)
            return false;

        var daysUntilExpiry = (expiryDate.Value.Date - DateTime.Today).Days;
        return daysUntilExpiry > 0 && daysUntilExpiry <= alertDays;
    }

    /// <summary>
    /// Checks if product has already expired.
    /// </summary>
    /// <param name="expiryDate">Product expiry date</param>
    /// <returns>True if expired</returns>
    public static bool IsProductExpired(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
            return false;

        return expiryDate.Value.Date < DateTime.Today;
    }
}
