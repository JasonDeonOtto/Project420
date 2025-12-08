using Project420.Shared.Core.Enums;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Infrastructure.Services
{
    /// <summary>
    /// Hardcoded permission service for Phase 2 POC
    /// </summary>
    /// <remarks>
    /// PHASE 2 Implementation (Current):
    /// - Simple role-based permission mapping
    /// - No database lookups
    /// - Fast and easy to maintain
    /// - All logic in this file (easy to find and modify)
    ///
    /// How to modify permissions:
    /// 1. Find the role in the _rolePermissions dictionary
    /// 2. Add or remove permissions from the HashSet
    /// 3. Restart application (no database changes needed)
    ///
    /// Supported Roles:
    /// - SuperAdmin: Full system access
    /// - Admin: Most permissions, no SystemAdmin
    /// - Manager: Module management + POS operations
    /// - Supervisor: View + basic operations
    /// - Cashier: POS operations only
    /// - Inventory: Stock management
    /// - ReadOnly: View-only access
    ///
    /// PHASE 4 Migration Path:
    /// - Replace this service with DatabasePermissionService
    /// - Keep same IPermissionService interface
    /// - No code changes in controllers/services (dependency injection handles it)
    /// - Migrate role permissions to database seed data
    ///
    /// Usage (in Program.cs):
    /// <code>
    /// // Phase 2: Use this hardcoded service
    /// builder.Services.AddScoped&lt;IPermissionService, HardcodedPermissionService&gt;();
    ///
    /// // Phase 4: Switch to database service (same interface!)
    /// builder.Services.AddScoped&lt;IPermissionService, DatabasePermissionService&gt;();
    /// </code>
    /// </remarks>
    public class HardcodedPermissionService : IPermissionService
    {
        // ============================================================
        // ROLE-TO-PERMISSION MAPPING (Hardcoded for Phase 2)
        // ============================================================

        /// <summary>
        /// Hardcoded role-to-permission mapping
        /// </summary>
        /// <remarks>
        /// Modify this dictionary to change role permissions
        /// Each role has a HashSet of permissions for O(1) lookup performance
        /// </remarks>
        private static readonly Dictionary<string, HashSet<SystemPermission>> _rolePermissions = new()
        {
            // --------------------------------------------------------
            // SUPERADMIN - Full system access (all permissions)
            // --------------------------------------------------------
            ["SuperAdmin"] = new HashSet<SystemPermission>
            {
                // Management Module
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_Create_Customer,
                SystemPermission.Management_Edit_Customer,
                SystemPermission.Management_Delete_Customer,
                SystemPermission.Management_Manage_CustomerCredit,
                SystemPermission.Management_Export_Customers,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_Create_Product,
                SystemPermission.Management_Edit_Product,
                SystemPermission.Management_Delete_Product,
                SystemPermission.Management_Manage_Categories,
                SystemPermission.Management_Adjust_Inventory,
                SystemPermission.Management_Export_Products,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_Create_Pricelist,
                SystemPermission.Management_Edit_Pricelist,
                SystemPermission.Management_Delete_Pricelist,
                SystemPermission.Management_Activate_Pricelist,
                SystemPermission.Management_View_Users,
                SystemPermission.Management_Create_User,
                SystemPermission.Management_Edit_User,
                SystemPermission.Management_Delete_User,
                SystemPermission.Management_Assign_Permissions,
                SystemPermission.Management_Reset_Password,
                SystemPermission.Management_Unlock_User,
                SystemPermission.Management_View_Reports,
                SystemPermission.Management_Export_Reports,

                // Retail POS Module
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Refund,
                SystemPermission.RetailPOS_Void_Transaction,
                SystemPermission.RetailPOS_Apply_Discount,
                SystemPermission.RetailPOS_Override_Price,
                SystemPermission.RetailPOS_Open_CashDrawer,
                SystemPermission.RetailPOS_Cash_Drop,
                SystemPermission.RetailPOS_Close_Shift,
                SystemPermission.RetailPOS_View_CashBalance,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport,
                SystemPermission.RetailPOS_Export_Reports,

                // Plantation Module
                SystemPermission.Plantation_View_Plants,
                SystemPermission.Plantation_Create_Plant,
                SystemPermission.Plantation_Update_Plant,
                SystemPermission.Plantation_Record_Harvest,
                SystemPermission.Plantation_Record_Waste,

                // Production Module
                SystemPermission.Production_View_Batches,
                SystemPermission.Production_Create_Batch,
                SystemPermission.Production_Update_Batch,
                SystemPermission.Production_Record_QC,

                // Compliance Module
                SystemPermission.Compliance_View_Dashboard,
                SystemPermission.Compliance_Generate_SAHPRA,
                SystemPermission.Compliance_Generate_DALRRD,
                SystemPermission.Compliance_Generate_SARS,
                SystemPermission.Compliance_View_AuditLogs,
                SystemPermission.Compliance_Export_Reports,

                // Analytics Module
                SystemPermission.Analytics_View_Sales,
                SystemPermission.Analytics_View_Inventory,
                SystemPermission.Analytics_View_Customers,
                SystemPermission.Analytics_View_Financial,
                SystemPermission.Analytics_Create_Dashboard,
                SystemPermission.Analytics_Export_Data,

                // Purchasing Module
                SystemPermission.Purchasing_View_PO,
                SystemPermission.Purchasing_Create_PO,
                SystemPermission.Purchasing_Edit_PO,
                SystemPermission.Purchasing_Approve_PO,
                SystemPermission.Purchasing_Cancel_PO,
                SystemPermission.Purchasing_View_Suppliers,
                SystemPermission.Purchasing_Create_Supplier,
                SystemPermission.Purchasing_Edit_Supplier,
                SystemPermission.Purchasing_Manage_Payments,

                // System Admin Module
                SystemPermission.SystemAdmin_Access,
                SystemPermission.SystemAdmin_Edit_Settings,
                SystemPermission.SystemAdmin_View_Logs,
                SystemPermission.SystemAdmin_Manage_Backups,
                SystemPermission.SystemAdmin_Manage_Licenses,
                SystemPermission.SystemAdmin_Database_Maintenance
            },

            // --------------------------------------------------------
            // ADMIN - Most permissions except SystemAdmin
            // --------------------------------------------------------
            ["Admin"] = new HashSet<SystemPermission>
            {
                // Management Module (full access)
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_Create_Customer,
                SystemPermission.Management_Edit_Customer,
                SystemPermission.Management_Delete_Customer,
                SystemPermission.Management_Manage_CustomerCredit,
                SystemPermission.Management_Export_Customers,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_Create_Product,
                SystemPermission.Management_Edit_Product,
                SystemPermission.Management_Delete_Product,
                SystemPermission.Management_Manage_Categories,
                SystemPermission.Management_Adjust_Inventory,
                SystemPermission.Management_Export_Products,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_Create_Pricelist,
                SystemPermission.Management_Edit_Pricelist,
                SystemPermission.Management_Delete_Pricelist,
                SystemPermission.Management_Activate_Pricelist,
                SystemPermission.Management_View_Users,
                SystemPermission.Management_Create_User,
                SystemPermission.Management_Edit_User,
                SystemPermission.Management_Delete_User,
                SystemPermission.Management_Assign_Permissions,
                SystemPermission.Management_Reset_Password,
                SystemPermission.Management_Unlock_User,
                SystemPermission.Management_View_Reports,
                SystemPermission.Management_Export_Reports,

                // Retail POS Module (full access)
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Refund,
                SystemPermission.RetailPOS_Void_Transaction,
                SystemPermission.RetailPOS_Apply_Discount,
                SystemPermission.RetailPOS_Override_Price,
                SystemPermission.RetailPOS_Open_CashDrawer,
                SystemPermission.RetailPOS_Cash_Drop,
                SystemPermission.RetailPOS_Close_Shift,
                SystemPermission.RetailPOS_View_CashBalance,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport,
                SystemPermission.RetailPOS_Export_Reports,

                // Compliance Module
                SystemPermission.Compliance_View_Dashboard,
                SystemPermission.Compliance_View_AuditLogs,
                SystemPermission.Compliance_Export_Reports,

                // Analytics Module
                SystemPermission.Analytics_View_Sales,
                SystemPermission.Analytics_View_Inventory,
                SystemPermission.Analytics_View_Customers,
                SystemPermission.Analytics_View_Financial,
                SystemPermission.Analytics_Export_Data,

                // Purchasing Module
                SystemPermission.Purchasing_View_PO,
                SystemPermission.Purchasing_Create_PO,
                SystemPermission.Purchasing_Edit_PO,
                SystemPermission.Purchasing_Approve_PO,
                SystemPermission.Purchasing_View_Suppliers,
                SystemPermission.Purchasing_Create_Supplier,
                SystemPermission.Purchasing_Edit_Supplier

                // NOTE: No SystemAdmin permissions
            },

            // --------------------------------------------------------
            // MANAGER - Store/department management + POS
            // --------------------------------------------------------
            ["Manager"] = new HashSet<SystemPermission>
            {
                // Management Module (limited)
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_Create_Customer,
                SystemPermission.Management_Edit_Customer,
                SystemPermission.Management_Manage_CustomerCredit,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_Edit_Product,
                SystemPermission.Management_Adjust_Inventory,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_View_Reports,
                SystemPermission.Management_Export_Reports,

                // Retail POS Module (full access)
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Refund,
                SystemPermission.RetailPOS_Void_Transaction,
                SystemPermission.RetailPOS_Apply_Discount,
                SystemPermission.RetailPOS_Override_Price,
                SystemPermission.RetailPOS_Open_CashDrawer,
                SystemPermission.RetailPOS_Cash_Drop,
                SystemPermission.RetailPOS_Close_Shift,
                SystemPermission.RetailPOS_View_CashBalance,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport,
                SystemPermission.RetailPOS_Export_Reports,

                // Analytics Module (view only)
                SystemPermission.Analytics_View_Sales,
                SystemPermission.Analytics_View_Inventory,
                SystemPermission.Analytics_View_Customers
            },

            // --------------------------------------------------------
            // SUPERVISOR - Team supervision + basic POS
            // --------------------------------------------------------
            ["Supervisor"] = new HashSet<SystemPermission>
            {
                // Management Module (view mostly)
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_View_Reports,

                // Retail POS Module (most operations)
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Refund,
                SystemPermission.RetailPOS_Apply_Discount,
                SystemPermission.RetailPOS_Open_CashDrawer,
                SystemPermission.RetailPOS_Cash_Drop,
                SystemPermission.RetailPOS_View_CashBalance,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport
            },

            // --------------------------------------------------------
            // CASHIER - POS operations only (most restrictive)
            // --------------------------------------------------------
            ["Cashier"] = new HashSet<SystemPermission>
            {
                // Management Module (view only basics)
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_View_Products,

                // Retail POS Module (basic sales operations)
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,

                // NOTE: Cashiers CANNOT:
                // - Process refunds (requires Manager+)
                // - Void transactions (requires Manager+)
                // - Override prices (requires Manager+)
                // - Open cash drawer without sale (requires Supervisor+)
                // - Close shift (requires Supervisor+)
            },

            // --------------------------------------------------------
            // INVENTORY - Stock management focus
            // --------------------------------------------------------
            ["Inventory"] = new HashSet<SystemPermission>
            {
                // Management Module (product/inventory focus)
                SystemPermission.Management_View_Products,
                SystemPermission.Management_Create_Product,
                SystemPermission.Management_Edit_Product,
                SystemPermission.Management_Manage_Categories,
                SystemPermission.Management_Adjust_Inventory,
                SystemPermission.Management_Export_Products,
                SystemPermission.Management_View_Pricelists,

                // Analytics Module (inventory reports)
                SystemPermission.Analytics_View_Inventory,

                // Purchasing Module (receiving goods)
                SystemPermission.Purchasing_View_PO,
                SystemPermission.Purchasing_View_Suppliers
            },

            // --------------------------------------------------------
            // READONLY - View-only access (reports, audits)
            // --------------------------------------------------------
            ["ReadOnly"] = new HashSet<SystemPermission>
            {
                // Management Module (view only)
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_View_Users,
                SystemPermission.Management_View_Reports,

                // Retail POS Module (view only)
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport,

                // Compliance Module (view only)
                SystemPermission.Compliance_View_Dashboard,
                SystemPermission.Compliance_View_AuditLogs,

                // Analytics Module (view only)
                SystemPermission.Analytics_View_Sales,
                SystemPermission.Analytics_View_Inventory,
                SystemPermission.Analytics_View_Customers,
                SystemPermission.Analytics_View_Financial
            }
        };

        // ============================================================
        // MODULE-TO-PERMISSIONS MAPPING (For module access checks)
        // ============================================================

        /// <summary>
        /// Map each module to its permissions (for module access checks)
        /// </summary>
        private static readonly Dictionary<SystemModule, HashSet<SystemPermission>> _modulePermissions = new()
        {
            [SystemModule.Management] = new HashSet<SystemPermission>
            {
                SystemPermission.Management_View_Customers,
                SystemPermission.Management_Create_Customer,
                SystemPermission.Management_Edit_Customer,
                SystemPermission.Management_Delete_Customer,
                SystemPermission.Management_Manage_CustomerCredit,
                SystemPermission.Management_Export_Customers,
                SystemPermission.Management_View_Products,
                SystemPermission.Management_Create_Product,
                SystemPermission.Management_Edit_Product,
                SystemPermission.Management_Delete_Product,
                SystemPermission.Management_Manage_Categories,
                SystemPermission.Management_Adjust_Inventory,
                SystemPermission.Management_Export_Products,
                SystemPermission.Management_View_Pricelists,
                SystemPermission.Management_Create_Pricelist,
                SystemPermission.Management_Edit_Pricelist,
                SystemPermission.Management_Delete_Pricelist,
                SystemPermission.Management_Activate_Pricelist,
                SystemPermission.Management_View_Users,
                SystemPermission.Management_Create_User,
                SystemPermission.Management_Edit_User,
                SystemPermission.Management_Delete_User,
                SystemPermission.Management_Assign_Permissions,
                SystemPermission.Management_Reset_Password,
                SystemPermission.Management_Unlock_User,
                SystemPermission.Management_View_Reports,
                SystemPermission.Management_Export_Reports
            },

            [SystemModule.RetailPOS] = new HashSet<SystemPermission>
            {
                SystemPermission.RetailPOS_Create_Sale,
                SystemPermission.RetailPOS_View_Sales,
                SystemPermission.RetailPOS_Process_Refund,
                SystemPermission.RetailPOS_Void_Transaction,
                SystemPermission.RetailPOS_Apply_Discount,
                SystemPermission.RetailPOS_Override_Price,
                SystemPermission.RetailPOS_Open_CashDrawer,
                SystemPermission.RetailPOS_Cash_Drop,
                SystemPermission.RetailPOS_Close_Shift,
                SystemPermission.RetailPOS_View_CashBalance,
                SystemPermission.RetailPOS_Process_Cash,
                SystemPermission.RetailPOS_Process_Card,
                SystemPermission.RetailPOS_Process_EFT,
                SystemPermission.RetailPOS_Process_OnAccount,
                SystemPermission.RetailPOS_Split_Payment,
                SystemPermission.RetailPOS_View_DailyReport,
                SystemPermission.RetailPOS_View_ShiftReport,
                SystemPermission.RetailPOS_Export_Reports
            },

            [SystemModule.Plantation] = new HashSet<SystemPermission>
            {
                SystemPermission.Plantation_View_Plants,
                SystemPermission.Plantation_Create_Plant,
                SystemPermission.Plantation_Update_Plant,
                SystemPermission.Plantation_Record_Harvest,
                SystemPermission.Plantation_Record_Waste
            },

            [SystemModule.Production] = new HashSet<SystemPermission>
            {
                SystemPermission.Production_View_Batches,
                SystemPermission.Production_Create_Batch,
                SystemPermission.Production_Update_Batch,
                SystemPermission.Production_Record_QC
            },

            [SystemModule.Compliance] = new HashSet<SystemPermission>
            {
                SystemPermission.Compliance_View_Dashboard,
                SystemPermission.Compliance_Generate_SAHPRA,
                SystemPermission.Compliance_Generate_DALRRD,
                SystemPermission.Compliance_Generate_SARS,
                SystemPermission.Compliance_View_AuditLogs,
                SystemPermission.Compliance_Export_Reports
            },

            [SystemModule.Analytics] = new HashSet<SystemPermission>
            {
                SystemPermission.Analytics_View_Sales,
                SystemPermission.Analytics_View_Inventory,
                SystemPermission.Analytics_View_Customers,
                SystemPermission.Analytics_View_Financial,
                SystemPermission.Analytics_Create_Dashboard,
                SystemPermission.Analytics_Export_Data
            },

            [SystemModule.Purchasing] = new HashSet<SystemPermission>
            {
                SystemPermission.Purchasing_View_PO,
                SystemPermission.Purchasing_Create_PO,
                SystemPermission.Purchasing_Edit_PO,
                SystemPermission.Purchasing_Approve_PO,
                SystemPermission.Purchasing_Cancel_PO,
                SystemPermission.Purchasing_View_Suppliers,
                SystemPermission.Purchasing_Create_Supplier,
                SystemPermission.Purchasing_Edit_Supplier,
                SystemPermission.Purchasing_Manage_Payments
            },

            [SystemModule.SystemAdmin] = new HashSet<SystemPermission>
            {
                SystemPermission.SystemAdmin_Access,
                SystemPermission.SystemAdmin_Edit_Settings,
                SystemPermission.SystemAdmin_View_Logs,
                SystemPermission.SystemAdmin_Manage_Backups,
                SystemPermission.SystemAdmin_Manage_Licenses,
                SystemPermission.SystemAdmin_Database_Maintenance
            }
        };

        // ============================================================
        // IMPLEMENTATION (Interface Methods)
        // ============================================================

        // NOTE: This is a simplified Phase 2 implementation
        // In production, inject a User repository to fetch user data
        // For now, we'll need the consuming code to pass user role or fetch it themselves

        public Task<bool> UserCanAsync(int userId, SystemPermission permission)
        {
            // TODO Phase 2: Inject IUserRepository and fetch user.Role
            // For now, this is a placeholder that assumes role is passed via context
            // Implementation will be completed when integrating with BLL/DAL
            throw new NotImplementedException("Phase 2: Requires User repository injection to fetch user role");
        }

        public Task<bool> UserCanAccessModuleAsync(int userId, SystemModule module)
        {
            // TODO Phase 2: Inject IUserRepository and fetch user.Role
            throw new NotImplementedException("Phase 2: Requires User repository injection to fetch user role");
        }

        public Task<List<SystemPermission>> GetUserPermissionsAsync(int userId)
        {
            // TODO Phase 2: Inject IUserRepository and fetch user.Role
            throw new NotImplementedException("Phase 2: Requires User repository injection to fetch user role");
        }

        public Task<List<SystemModule>> GetUserModulesAsync(int userId)
        {
            // TODO Phase 2: Inject IUserRepository and fetch user role
            throw new NotImplementedException("Phase 2: Requires User repository injection to fetch user role");
        }

        public Task<Dictionary<SystemPermission, bool>> UserCanBulkAsync(int userId, IEnumerable<SystemPermission> permissions)
        {
            // TODO Phase 2: Inject IUserRepository and fetch user.Role
            throw new NotImplementedException("Phase 2: Requires User repository injection to fetch user role");
        }

        public Task<List<SystemPermission>> GetRolePermissionsAsync(string role)
        {
            // This method can work without user repository
            if (!_rolePermissions.ContainsKey(role))
            {
                return Task.FromResult(new List<SystemPermission>());
            }

            return Task.FromResult(_rolePermissions[role].ToList());
        }

        // ============================================================
        // HELPER METHODS (For when User repository is injected)
        // ============================================================

        /// <summary>
        /// Helper method: Check if role has permission (call this from implemented methods)
        /// </summary>
        private bool RoleHasPermission(string role, SystemPermission permission)
        {
            if (!_rolePermissions.ContainsKey(role))
            {
                return false; // Unknown role = no permissions
            }

            return _rolePermissions[role].Contains(permission);
        }

        /// <summary>
        /// Helper method: Get all permissions for a role
        /// </summary>
        private HashSet<SystemPermission> GetRolePermissionsInternal(string role)
        {
            if (!_rolePermissions.ContainsKey(role))
            {
                return new HashSet<SystemPermission>();
            }

            return _rolePermissions[role];
        }

        /// <summary>
        /// Helper method: Check if user has any permission in a module
        /// </summary>
        private bool RoleCanAccessModule(string role, SystemModule module)
        {
            var rolePerms = GetRolePermissionsInternal(role);
            var modulePerms = _modulePermissions[module];

            // User can access module if they have at least one permission from that module
            return rolePerms.Any(perm => modulePerms.Contains(perm));
        }
    }
}
