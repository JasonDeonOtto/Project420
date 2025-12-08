using System.ComponentModel.DataAnnotations;

namespace Project420.Shared.Core.Entities
{
    /// <summary>
    /// Represents which stations (PCs/terminals) can access which company databases.
    /// Multi-tenant routing metadata stored in Shared database.
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Maps hostnames/hardware IDs to authorized company databases
    /// - Stores encrypted connection strings per company
    /// - Enables multi-tenant architecture with data isolation
    ///
    /// Usage Flow:
    /// 1. App starts on station "POS-STORE-01"
    /// 2. Query StationConnections: WHERE HostName = 'POS-STORE-01'
    /// 3. Returns list of companies this station can access
    /// 4. User selects company from dropdown
    /// 5. App uses EncryptedConnectionString to connect to that company's database
    /// 6. User authenticates against selected company's Users table
    ///
    /// Multi-Tenant Examples:
    /// - POS-STORE-01 → Can access "Main Store" and "Branch A"
    /// - POS-STORE-02 → Can only access "Branch B"
    /// - MOBILE-REP-01 → Can access all companies (regional manager)
    ///
    /// Security:
    /// - Connection strings encrypted at rest (AES-256)
    /// - Decrypted only when establishing database connection
    /// - Different SQL credentials per company for additional isolation
    /// - IsAuthorized flag for quick enable/disable access
    ///
    /// POPIA Compliance:
    /// - Audit trail via AuditableEntity (who authorized this station)
    /// - Soft delete (preserve authorization history)
    /// - 7-year retention for compliance audits
    ///
    /// Architecture Decision:
    /// - Phase 1: Use Machine Name (Environment.MachineName)
    /// - Phase 4: Upgrade to Hardware ID licensing system
    /// - HostName field accepts any identifier (future-proof)
    /// </remarks>
    public class StationConnection : AuditableEntity
    {
        // ============================================================
        // STATION IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Hostname or unique identifier of the connecting station/PC
        /// </summary>
        /// <remarks>
        /// Identification Methods:
        /// - Phase 1: Machine Name (Environment.MachineName) e.g., "POS-STORE-01"
        /// - Phase 4: Hardware ID (license key) e.g., "HW-ABC123-XYZ789"
        ///
        /// Case Sensitivity:
        /// - Store uppercase for consistency (queries use .ToUpper())
        /// - Handle variations (e.g., "pos-store-01" vs "POS-STORE-01")
        ///
        /// Examples:
        /// - "POS-STORE-01" (physical desktop terminal)
        /// - "MOBILE-REP-05" (tablet/laptop)
        /// - "BACKOFFICE-ADMIN" (admin workstation)
        /// - "HW-ABC123-XYZ789" (future hardware ID)
        /// </remarks>
        [Required(ErrorMessage = "Station hostname is required")]
        [MaxLength(100, ErrorMessage = "Hostname cannot exceed 100 characters")]
        [Display(Name = "Station Hostname")]
        public string HostName { get; set; } = string.Empty;

        // ============================================================
        // COMPANY IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Friendly company name displayed to user during company selection
        /// </summary>
        /// <remarks>
        /// User Interface:
        /// - Shown in company selection dropdown
        /// - Should be human-readable and recognizable
        ///
        /// Examples:
        /// - "Project420 - Main Store"
        /// - "Project420 - Branch A (Sandton)"
        /// - "Project420 - Franchise #5"
        /// - "ABC Cannabis - Johannesburg"
        ///
        /// Not unique: Multiple stations can access same company
        /// </remarks>
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Physical database name for this company
        /// </summary>
        /// <remarks>
        /// Database Naming Convention:
        /// - Project420_Dev (first company / development)
        /// - Project420_Dev2 (second company)
        /// - Project420_CompanyA (named by company)
        /// - Project420_Franchise_001 (franchise model)
        ///
        /// Used to:
        /// - Identify which database to connect to
        /// - Build connection string dynamically
        /// - Track which company's data is being accessed
        ///
        /// Same Schema:
        /// - All company databases have identical schema
        /// - Migrations applied to all company databases
        /// - Only data differs per company
        /// </remarks>
        [Required(ErrorMessage = "Database name is required")]
        [MaxLength(100, ErrorMessage = "Database name cannot exceed 100 characters")]
        [Display(Name = "Database Name")]
        public string DatabaseName { get; set; } = string.Empty;

        // ============================================================
        // CONNECTION CONFIGURATION
        // ============================================================

