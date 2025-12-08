using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities
{
    /// <summary>
    /// Junction table linking Users to specific Permissions (many-to-many)
    /// </summary>
    /// <remarks>
    /// Phase Implementation Strategy:
    ///
    /// PHASE 2 (POC - Current): NOT USED
    /// - Permissions are hardcoded in PermissionService based on User.Role
    /// - This entity exists but table is not populated
    /// - Simple role-based logic:
    ///   - "SuperAdmin" → All permissions
    ///   - "Manager" → Module-specific management permissions
    ///   - "Cashier" → Basic POS operations only
    ///
    /// PHASE 4 (Future): FULLY IMPLEMENTED
    /// - Store user-specific permission grants and denies
    /// - Allows customization beyond role defaults
    /// - Admin UI for permission assignment
    /// - Permission inheritance: Role grants base permissions, then customize per user
    ///
    /// Design Pattern: Grant/Deny Model
    /// - IsGranted = true: Explicitly grant permission (even if role doesn't have it)
    /// - IsGranted = false: Explicitly deny permission (even if role has it)
    /// - No record: Use role defaults
    ///
    /// Example Use Cases:
    /// 1. Grant Exception: Cashier needs refund permission (normally Manager only)
    ///    - Create record: UserId=5, Permission=RetailPOS_Process_Refund, IsGranted=true
    ///
    /// 2. Deny Exception: Manager should NOT have user management access
    ///    - Create record: UserId=10, Permission=Management_Edit_User, IsGranted=false
    ///
    /// Permission Check Logic (Phase 4):
    /// <code>
    /// // 1. Check explicit user permission (grant or deny)
    /// var userPerm = await _db.UserPermissions
    ///     .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission == permission);
    /// if (userPerm != null)
    ///     return userPerm.IsGranted; // Explicit override
    ///
    /// // 2. Fall back to role default
    /// return _rolePermissions[user.Role].Contains(permission);
    /// </code>
    ///
    /// POPIA Compliance:
    /// - Permission changes must be logged (audit trail via AuditableEntity)
    /// - Track who granted/revoked permissions (CreatedBy, ModifiedBy)
    /// - Retention: Keep permission history (soft delete only)
    ///
    /// Cannabis Compliance:
    /// - Sensitive permissions (Plantation, Production) require additional checks:
    ///   - Background check clearance
    ///   - Cannabis license verification
    ///   - Compliance training completion
    /// - These checks happen in PermissionService, not in this entity
    /// </remarks>
    public class UserPermission : AuditableEntity
    {
        // ============================================================
        // RELATIONSHIP KEYS
        // ============================================================

        /// <summary>
        /// Foreign key to User entity
        /// </summary>
        [Required]
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        /// <summary>
        /// The specific permission being granted or denied
        /// </summary>
        /// <remarks>
        /// References SystemPermission enum
        /// Examples:
        /// - SystemPermission.RetailPOS_Create_Sale
        /// - SystemPermission.Management_Edit_User
        /// - SystemPermission.RetailPOS_Process_Refund
        ///
        /// Stored as integer in database (enum underlying type)
        /// </remarks>
        [Required]
        [Display(Name = "Permission")]
        public SystemPermission Permission { get; set; }

        // ============================================================
        // PERMISSION GRANT/DENY
        // ============================================================

        /// <summary>
        /// Whether this permission is granted (true) or denied (false)
        /// </summary>
        /// <remarks>
        /// Grant/Deny Model:
        /// - true: User HAS this permission (even if role doesn't normally grant it)
        /// - false: User DOES NOT have this permission (even if role normally grants it)
        ///
        /// Use Cases:
        /// - Grant: Exception permissions for specific users
        ///   Example: Trusted cashier can process refunds without manager
        ///
        /// - Deny: Revoke specific permissions
        ///   Example: Manager cannot manage users (HR restriction)
        ///
        /// If no record exists: Fall back to role-based defaults
        /// </remarks>
        [Required]
        [Display(Name = "Is Granted")]
        public bool IsGranted { get; set; } = true;

        // ============================================================
        // PERMISSION METADATA (Optional - for audit/admin UI)
        // ============================================================

        /// <summary>
        /// Reason why this permission was granted or denied
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Audit trail documentation
        /// - Admin UI display
        /// - Compliance documentation
        ///
        /// Examples:
        /// - "Temporary access for Black Friday weekend"
        /// - "HR restriction - no user management access"
        /// - "Approved by Store Manager on 2025-01-15"
        /// - "Revoked due to security incident"
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Date when this permission grant/deny expires (optional)
        /// </summary>
        /// <remarks>
        /// Useful for temporary permissions:
        /// - Weekend coverage: "Grant manager permissions for Sat-Sun only"
        /// - Vacation replacement: "Grant admin access while manager on leave"
        /// - Trial period: "Grant access for 30 days, then review"
        ///
        /// If null: Permission is permanent (until manually revoked)
        /// If past: Permission has expired, treat as revoked
        ///
        /// System should:
        /// - Check expiry during permission evaluation
        /// - Alert admins when permissions are about to expire
        /// - Automatically soft-delete expired permissions (background job)
        /// </remarks>
        [Display(Name = "Expires At")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Whether this permission assignment is currently active
        /// </summary>
        /// <remarks>
        /// Allows temporarily disabling permissions without deleting record:
        /// - Suspend during investigation
        /// - Inactive during user leave/vacation
        /// - Re-enable when user returns
        ///
        /// Different from IsDeleted:
        /// - IsActive: Temporary suspension (easily reversed)
        /// - IsDeleted: Soft delete (permanent removal)
        /// </remarks>
        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Navigation property to User entity
        /// </summary>
        /// <remarks>
        /// Allows eager loading:
        /// <code>
        /// var permissions = await _db.UserPermissions
        ///     .Include(up => up.User)
        ///     .Where(up => up.UserId == userId)
        ///     .ToListAsync();
        /// </code>
        /// </remarks>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
