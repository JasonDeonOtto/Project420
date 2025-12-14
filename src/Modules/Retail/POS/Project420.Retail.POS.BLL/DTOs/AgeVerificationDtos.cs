namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Result of SA ID number validation
/// </summary>
/// <remarks>
/// Phase 9.7: Age Verification Enhancement
///
/// SA ID Number Format: YYMMDDGSSSCAZ (13 digits)
/// Contains extracted information from a valid SA ID
/// </remarks>
public class SAIdValidationResultDto
{
    /// <summary>
    /// Whether the ID number is valid (format + check digit)
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The ID number that was validated
    /// </summary>
    public string IdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Extracted date of birth (if valid)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Calculated age in years (if valid)
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Whether the person is 18 or older
    /// </summary>
    public bool IsOver18 { get; set; }

    /// <summary>
    /// Extracted gender (Male/Female)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Citizenship status (SA Citizen/Permanent Resident)
    /// </summary>
    public string? Citizenship { get; set; }

    /// <summary>
    /// Error messages if validation failed
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Warnings (e.g., ID holder is exactly 18)
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Result of age verification (from ID or manual DOB)
/// </summary>
public class AgeVerificationResultDto
{
    /// <summary>
    /// Whether age verification was successful
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Whether the person is 18 or older
    /// </summary>
    public bool IsOver18 { get; set; }

    /// <summary>
    /// Calculated age in years
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Date of birth used for verification
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Method of verification (ID Scan, Manual DOB, Manager Override)
    /// </summary>
    public AgeVerificationMethod Method { get; set; }

    /// <summary>
    /// Timestamp of verification
    /// </summary>
    public DateTime VerificationDate { get; set; }

    /// <summary>
    /// User ID who performed the verification
    /// </summary>
    public int VerifiedByUserId { get; set; }

    /// <summary>
    /// Manager ID if override was used
    /// </summary>
    public int? ManagerOverrideUserId { get; set; }

    /// <summary>
    /// Error message if verification failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// ID number (masked for privacy, e.g., "8501***XXXX**")
    /// </summary>
    public string? MaskedIdNumber { get; set; }
}

/// <summary>
/// Request for age verification
/// </summary>
public class AgeVerificationRequestDto
{
    /// <summary>
    /// SA ID number (13 digits) - for ID scan method
    /// </summary>
    public string? IdNumber { get; set; }

    /// <summary>
    /// Date of birth - for manual entry method
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Verification method being used
    /// </summary>
    public AgeVerificationMethod Method { get; set; }

    /// <summary>
    /// User ID performing the verification
    /// </summary>
    public int VerifiedByUserId { get; set; }

    /// <summary>
    /// Manager ID (required for override method)
    /// </summary>
    public int? ManagerUserId { get; set; }

    /// <summary>
    /// Manager PIN (required for override method)
    /// </summary>
    public string? ManagerPin { get; set; }

    /// <summary>
    /// Override reason (required for manager override)
    /// </summary>
    public string? OverrideReason { get; set; }
}

/// <summary>
/// Methods of age verification for Cannabis Act compliance
/// </summary>
public enum AgeVerificationMethod
{
    /// <summary>
    /// SA ID card barcode/number scanned
    /// </summary>
    IdScan = 1,

    /// <summary>
    /// Date of birth entered manually (from ID/passport)
    /// </summary>
    ManualDob = 2,

    /// <summary>
    /// Manager override (for special circumstances)
    /// </summary>
    ManagerOverride = 3,

    /// <summary>
    /// Returning customer with verified age on file
    /// </summary>
    ReturningCustomer = 4
}
