using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities.Sales.Common
{
    /// <summary>
    /// Represents a customer/debtor in the POS system
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance Requirements (SA Cannabis for Private Purposes Act 2024):
    /// - Age verification REQUIRED (18+ minimum)
    /// - Medical license tracking for Section 21 permit holders
    /// - Purchase limits may need to be enforced (future regulation)
    ///
    /// POPIA Compliance (CRITICAL - R10 MILLION PENALTY):
    /// - This entity contains PII (Personally Identifiable Information)
    /// - Email, phone, address, ID number are ALL protected under POPIA
    /// - Must have customer consent to store this information
    /// - Must implement encryption at rest and in transit
    /// - Cannot permanently delete (soft delete only)
    /// - 7-year retention requirement for tax purposes
    /// - Customer has right to access, correct, and request deletion (with exceptions)
    ///
    /// Note: In production, implement encryption for:
    /// - Email, Phone, PhysicalAddress, IdNumber (use EncryptedString type or similar)
    /// </remarks>
    public class Debtor : AuditableEntity
    {
        // ============================================================
        // BASIC INFORMATION
        // ============================================================

        /// <summary>
        /// Customer account number (unique identifier for business use)
        /// </summary>
        /// <remarks>
        /// Best Practice: Generate unique account numbers
        /// Example formats: "CUST-0001", "DBT-00001", or auto-incrementing
        /// Different from database Id - this is user-facing
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Customer's full name or business name
        /// </summary>
        [Required(ErrorMessage = "Customer name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Customer type: Individual or Business
        /// </summary>
        /// <remarks>
        /// Individual = Private person
        /// Business = Company/Organization (may have different rules/limits)
        /// </remarks>
        [Required]
        [MaxLength(20)]
        [Display(Name = "Customer Type")]
        public int CategoryID { get; set; } // or "Business" Check here for linking to debtorCategory since we wish to link this to that.
        public virtual ICollection<DebtorCategory> DebtorCategories { get; set; }
        /// <summary>
        /// Whether this customer account is currently active
        /// </summary>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // CONTACT INFORMATION (PII - POPIA Protected)
        // ============================================================

        /// <summary>
        /// Customer email address
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        /// Requires customer consent to store and use
        /// Used for: receipts, marketing (with consent), account recovery
        /// </remarks>
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// Customer primary phone number
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        /// Format: South African numbers typically +27 XX XXX XXXX
        /// </remarks>
        [MaxLength(20)]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Customer mobile number (alias for PhoneNumber for compatibility)
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        /// Format: 0XX XXX XXXX (10 digits)
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "Mobile Number")]
        public string? Mobile { get; set; }

        /// <summary>
        /// Customer physical address
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        /// May be required for:
        /// - Delivery services (when legal)
        /// - Credit applications
        /// - Compliance verification
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Physical Address")]
        public string? PhysicalAddress { get; set; }

        /// <summary>
        /// Customer postal address (if different from physical)
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        /// Used for sending statements, invoices, etc.
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Postal Address")]
        public string? PostalAddress { get; set; }

        // ============================================================
        // AGE VERIFICATION (Cannabis Compliance - CRITICAL)
        // ============================================================

        /// <summary>
        /// Customer's date of birth (for age verification)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: REQUIRED - Must verify 18+ years old
        /// SA Cannabis for Private Purposes Act 2024: Minimum age is 18
        ///
        /// POPIA: Date of birth is PII but necessary for legal compliance
        /// Age verification is a lawful purpose under POPIA
        ///
        /// Best Practice: Calculate age at point of sale, don't store age itself
        /// (age changes, date of birth doesn't)
        /// </remarks>
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// South African ID number or passport number
        /// </summary>
        /// <remarks>
        /// POPIA: This is SENSITIVE PII - MUST be encrypted in production
        /// Only collect if absolutely necessary for:
        /// - Credit applications
        /// - Regulatory compliance
        /// - Age verification (if date of birth not available)
        ///
        /// SA ID Number format: YYMMDD SSSS C AZ
        /// Can derive date of birth and gender from ID number
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "ID / Passport Number")]
        public string? IdNumber { get; set; }

        /// <summary>
        /// Whether age has been verified (18+)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Must verify before ANY cannabis sale
        /// Can be verified via:
        /// - Date of birth calculation
        /// - ID document inspection
        /// - ID scanner
        ///
        /// Best Practice: Re-verify periodically (every 6-12 months)
        /// </remarks>
        [Required]
        [Display(Name = "Age Verified")]
        public bool AgeVerified { get; set; } = false;

        /// <summary>
        /// Date when age verification was last performed
        /// </summary>
        [Display(Name = "Age Verification Date")]
        public DateTime? AgeVerificationDate { get; set; }

        // ============================================================
        // MEDICAL CANNABIS (Section 21 Permit Holders)
        // ============================================================

        /// <summary>
        /// Medical cannabis license number (Section 21 permit from SAHPRA)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Section 21 of Medicines Act
        /// Allows access to medical cannabis with doctor prescription
        /// Different rules/limits may apply vs recreational customers
        ///
        /// SAHPRA issues these permits for specific conditions:
        /// - Chronic pain, epilepsy, cancer treatment side effects, etc.
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Medical License Number")]
        public string? MedicalLicenseNumber { get; set; }

        /// <summary>
        /// Expiry date of medical cannabis license
        /// </summary>
        /// <remarks>
        /// Must verify license is current before selling medical cannabis
        /// Implement alerts for expiring licenses (90, 60, 30 days)
        /// </remarks>
        [Display(Name = "License Expiry Date")]
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// Whether customer has a valid medical cannabis license
        /// </summary>
        [Display(Name = "Has Medical License")]
        public bool HasMedicalLicense { get; set; } = false;

        /// <summary>
        /// Medical permit number (alias for MedicalLicenseNumber for compatibility)
        /// Section 21 permit from SAHPRA
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Medical Permit Number")]
        public string? MedicalPermitNumber { get; set; }

        /// <summary>
        /// Medical permit expiry date (alias for LicenseExpiryDate for compatibility)
        /// </summary>
        [Display(Name = "Medical Permit Expiry Date")]
        public DateTime? MedicalPermitExpiryDate { get; set; }

        /// <summary>
        /// Prescribing doctor's name for medical cannabis patients
        /// </summary>
        /// <remarks>
        /// Required for Section 21 permit holders
        /// Used for compliance verification
        /// </remarks>
        [MaxLength(200)]
        [Display(Name = "Prescribing Doctor")]
        public string? PrescribingDoctor { get; set; }

        // ============================================================
        // POPIA COMPLIANCE (Data Protection)
        // ============================================================

        /// <summary>
        /// Whether customer has given consent for data processing
        /// </summary>
        /// <remarks>
        /// POPIA CRITICAL: REQUIRED before processing any personal information
        /// Must be freely given, specific, informed, and unambiguous
        /// Customer must understand:
        /// - What data is collected
        /// - Why it's collected
        /// - How it will be used
        /// - How long it will be retained
        /// </remarks>
        [Required]
        [Display(Name = "Consent Given")]
        public bool ConsentGiven { get; set; } = false;

        /// <summary>
        /// Date and time when consent was given
        /// </summary>
        /// <remarks>
        /// POPIA requires proof of when consent was obtained
        /// Must be stored for audit purposes
        /// </remarks>
        [Display(Name = "Consent Date")]
        public DateTime? ConsentDate { get; set; }

        /// <summary>
        /// Purpose for which consent was given
        /// </summary>
        /// <remarks>
        /// POPIA requires clear purpose specification
        /// Example: "Customer account management, purchase history, and legal compliance"
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Consent Purpose")]
        public string? ConsentPurpose { get; set; }

        /// <summary>
        /// Whether customer consents to marketing communications
        /// </summary>
        /// <remarks>
        /// POPIA: Marketing consent is SEPARATE from data processing consent
        /// Must be opt-in (not pre-checked)
        /// Customer can withdraw at any time
        /// </remarks>
        [Display(Name = "Marketing Consent")]
        public bool MarketingConsent { get; set; } = false;

        // ============================================================
        // CREDIT MANAGEMENT (Business Logic)
        // ============================================================

        /// <summary>
        /// Maximum credit limit allowed for this customer
        /// </summary>
        /// <remarks>
        /// Set based on:
        /// - Credit application and checks
        /// - Payment history
        /// - Business relationship
        ///
        /// 0 = Cash only (no credit)
        /// </remarks>
        [Required]
        [Range(0, 999999.99, ErrorMessage = "Credit limit must be between R0 and R999,999.99")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Credit Limit")]
        public decimal CreditLimit { get; set; } = 0.00m;

        /// <summary>
        /// Current outstanding balance (what customer owes)
        /// </summary>
        /// <remarks>
        /// Calculated from:
        /// - Sales on account (increases balance)
        /// - Payments received (decreases balance)
        /// - Refunds (decreases balance)
        ///
        /// Negative balance = customer has credit/overpaid
        /// Positive balance = customer owes money
        ///
        /// Best Practice: Prevent new sales if:
        /// CurrentBalance + NewSale > CreditLimit
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; } = 0.00m;

        /// <summary>
        /// Number of days allowed for payment (payment terms)
        /// </summary>
        /// <remarks>
        /// Common values:
        /// - 0 = Cash on delivery / Immediate payment
        /// - 7 = 7 days payment terms
        /// - 30 = 30 days payment terms
        /// - 60 = 60 days payment terms
        /// </remarks>
        [Required]
        [Range(0, 365)]
        [Display(Name = "Payment Terms (Days)")]
        public int PaymentTermsDays { get; set; } = 0;

        /// <summary>
        /// Payment terms (alias for PaymentTermsDays for compatibility)
        /// </summary>
        [Range(0, 365)]
        [Display(Name = "Payment Terms")]
        public int PaymentTerms { get; set; } = 0;

        // ============================================================
        // NOTES AND ADDITIONAL INFO
        // ============================================================

        /// <summary>
        /// Internal notes about this customer
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Special instructions
        /// - Preferences
        /// - Credit notes
        /// - Customer service history
        ///
        /// Warning: Be careful what you write here (POPIA implications)
        /// Don't record sensitive personal opinions or discriminatory information
        /// </remarks>
        [MaxLength(2000)]
        [Display(Name = "Internal Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

    }
}

