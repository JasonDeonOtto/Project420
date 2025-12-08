namespace Project420.Shared.Core.Compliance;

/// <summary>
/// Cannabis compliance constants for South African regulations.
/// Centralized location for all compliance-related constants used across modules.
/// </summary>
/// <remarks>
/// Regulatory Framework:
/// - Cannabis for Private Purposes Act 2024
/// - POPIA (Protection of Personal Information Act)
/// - SAHPRA (South African Health Products Regulatory Authority)
/// - DALRRD (Department of Agriculture, Land Reform and Rural Development)
/// </remarks>
public static class CannabisComplianceConstants
{
    // ============================================================
    // AGE VERIFICATION (Cannabis for Private Purposes Act 2024)
    // ============================================================

    /// <summary>
    /// Minimum age requirement for cannabis purchase/possession in South Africa.
    /// Cannabis for Private Purposes Act 2024: 18 years minimum.
    /// </summary>
    public const int MIN_AGE_REQUIREMENT = 18;

    // ============================================================
    // AUDIT & DATA RETENTION (POPIA Compliance)
    // ============================================================

    /// <summary>
    /// Audit trail retention period in years (POPIA requirement).
    /// POPIA: 7-year retention for audit logs and data modifications.
    /// </summary>
    public const int AUDIT_RETENTION_YEARS = 7;

    /// <summary>
    /// Tax records retention period in years (SARS requirement).
    /// SARS: 5-year retention for tax-related documents.
    /// </summary>
    public const int TAX_RETENTION_YEARS = 5;

    // ============================================================
    // BATCH & LOT TRACKING (SAHPRA Seed-to-Sale)
    // ============================================================

    /// <summary>
    /// Maximum length for batch/lot numbers.
    /// SAHPRA: Unique identifier required for seed-to-sale traceability.
    /// </summary>
    public const int BATCH_NUMBER_MAX_LENGTH = 50;

    /// <summary>
    /// Suggested batch number format pattern.
    /// Format: BATCH-YYYY-MM-XXX
    /// Example: "BATCH-2024-12-001"
    /// </summary>
    public const string BATCH_NUMBER_SUGGESTED_FORMAT = @"^BATCH-\d{4}-\d{2}-\d{3,}$";

    // ============================================================
    // CANNABINOID CONTENT (THC/CBD Labeling)
    // ============================================================

    /// <summary>
    /// Maximum length for THC percentage/content field.
    /// Format examples: "15%", "150mg", "15-18%"
    /// </summary>
    public const int CANNABINOID_CONTENT_MAX_LENGTH = 20;

    /// <summary>
    /// Regex pattern for validating cannabinoid content format.
    /// Accepts: "15%", "150mg", "15-18%", "0.5%", etc.
    /// </summary>
    public const string CANNABINOID_CONTENT_PATTERN = @"^[\d.]+([-][\d.]+)?(%|mg)$";

    // ============================================================
    // STRAIN & PRODUCT INFORMATION
    // ============================================================

    /// <summary>
    /// Maximum length for cannabis strain names.
    /// Examples: "Blue Dream", "OG Kush", "Charlotte's Web"
    /// </summary>
    public const int STRAIN_NAME_MAX_LENGTH = 100;

    /// <summary>
    /// Maximum length for SKU (Stock Keeping Unit).
    /// Format examples: "CBD-OIL-001", "FLOWER-IND-002"
    /// </summary>
    public const int SKU_MAX_LENGTH = 50;

    /// <summary>
    /// Suggested SKU format pattern.
    /// Uppercase letters, numbers, hyphens, underscores only.
    /// Example: "CBD-OIL-001", "FLOWER-INDICA-002"
    /// </summary>
    public const string SKU_FORMAT_PATTERN = @"^[A-Z0-9\-_]+$";

    // ============================================================
    // LAB TESTING (Certificate of Analysis)
    // ============================================================

    /// <summary>
    /// Maximum age of lab test results in days.
    /// SAHPRA: Certificate of Analysis (COA) must be current.
    /// Suggested: 180 days (6 months) for validity.
    /// </summary>
    public const int LAB_TEST_VALIDITY_DAYS = 180;

    /// <summary>
    /// Number of days before expiry to trigger alerts.
    /// Default: 30 days notice for expiring products.
    /// </summary>
    public const int EXPIRY_ALERT_DAYS = 30;

    // ============================================================
    // ALERT THRESHOLDS
    // ============================================================

    /// <summary>
    /// Days before medical permit expiry to send renewal reminders.
    /// Multiple alerts: 90, 60, 30 days before expiry.
    /// </summary>
    public static readonly int[] PERMIT_EXPIRY_ALERT_DAYS = { 90, 60, 30 };

    // ============================================================
    // REGULATORY AUTHORITY CONTACT
    // ============================================================

    /// <summary>
    /// SAHPRA (South African Health Products Regulatory Authority)
    /// Website: www.sahpra.org.za
    /// Email: enquiries@sahpra.org.za
    /// </summary>
    public const string SAHPRA_WEBSITE = "https://www.sahpra.org.za";
    public const string SAHPRA_EMAIL = "enquiries@sahpra.org.za";

    /// <summary>
    /// DALRRD (Department of Agriculture, Land Reform and Rural Development)
    /// Website: www.dalrrd.gov.za
    /// Email: Hemp.PIA@dalrrd.gov.za
    /// </summary>
    public const string DALRRD_WEBSITE = "https://www.dalrrd.gov.za";
    public const string DALRRD_EMAIL = "Hemp.PIA@dalrrd.gov.za";

    /// <summary>
    /// Information Regulator (POPIA Compliance)
    /// Website: www.inforegulator.org.za
    /// Email: enquiries@inforegulator.org.za
    /// </summary>
    public const string INFO_REGULATOR_WEBSITE = "https://www.inforegulator.org.za";
    public const string INFO_REGULATOR_EMAIL = "enquiries@inforegulator.org.za";

    // ============================================================
    // VALIDATION MESSAGES (Reusable)
    // ============================================================

    public static class ValidationMessages
    {
        public const string AGE_REQUIREMENT = "Customer must be at least 18 years old (Cannabis Act requirement)";
        public const string THC_CBD_REQUIRED = "Cannabis products must specify THC and/or CBD content for compliance";
        public const string BATCH_NUMBER_REQUIRED = "Batch number required for SAHPRA seed-to-sale traceability";
        public const string LAB_TEST_REQUIRED = "Lab test certificate (COA) required for cannabis products";
        public const string POPIA_CONSENT_REQUIRED = "POPIA consent is required to process personal information";
        public const string AUDIT_TRAIL_REQUIRED = "All cannabis transactions must maintain a 7-year audit trail";
    }
}
