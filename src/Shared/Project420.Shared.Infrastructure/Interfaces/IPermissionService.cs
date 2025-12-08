using Project420.Shared.Core.Enums;

namespace Project420.Shared.Infrastructure.Interfaces
{
    /// <summary>
    /// Service for checking user permissions and module access
    /// </summary>
    /// <remarks>
    /// Implementation Strategy by Phase:
    ///
    /// PHASE 2 (POC - Current):
    /// - Hardcoded permission logic based on User.Role
    /// - Simple role-to-permission mapping in PermissionService
    /// - No database lookups for permissions
    /// - Fast and simple for proof of concept
    ///
    /// PHASE 4 (Future):
    /// - Database-driven permissions via UserPermission table
    /// - Dynamic permission assignment through admin UI
    /// - Permission inheritance (role defaults + user overrides)
    /// - Complex permission logic (grant/deny model)
    ///
    /// This interface remains unchanged across phases - only implementation changes
    ///
    /// Usage Example:
    /// <code>
    /// // In a controller or service
    /// public class SalesController
    /// {
    ///     private readonly IPermissionService _permissionService;
    ///
    ///     public async Task&lt;IActionResult&gt; ProcessRefund(int transactionId)
    ///     {
    ///         var userId = GetCurrentUserId();
    ///
    ///         // Check permission
    ///         if (!await _permissionService.UserCanAsync(userId, SystemPermission.RetailPOS_Process_Refund))
    ///         {
    ///             return Forbid("You do not have permission to process refunds");
    ///         }
    ///
    ///         // Process refund logic
    ///         ...
    ///     }
    /// }
    /// </code>
    ///
    /// Dependency Injection Setup (Program.cs):
    /// <code>
    /// // Phase 2: Hardcoded implementation
    /// builder.Services.AddScoped&lt;IPermissionService, HardcodedPermissionService&gt;();
    ///
    /// // Phase 4: Database implementation
    /// builder.Services.AddScoped&lt;IPermissionService, DatabasePermissionService&gt;();
    /// </code>
    /// </remarks>
    public interface IPermissionService
    {
        // ============================================================
        // PERMISSION CHECKS (Core Functionality)
        // ============================================================

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <param name="permission">Permission to verify</param>
        /// <returns>True if user has permission, false otherwise</returns>
        /// <remarks>
        /// Phase 2 Logic:
        /// - Look up user's role
        /// - Check hardcoded role-to-permission mapping
        /// - Return true/false
        ///
        /// Phase 4 Logic:
        /// 1. Check UserPermission table for explicit grant/deny
        /// 2. If no explicit permission, check role defaults
        /// 3. If no role default, return false
        ///
        /// Example:
        /// <code>
        /// if (await _permissionService.UserCanAsync(userId, SystemPermission.RetailPOS_Process_Refund))
        /// {
        ///     // Allow refund
        /// }
        /// </code>
        /// </remarks>
        Task<bool> UserCanAsync(int userId, SystemPermission permission);

        /// <summary>
        /// Check if user can access a specific module
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <param name="module">Module to verify access</param>
        /// <returns>True if user can access module, false otherwise</returns>
        /// <remarks>
        /// Module access includes:
        /// 1. License check: System has module licensed
        /// 2. User permissions: User has at least one permission in that module
        /// 3. Compliance checks: Background check, cannabis license (if required by module)
        /// 4. Module status: Module is active
        ///
        /// Example:
        /// <code>
        /// if (await _permissionService.UserCanAccessModuleAsync(userId, SystemModule.Plantation))
        /// {
        ///     // Show Plantation menu item
        /// }
        /// </code>
        /// </remarks>
        Task<bool> UserCanAccessModuleAsync(int userId, SystemModule module);

