namespace Project420.Shared.Core.Enums
{
    /// <summary>
    /// Defines granular permissions for specific actions within system modules
    /// </summary>
    /// <remarks>
    /// Permission Naming Convention: Module_Action_Resource
    /// - Module: Which system module (Management, RetailPOS, etc.)
    /// - Action: What operation (Create, View, Edit, Delete, Process, Approve, Export)
    /// - Resource: What entity/feature (Sale, Product, User, Report)
    ///
    /// Permission Ranges (by module):
    /// - 1000-1999: Management module
    /// - 2000-2999: Retail POS module
    /// - 3000-3999: Plantation module
    /// - 4000-4999: Production module
    /// - 5000-5999: Compliance module
    /// - 6000-6999: Analytics module
    /// - 7000-7999: Purchasing module
    /// - 9000-9999: System Admin module
    ///
    /// Usage in code:
    /// <code>
    /// if (await _permissionService.UserCanAsync(userId, SystemPermission.RetailPOS_Process_Refund))
    /// {
    ///     // Allow refund processing
    /// }
    /// </code>
    ///
    /// Role Assignment (Phase 2 - Hardcoded):
    /// - SuperAdmin: All permissions
    /// - Admin: Most permissions except SystemAdmin
    /// - Manager: Module-specific management permissions
    /// - Supervisor: View + basic operations
    /// - Cashier: Basic POS operations only
    /// - ReadOnly: View permissions only
    ///
    /// Future (Phase 4):
    /// - Store in UserPermission table
    /// - Dynamic assignment via admin UI
    /// - Permission inheritance (roles grant base permissions, then customize per user)
    /// </remarks>
    public enum SystemPermission
    {
        // ============================================================
        // MANAGEMENT MODULE (1000-1999)
        // ============================================================

        #region Customer Management (1000-1099)

        /// <summary>View customer/debtor list and details</summary>
        Management_View_Customers = 1001,

        /// <summary>Create new customer/debtor accounts</summary>
        Management_Create_Customer = 1002,

        /// <summary>Edit existing customer details</summary>
        Management_Edit_Customer = 1003,

        /// <summary>Delete customers (soft delete)</summary>
        Management_Delete_Customer = 1004,

        /// <summary>Manage customer credit limits and terms</summary>
        Management_Manage_CustomerCredit = 1005,

        /// <summary>Export customer data to CSV/Excel</summary>
        Management_Export_Customers = 1006,

        #endregion

        #region Product Management (1100-1199)

        /// <summary>View product catalog</summary>
        Management_View_Products = 1101,

        /// <summary>Create new products</summary>
        Management_Create_Product = 1102,

        /// <summary>Edit product details, pricing, inventory</summary>
        Management_Edit_Product = 1103,

        /// <summary>Delete products (soft delete)</summary>
        Management_Delete_Product = 1104,

        /// <summary>Manage product categories</summary>
        Management_Manage_Categories = 1105,

        /// <summary>Update product inventory/stock levels</summary>
        Management_Adjust_Inventory = 1106,

        /// <summary>Export product data</summary>
        Management_Export_Products = 1107,

        #endregion

        #region Pricelist Management (1200-1299)

        /// <summary>View pricelists and pricing</summary>
        Management_View_Pricelists = 1201,

        /// <summary>Create new pricelists</summary>
        Management_Create_Pricelist = 1202,

        /// <summary>Edit pricelist details and items</summary>
        Management_Edit_Pricelist = 1203,

        /// <summary>Delete pricelists</summary>
        Management_Delete_Pricelist = 1204,

        /// <summary>Activate/deactivate pricelists</summary>
        Management_Activate_Pricelist = 1205,

        #endregion

        #region User Management (1300-1399)

        /// <summary>View user list and details</summary>
        Management_View_Users = 1301,

        /// <summary>Create new user accounts</summary>
        Management_Create_User = 1302,

        /// <summary>Edit user details and profiles</summary>
        Management_Edit_User = 1303,

        /// <summary>Delete/deactivate users</summary>
        Management_Delete_User = 1304,

        /// <summary>Assign roles and permissions to users</summary>
        Management_Assign_Permissions = 1305,

        /// <summary>Reset user passwords</summary>
        Management_Reset_Password = 1306,

        /// <summary>Unlock locked user accounts</summary>
        Management_Unlock_User = 1307,

        #endregion

        #region Reports (1400-1499)

        /// <summary>View management reports</summary>
        Management_View_Reports = 1401,

        /// <summary>Export reports to PDF/Excel</summary>
        Management_Export_Reports = 1402,

        #endregion

        // ============================================================
        // RETAIL POS MODULE (2000-2999)
        // ============================================================

        #region Sales Operations (2000-2099)

        /// <summary>Create and process sales transactions</summary>
        RetailPOS_Create_Sale = 2001,

        /// <summary>View sales history</summary>
        RetailPOS_View_Sales = 2002,

        /// <summary>Process refunds and returns</summary>
        RetailPOS_Process_Refund = 2003,

        /// <summary>Void/cancel transactions</summary>
        RetailPOS_Void_Transaction = 2004,

        /// <summary>Apply discounts to line items or transactions</summary>
        RetailPOS_Apply_Discount = 2005,

        /// <summary>Override prices (requires manager approval typically)</summary>
        RetailPOS_Override_Price = 2006,

        #endregion

        #region Cash Management (2100-2199)

        /// <summary>Open cash drawer</summary>
        RetailPOS_Open_CashDrawer = 2101,

        /// <summary>Perform cash drops (remove excess cash)</summary>
        RetailPOS_Cash_Drop = 2102,

