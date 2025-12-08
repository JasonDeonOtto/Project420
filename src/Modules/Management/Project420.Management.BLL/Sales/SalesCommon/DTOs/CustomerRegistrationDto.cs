namespace Project420.Management.BLL.Sales.SalesCommon.DTOs;

/// <summary>
/// Data Transfer Object for customer registration.
/// Used to transfer customer data from UI to business logic layer.
/// </summary>
public class CustomerRegistrationDto
{
    // ============================================================
    // PERSONAL INFORMATION (Required)
    // ============================================================

    /// <summary>
    /// Customer's full name (Required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// South African ID number (Required).
    /// Format: YYMMDD-SSSS-C-ZZ
    /// Used for age verification and duplicate detection.
    /// </summary>
    public string IdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth (Auto-extracted from ID number).
    /// Cannabis Act requires 18+ years.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    // ============================================================
    // CONTACT INFORMATION (Required)
    // ============================================================

    /// <summary>
    /// Mobile number (Required).
    /// Format: 0XX XXX XXXX
    /// </summary>
    public string Mobile { get; set; } = string.Empty;

    /// <summary>
    /// Email address (Required).
    /// Used for notifications and marketing (with consent).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    // ============================================================
    // ADDRESS INFORMATION (Optional)
    // ============================================================

    /// <summary>
    /// Physical street address.
    /// </summary>
    public string? PhysicalAddress { get; set; }

    /// <summary>
    /// Postal address (if different from physical).
    /// </summary>
    public string? PostalAddress { get; set; }

    // ============================================================
    // MEDICAL CANNABIS (Optional - Section 21)
    // ============================================================

    /// <summary>
    /// Medical cannabis permit number (Section 21).
    /// Required for medical patients only.
    /// </summary>
    public string? MedicalPermitNumber { get; set; }

    /// <summary>
    /// Medical permit expiry date.
    /// System will send renewal reminders.
    /// </summary>
    public DateTime? MedicalPermitExpiryDate { get; set; }

    /// <summary>
    /// Prescribing doctor's name.
    /// Required for medical patients.
    /// </summary>
    public string? PrescribingDoctor { get; set; }

    // ============================================================
    // ACCOUNT/CREDIT (Optional)
    // ============================================================

    /// <summary>
    /// Credit limit for account customers.
    /// 0 = Cash customer only.
    /// </summary>
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>
    /// Payment terms in days (e.g., 30 for Net 30).
    /// </summary>
    public int PaymentTerms { get; set; } = 0;

    // ============================================================
    // POPIA COMPLIANCE (Required)
    // ============================================================

    /// <summary>
    /// Customer has given consent for data processing (Required).
    /// POPIA compliance - must be true to proceed.
    /// </summary>
    public bool ConsentGiven { get; set; }

    /// <summary>
    /// Consent for marketing communications (Optional).
    /// Separate from data processing consent.
    /// </summary>
    public bool MarketingConsent { get; set; }

    /// <summary>
    /// Purpose of data collection (auto-set by system).
    /// POPIA requires clear purpose specification.
    /// </summary>
    public string ConsentPurpose { get; set; } = "Customer account management, purchase history, and legal compliance";

    // ============================================================
    // INTERNAL NOTES (Optional)
    // ============================================================

    /// <summary>
    /// Internal notes about customer.
    /// Not visible to customer.
    /// </summary>
    public string? Notes { get; set; }
}