        // ============================================================
        // PERMISSION DISCOVERY (For UI)
        // ============================================================

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        /// <param name="userId">User ID to query</param>
        /// <returns>List of all permissions the user has</returns>
        /// <remarks>
        /// Used for:
        /// - Generating navigation menus (show only accessible items)
        /// - Client-side authorization (hide unavailable buttons)
        /// - Admin UI (display what user can do)
        ///
        /// Phase 2: Returns permissions based on role mapping
        /// Phase 4: Returns role defaults + explicit grants - explicit denies
        ///
        /// Example:
        /// <code>
        /// var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        /// foreach (var perm in permissions)
        /// {
        ///     Console.WriteLine($"User can: {perm}");
        /// }
        /// </code>
        /// </remarks>
        Task<List<SystemPermission>> GetUserPermissionsAsync(int userId);

        /// <summary>
        /// Get all modules user can access
        /// </summary>
        /// <param name="userId">User ID to query</param>
        /// <returns>List of modules the user can access</returns>
        /// <remarks>
        /// Used for:
        /// - Building navigation menu (show only accessible modules)
        /// - Dashboard configuration (show relevant widgets)
        /// - License compliance display
        ///
        /// Checks:
        /// - System has module licensed
        /// - User has permissions for that module
        /// - User meets compliance requirements (if module requires them)
        ///
        /// Example:
        /// <code>
        /// var modules = await _permissionService.GetUserModulesAsync(userId);
        /// foreach (var module in modules)
        /// {
        ///     // Add menu item for module
        ///     menu.Add(new MenuItem { Text = module.ToString(), Link = $"/{module}" });
        /// }
        /// </code>
        /// </remarks>
        Task<List<SystemModule>> GetUserModulesAsync(int userId);

        // ============================================================
        // BULK PERMISSION CHECKS (Performance Optimization)
        // ============================================================

        /// <summary>
        /// Check multiple permissions at once (performance optimization)
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <param name="permissions">List of permissions to verify</param>
        /// <returns>Dictionary of permission → has access (true/false)</returns>
        /// <remarks>
        /// More efficient than calling UserCanAsync multiple times
        /// Useful when checking many permissions for UI rendering
        ///
        /// Example:
        /// <code>
        /// var permsToCheck = new[]
        /// {
        ///     SystemPermission.RetailPOS_Create_Sale,
        ///     SystemPermission.RetailPOS_Process_Refund,
        ///     SystemPermission.RetailPOS_Void_Transaction
        /// };
        ///
        /// var results = await _permissionService.UserCanBulkAsync(userId, permsToCheck);
        ///
        /// if (results[SystemPermission.RetailPOS_Process_Refund])
        /// {
        ///     // Show refund button
        /// }
        /// </code>
        /// </remarks>
        Task<Dictionary<SystemPermission, bool>> UserCanBulkAsync(int userId, IEnumerable<SystemPermission> permissions);

        // ============================================================
        // ROLE-BASED QUERIES (For Admin UI - Phase 2)
        // ============================================================

        /// <summary>
        /// Get all permissions for a specific role
        /// </summary>
        /// <param name="role">Role name (e.g., "Manager", "Cashier")</param>
        /// <returns>List of permissions granted to this role</returns>
        /// <remarks>
        /// Phase 2: Returns hardcoded role permissions
        /// Phase 4: Could query database role definitions
        ///
        /// Used for:
        /// - Admin UI showing what each role can do
        /// - Role comparison
        /// - Documentation
        ///
        /// Example:
        /// <code>
        /// var managerPerms = await _permissionService.GetRolePermissionsAsync("Manager");
        /// Console.WriteLine($"Managers have {managerPerms.Count} permissions");
        /// </code>
        /// </remarks>
        Task<List<SystemPermission>> GetRolePermissionsAsync(string role);

        // ============================================================
        // FUTURE METHODS (Phase 4) - Optional for now
        // ============================================================

        // Phase 4: Add these methods when implementing database-driven permissions
        // Task GrantPermissionAsync(int userId, SystemPermission permission, string reason);
        // Task DenyPermissionAsync(int userId, SystemPermission permission, string reason);
        // Task RevokePermissionAsync(int userId, SystemPermission permission);
        // Task<List<UserPermission>> GetUserPermissionOverridesAsync(int userId);
    }
}