        /// <summary>Close shift and cash up</summary>
        RetailPOS_Close_Shift = 2103,

        /// <summary>View cash drawer balance</summary>
        RetailPOS_View_CashBalance = 2104,

        #endregion

        #region Payment Processing (2200-2299)

        /// <summary>Process cash payments</summary>
        RetailPOS_Process_Cash = 2201,

        /// <summary>Process card payments</summary>
        RetailPOS_Process_Card = 2202,

        /// <summary>Process EFT payments</summary>
        RetailPOS_Process_EFT = 2203,

        /// <summary>Process account/credit payments</summary>
        RetailPOS_Process_OnAccount = 2204,

        /// <summary>Accept split payments (multiple payment methods)</summary>
        RetailPOS_Split_Payment = 2205,

        #endregion

        #region POS Reports (2300-2399)

        /// <summary>View daily sales reports</summary>
        RetailPOS_View_DailyReport = 2301,

        /// <summary>View shift reports</summary>
        RetailPOS_View_ShiftReport = 2302,

        /// <summary>Export POS reports</summary>
        RetailPOS_Export_Reports = 2303,

        #endregion

        // ============================================================
        // PLANTATION MODULE (3000-3999)
        // ============================================================

        #region Cultivation (3000-3099)

        /// <summary>View cultivation areas and plants</summary>
        Plantation_View_Plants = 3001,

        /// <summary>Add new plants to tracking system</summary>
        Plantation_Create_Plant = 3002,

        /// <summary>Update plant growth stages and status</summary>
        Plantation_Update_Plant = 3003,

        /// <summary>Record harvest activities</summary>
        Plantation_Record_Harvest = 3004,

        /// <summary>Record plant destruction/waste</summary>
        Plantation_Record_Waste = 3005,

        #endregion

        // ============================================================
        // PRODUCTION MODULE (4000-4999)
        // ============================================================

        #region Processing (4000-4099)

        /// <summary>View production batches</summary>
        Production_View_Batches = 4001,

        /// <summary>Create production batches</summary>
        Production_Create_Batch = 4002,

        /// <summary>Update batch processing status</summary>
        Production_Update_Batch = 4003,

        /// <summary>Record quality control results</summary>
        Production_Record_QC = 4004,

        #endregion

        // ============================================================
        // COMPLIANCE MODULE (5000-5999)
        // ============================================================

        #region Compliance Reporting (5000-5099)

        /// <summary>View compliance dashboard</summary>
        Compliance_View_Dashboard = 5001,

        /// <summary>Generate SAHPRA reports</summary>
        Compliance_Generate_SAHPRA = 5002,

        /// <summary>Generate DALRRD reports</summary>
        Compliance_Generate_DALRRD = 5003,

        /// <summary>Generate SARS tax reports</summary>
        Compliance_Generate_SARS = 5004,

        /// <summary>View audit logs</summary>
        Compliance_View_AuditLogs = 5005,

        /// <summary>Export compliance reports</summary>
        Compliance_Export_Reports = 5006,

        #endregion

        // ============================================================
        // ANALYTICS MODULE (6000-6999)
        // ============================================================

        #region Business Intelligence (6000-6099)

        /// <summary>View sales analytics</summary>
        Analytics_View_Sales = 6001,

        /// <summary>View inventory analytics</summary>
        Analytics_View_Inventory = 6002,

        /// <summary>View customer analytics</summary>
        Analytics_View_Customers = 6003,

        /// <summary>View financial analytics</summary>
        Analytics_View_Financial = 6004,

        /// <summary>Create custom dashboards</summary>
        Analytics_Create_Dashboard = 6005,

        /// <summary>Export analytics data</summary>
        Analytics_Export_Data = 6006,

        #endregion

        // ============================================================
        // PURCHASING MODULE (7000-7999)
        // ============================================================

        #region Purchase Orders (7000-7099)

        /// <summary>View purchase orders</summary>
        Purchasing_View_PO = 7001,

        /// <summary>Create purchase orders</summary>
        Purchasing_Create_PO = 7002,

        /// <summary>Edit purchase orders</summary>
        Purchasing_Edit_PO = 7003,

        /// <summary>Approve purchase orders</summary>
        Purchasing_Approve_PO = 7004,

        /// <summary>Cancel purchase orders</summary>
        Purchasing_Cancel_PO = 7005,

        #endregion

        #region Suppliers (7100-7199)

        /// <summary>View supplier list</summary>
        Purchasing_View_Suppliers = 7101,

        /// <summary>Create new suppliers</summary>
        Purchasing_Create_Supplier = 7102,

        /// <summary>Edit supplier details</summary>
        Purchasing_Edit_Supplier = 7103,

        /// <summary>Manage supplier payments</summary>
        Purchasing_Manage_Payments = 7104,

        #endregion

        // ============================================================
        // SYSTEM ADMIN MODULE (9000-9999)
        // ============================================================

        #region System Configuration (9000-9099)

        /// <summary>Access system administration area</summary>
        SystemAdmin_Access = 9001,

        /// <summary>Modify system settings</summary>
        SystemAdmin_Edit_Settings = 9002,

        /// <summary>View system logs</summary>
        SystemAdmin_View_Logs = 9003,

        /// <summary>Manage database backups</summary>
        SystemAdmin_Manage_Backups = 9004,

        /// <summary>Manage system licenses</summary>
        SystemAdmin_Manage_Licenses = 9005,

        /// <summary>Execute database maintenance</summary>
        SystemAdmin_Database_Maintenance = 9006,

        #endregion
    }
}
