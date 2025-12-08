using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Customer account for online ordering
/// POPIA compliant with consent tracking
/// Cannabis Act compliant with age verification
/// </summary>
[Table("customer_accounts")]
public class CustomerAccount : AuditableEntity
{
    /// <summary>
    /// Customer email (unique login)
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password hash (never store plain passwords)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    // ===== Personal Information (POPIA Protected) =====

    /// <summary>
    /// Customer first name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer last name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// South African ID number (unique)
    /// </summary>
    [MaxLength(20)]
    public string? IdNumber { get; set; }

    /// <summary>
    /// Date of birth (must be 18+ for Cannabis Act)
    /// </summary>
    [Required]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // ===== Age Verification (Cannabis Act Requirement) =====

    /// <summary>
    /// Age has been verified (18+)
    /// </summary>
    public bool AgeVerified { get; set; }

    /// <summary>
    /// Timestamp of age verification
    /// </summary>
    public DateTime? AgeVerificationDate { get; set; }

    /// <summary>
    /// Method used for age verification
    /// </summary>
    [MaxLength(50)]
    public PickupVerificationMethod? AgeVerificationMethod { get; set; }

    /// <summary>
    /// ID document has been verified
    /// </summary>
    public bool IdDocumentVerified { get; set; }

    // ===== POPIA Compliance =====

    /// <summary>
    /// Customer consented to POPIA terms (REQUIRED)
    /// </summary>
    [Required]
    public bool ConsentToPOPIA { get; set; }

    /// <summary>
    /// Date of POPIA consent
    /// </summary>
    [Required]
    public DateTime ConsentToPOPIADate { get; set; }

    /// <summary>
    /// Customer consented to marketing communications (OPTIONAL)
    /// </summary>
    public bool ConsentToMarketing { get; set; }

    /// <summary>
    /// Date of marketing consent
    /// </summary>
    public DateTime? ConsentToMarketingDate { get; set; }

    // ===== Account Status =====

    /// <summary>
    /// Account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Account is locked (e.g., too many failed login attempts)
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Email has been verified
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Email verification token
    /// </summary>
    [MaxLength(500)]
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// Email verification timestamp
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }

    // ===== Password Reset =====

    /// <summary>
    /// Password reset token (temporary)
    /// </summary>
    [MaxLength(500)]
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// Password reset token expiration
    /// </summary>
    public DateTime? PasswordResetExpires { get; set; }

    // ===== Audit Fields =====

    /// <summary>
    /// Last successful login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Customer's orders
    /// </summary>
    public virtual ICollection<OnlineOrder> Orders { get; set; } = new List<OnlineOrder>();
}
