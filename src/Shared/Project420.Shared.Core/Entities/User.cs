using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project420.Shared.Core.Entities
{
    /// <summary>
    /// Represents a system user for authentication and basic identity
    /// </summary>
    /// <remarks>
    /// This is the CORE authentication/identity entity.
    /// All modules can reference this for user identity without circular dependencies.
    ///
    /// Design Decision: Split User Concerns
    /// - User (Shared.Core): WHO you are - authentication, login, basic identity
    /// - UserProfile (Management.Models): ADMINISTRATIVE data - HR info, employment, compliance
    ///
    /// This separation ensures:
    /// - Retail.POS can know who the cashier is without referencing Management
    /// - Clean dependency flow: Operational modules → Shared.Core (not → Management)
    /// - Management module extends User with HR/admin features via UserProfile
    ///
    /// POPIA Compliance (CRITICAL - R10 MILLION PENALTY):
    /// - User data is PII (Personally Identifiable Information)
    /// - Email must be protected and encrypted at rest
    /// - Password MUST NEVER be stored in plain text
    /// - Authentication attempts must be logged
    /// - User consent required for data collection
    /// - 3-year retention after employment termination (handle via soft delete)
    ///
    /// Security Requirements:
    /// - Passwords: Use bcrypt, PBKDF2, or Argon2 for hashing
    /// - Failed login tracking: Lock account after 5 failed attempts
    /// - Session management: Track active sessions
    /// - Password strength: Minimum 8 characters, complexity requirements
    ///
    /// Cannabis Compliance:
    /// - Background checks tracked in UserProfile (Management module)
    /// - Cannabis licenses tracked in UserProfile
    /// - Compliance training tracked in UserProfile
    ///
    /// Usage:
    /// <code>
    /// // Retail.POS checking who is logged in
    /// var cashier = await _userRepo.GetByIdAsync(currentUserId);
    /// transaction.ProcessedBy = cashier.FullName;
    ///
    /// // Management module getting full HR details
    /// var userProfile = await _userProfileRepo.GetByUserIdAsync(currentUserId);
    /// Console.WriteLine($"Employee: {userProfile.EmployeeNumber}");
    /// </code>
    /// </remarks>
    public class User : AuditableEntity
    {
        // ============================================================
        // AUTHENTICATION CREDENTIALS
        // ============================================================

        /// <summary>
        /// Unique username for system login
        /// </summary>
        /// <remarks>
        /// Best Practices:
        /// - Must be unique across the system
        /// - Case-insensitive comparison recommended
        /// - Consider using email as username for simplicity
        /// - Format: alphanumeric, underscore, dot allowed
        /// - Cannot be changed after creation (or require admin approval)
        ///
        /// Examples: "john.doe", "jdoe", "cashier01"
        /// </remarks>
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's email address (authentication + notifications)
        /// </summary>
        /// <remarks>
        /// POPIA: This is PII - must be encrypted at rest in production
        ///
        /// Uses:
        /// - Primary authentication method
        /// - Password reset
        /// - System notifications
        /// - Two-factor authentication
        ///
        /// Must be unique per user
        /// </remarks>
        [Required(ErrorMessage = "Email address is required")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password (NEVER store plain text passwords)
        /// </summary>
        /// <remarks>
        /// CRITICAL SECURITY REQUIREMENT:
        /// - NEVER store plain text passwords
        /// - Use bcrypt, PBKDF2, or Argon2 for hashing
        /// - Each password must be individually salted
        ///
        /// Password Policy:
        /// - Minimum length: 8 characters (recommend 12+)
        /// - Complexity: Require 3 of 4 (upper, lower, digit, special)
        /// - Cannot reuse last 5 passwords
        /// - No common passwords (use blacklist check)
        ///
        /// Example hash (bcrypt): "$2a$12$KIXxkCGYt0..."
        /// </remarks>
        [Required(ErrorMessage = "Password hash is required")]
        [MaxLength(500)]
        [Display(Name = "Password Hash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Security salt used for password hashing
        /// </summary>
        /// <remarks>
        /// Each user must have a unique salt
        /// Generated automatically during password creation
        /// Never exposed to user
        /// Stored separately from hash for additional security
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Password Salt")]
        public string? PasswordSalt { get; set; }

        // ============================================================
        // BASIC IDENTITY (Minimal PII)
        // ============================================================

        /// <summary>
        /// User's first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name (surname)
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Full name (computed from FirstName + LastName)
        /// </summary>
        /// <remarks>
        /// Not stored in database - computed property
        /// Use for display throughout the system
        /// </remarks>
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        // ============================================================
        // AUTHORIZATION (Simple RBAC for Phase 2)
        // ============================================================

        /// <summary>
        /// User's role in the system (for Role-Based Access Control)
        /// </summary>
        /// <remarks>
        /// Phase 2 Roles (Hardcoded in PermissionService):
        /// - "SuperAdmin" - Full system access, all modules, all permissions
        /// - "Admin" - Administrative functions, most permissions
        /// - "Manager" - Store/department management, supervisory permissions
        /// - "Supervisor" - Team supervision, limited admin
        /// - "Cashier" - POS operations only
        /// - "Inventory" - Stock management
        /// - "ReadOnly" - View-only access (reports, audits)
        ///
        /// Phase 4 Future: Consider separate Roles table with many-to-many
        ///
        /// Permission checking:
        /// <code>
        /// if (await _permissionService.UserCanAsync(user.Id, SystemPermission.RetailPOS_Process_Refund))
        /// {
        ///     // Allow refund
        /// }
        /// </code>
        /// </remarks>
        [Required(ErrorMessage = "User role is required")]
        [MaxLength(50)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Cashier";

        // ============================================================
        // ACCOUNT STATUS & SECURITY
        // ============================================================

        /// <summary>
        /// Whether this user account is active and can log in
        /// </summary>
        /// <remarks>
        /// Set to false to:
        /// - Temporarily suspend account
        /// - Disable account after termination
        /// - Lock account after security incident
        ///
        /// Inactive users cannot log in but records are preserved (POPIA compliance)
        /// </remarks>
        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether account is locked due to security policy
        /// </summary>
        /// <remarks>
        /// Account locked when:
        /// - Too many failed login attempts (5+)
        /// - Security breach detected
        /// - Manual admin lock
        ///
        /// Unlocking:
        /// - Admin can manually unlock
        /// - Auto-unlock after 30 minutes (configurable)
        /// - Password reset unlocks account
        /// </remarks>
        [Required]
        [Display(Name = "Is Locked")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Number of consecutive failed login attempts
        /// </summary>
        /// <remarks>
        /// Security policy:
        /// - Lock account after 5 failed attempts
        /// - Reset to 0 on successful login
        /// - Admin can manually reset
        /// - Consider CAPTCHA after 3 attempts
        /// </remarks>
        [Required]
        [Display(Name = "Failed Login Attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// When account was locked (if IsLocked = true)
        /// </summary>
        [Display(Name = "Account Locked At")]
        public DateTime? AccountLockedAt { get; set; }

        /// <summary>
        /// Date and time of last successful login
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Security monitoring
        /// - Inactive account detection
        /// - Audit trails
        /// - User activity reports
        /// </remarks>
        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// IP address of last login
        /// </summary>
        /// <remarks>
        /// Security monitoring:
        /// - Detect login from unusual locations
        /// - Track access patterns
        /// - Investigate security incidents
        ///
        /// Format: IPv4 (192.168.1.1) or IPv6
        /// Max length: 45 characters (IPv6 full notation)
        /// </remarks>
        [MaxLength(45)]
        [Display(Name = "Last Login IP")]
        public string? LastLoginIpAddress { get; set; }

        // ============================================================
        // PASSWORD MANAGEMENT
        // ============================================================

        /// <summary>
        /// Date when password was last changed
        /// </summary>
        /// <remarks>
        /// Track password age
        /// Used to enforce password history (no reuse of last 5)
        /// Calculate password expiry
        /// </remarks>
        [Display(Name = "Password Last Changed")]
        public DateTime? PasswordLastChangedAt { get; set; }

        /// <summary>
        /// Date when password must be changed
        /// </summary>
        /// <remarks>
        /// Password expiry policy:
        /// - SuperAdmin/Admin: 90 days
        /// - Manager: 90 days
        /// - Regular users: 180 days (or never for POC)
        /// - Force change on first login (set to past date)
        ///
        /// System warns user 7 days before expiry
        /// </remarks>
        [Display(Name = "Password Expires At")]
        public DateTime? PasswordExpiresAt { get; set; }

        /// <summary>
        /// Token for password reset (temporary, single-use)
        /// </summary>
        /// <remarks>
        /// Generated when user requests password reset
        /// Sent via email link
        /// Expires after 1 hour (see PasswordResetExpiresAt)
        /// Single use only - invalidate after successful reset
        /// Cryptographically secure random token
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Password Reset Token")]
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// When password reset token expires
        /// </summary>
        /// <remarks>
        /// Typically 1 hour from generation
        /// Token invalid after this time
        /// Check before allowing password reset
        /// </remarks>
        [Display(Name = "Password Reset Expires")]
        public DateTime? PasswordResetExpiresAt { get; set; }

        // ============================================================
        // TWO-FACTOR AUTHENTICATION (2FA) - Optional for Phase 2
        // ============================================================

        /// <summary>
        /// Whether two-factor authentication is enabled
        /// </summary>
        /// <remarks>
        /// Recommended for:
        /// - SuperAdmin roles (mandatory)
        /// - Admin roles (recommended)
        /// - Finance/accounting roles (mandatory)
        ///
        /// Methods: SMS, Email, Authenticator App (Google/Microsoft)
        ///
        /// Phase 2: Optional (build foundation)
        /// Phase 3+: Implement fully
        /// </remarks>
        [Required]
        [Display(Name = "Two-Factor Authentication Enabled")]
        public bool TwoFactorEnabled { get; set; } = false;

        /// <summary>
        /// Secret key for TOTP (Time-based One-Time Password) authentication
        /// </summary>
        /// <remarks>
        /// Used with Google Authenticator, Microsoft Authenticator, etc.
        /// Must be kept secret - encrypted at rest
        /// Generated during 2FA setup
        /// Base32-encoded string
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Two-Factor Secret")]
        public string? TwoFactorSecret { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (Future Relationships)
        // ============================================================

        /// <summary>
        /// User's granular permissions (future - Phase 4)
        /// </summary>
        /// <remarks>
        /// Many-to-many relationship through UserPermission junction table
        /// Phase 2: Not used (permissions hardcoded in PermissionService based on Role)
        /// Phase 4: Store user-specific permission grants/denies
        /// </remarks>
        public virtual ICollection<UserPermission>? UserPermissions { get; set; }

        // Note: Additional navigation properties can be added by other modules:
        // - TransactionHeader.ProcessedBy → User (who processed the sale)
        // - AuditLog.User → User (who performed the action)
        // - UserProfile → User (HR/admin data in Management module)
    }
}