        /// <summary>
        /// Encrypted connection string for this company's database
        /// </summary>
        /// <remarks>
        /// Security (CRITICAL):
        /// - NEVER store plain text connection strings
        /// - Encrypt before storing in database
        /// - Decrypt only when establishing connection
        /// - Different SQL credentials per company recommended
        ///
        /// Encryption Method (Architecture Decision):
        /// - AES-256 encryption (cross-platform support)
        /// - Key in appsettings (dev) or Azure Key Vault (production)
        /// - Implemented in Shared.Infrastructure encryption service
        ///
        /// Example Plain Text (NEVER STORE THIS):
        /// "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=company_a_user;Password=SecurePass123;TrustServerCertificate=True;"
        ///
        /// Example Encrypted (stored in this field):
        /// Base64-encoded AES-256 encrypted string
        ///
        /// Connection String Components:
        /// - Server: Same or different SQL Server per company
        /// - Database: DatabaseName field value
        /// - Credentials: Company-specific SQL login (not sa!)
        /// - Additional: Connection pooling, timeout, encryption settings
        ///
        /// Decryption happens in:
        /// - AuthenticationService.LoginAsync()
        /// - CompanySelectionService.GetConnectionString()
        /// </remarks>
        [Required(ErrorMessage = "Connection string is required")]
        [MaxLength(1000, ErrorMessage = "Encrypted connection string too long")]
        [Display(Name = "Encrypted Connection String")]
        public string EncryptedConnectionString { get; set; } = string.Empty;

        // ============================================================
        // AUTHORIZATION & ACCESS CONTROL
        // ============================================================

        /// <summary>
        /// Whether this station is currently authorized to access this company
        /// </summary>
        /// <remarks>
        /// Use Cases:
        /// - Enable/disable station access without deleting record
        /// - Temporary suspension (e.g., during investigation)
        /// - Emergency lockout (security incident)
        /// - Deactivate old terminals
        ///
        /// Different from IsDeleted:
        /// - IsActive: Temporary authorization flag (easily reversed)
        /// - IsDeleted: Soft delete (permanent removal)
        ///
        /// Security:
        /// - Set to false = station immediately loses access
        /// - User sees "Access denied" at company selection
        /// - Logged in users kicked out on next auth check
        ///
        /// Audit:
        /// - Changes tracked via ModifiedAt/ModifiedBy
        /// - Alert on authorization changes
        /// </remarks>
        [Required]
        [Display(Name = "Is Authorized")]
        public bool IsAuthorized { get; set; } = true;

        /// <summary>
        /// Optional: Date when station access to this company expires
        /// </summary>
        /// <remarks>
        /// Temporary Access Scenarios:
        /// - Mobile rep visiting location for 1 week
        /// - Temporary staff terminal (seasonal workers)
        /// - Trial period for new franchise
        /// - Contractor access (external auditors)
        ///
        /// Behavior:
        /// - If null: Access never expires (permanent)
        /// - If past date: Access expired (treat as unauthorized)
        /// - If future date: Access valid until that date
        ///
        /// System Monitoring:
        /// - Background job checks for expiring access (alert at 7/3/1 days before)
        /// - Automatic deactivation on expiry (set IsAuthorized = false)
        /// - Email notifications to admins
        ///
        /// Examples:
        /// - 2025-12-31 23:59:59 (expires end of year)
        /// - 2025-12-15 (temporary holiday staff)
        /// </remarks>
        [Display(Name = "Access Expires At")]
        public DateTime? AccessExpiresAt { get; set; }

        // ============================================================
        // METADATA & NOTES
        // ============================================================

        /// <summary>
        /// Optional: Administrative notes about this station connection
        /// </summary>
        /// <remarks>
        /// Documentation:
        /// - Why does this station have access to this company?
        /// - Who approved the access?
        /// - Temporary or permanent?
        /// - Special circumstances?
        ///
        /// Examples:
        /// - "Main POS terminal for Store 01"
        /// - "Regional manager mobile device - access to all franchises"
        /// - "Temporary access for year-end stocktake (approved by CEO)"
        /// - "Backup terminal - only used when POS-STORE-01 is down"
        ///
        /// Audit Value:
        /// - Helps during security reviews
        /// - Explains unusual access patterns
        /// - Documents business justification
        /// </remarks>
        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Authorization Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // COMPUTED PROPERTIES
        // ============================================================

        /// <summary>
        /// Computed: Is this station connection currently valid?
        /// </summary>
        /// <remarks>
        /// Combines multiple authorization checks:
        /// - IsAuthorized must be true
        /// - IsDeleted must be false (inherited from AuditableEntity)
        /// - AccessExpiresAt must be null or future date
        ///
        /// Usage:
        /// var validConnections = await _context.StationConnections
        ///     .Where(sc => sc.HostName == hostname && sc.IsValidConnection)
        ///     .ToListAsync();
        /// </remarks>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [Display(Name = "Is Valid Connection")]
        public bool IsValidConnection =>
            IsAuthorized &&
            !IsDeleted &&
            (AccessExpiresAt == null || AccessExpiresAt > DateTime.UtcNow);
    }
}
