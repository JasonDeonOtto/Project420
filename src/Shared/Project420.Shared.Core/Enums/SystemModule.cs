namespace Project420.Shared.Core.Enums
{
    /// <summary>
    /// Defines the main functional modules/areas in the Project420 system
    /// </summary>
    /// <remarks>
    /// System Modules represent major functional areas that can be:
    /// - Licensed separately (per module licensing)
    /// - Accessed based on user permissions
    /// - Enabled/disabled by system configuration
    ///
    /// Module Structure:
    /// - Management: Core business management (customers, products, pricing)
    /// - Retail: Point of Sale and retail operations
    /// - Plantation: Cannabis cultivation tracking
    /// - Production: Processing and manufacturing
    /// - Compliance: Regulatory reporting and tracking
    /// - Analytics: Business intelligence and reporting
    ///
    /// Licensing:
    /// - Core modules (Management, Retail) included in all licenses
    /// - Premium modules (Plantation, Production) require higher license tiers
    /// - Compliance and Analytics may be add-ons
    ///
    /// Usage:
    /// <code>
    /// if (license.HasModule(SystemModule.Plantation))
    /// {
    ///     // Allow access to cultivation features
    /// }
    /// </code>
    /// </remarks>
    public enum SystemModule
    {
        /// <summary>
        /// Management module - Core business entity management
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Customer/Debtor management
        /// - Product catalog management
        /// - Pricelist management
        /// - User management (admin)
        /// - System configuration
        ///
        /// License: Included in all tiers (Core)
        /// Required by: All other modules
        /// </remarks>
        Management = 1,

        /// <summary>
        /// Retail POS module - Point of Sale operations
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - POS transaction processing
        /// - Shopping cart management
        /// - Payment processing
        /// - Cash drawer operations
        /// - Daily sales reports
        ///
        /// License: Included in all tiers (Core)
        /// Cannabis Compliance: Age verification required
        /// </remarks>
        RetailPOS = 2,

        /// <summary>
        /// Plantation module - Cannabis cultivation tracking
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Plant tracking (seed-to-harvest)
        /// - Growth stage management
        /// - Cultivation area tracking
        /// - Harvest recording
        /// - Waste tracking
        ///
        /// License: Professional tier and above
        /// Cannabis Compliance: DALRRD cultivation permit required
        /// Background check: Required for all users
        /// </remarks>
        Plantation = 3,

        /// <summary>
        /// Production module - Processing and manufacturing
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Processing workflow management
        /// - Extraction tracking
        /// - Product manufacturing
        /// - Quality control
        /// - Batch management
        ///
        /// License: Professional tier and above
        /// Cannabis Compliance: GMP certification may be required
        /// Background check: Required for all users
        /// </remarks>
        Production = 4,

        /// <summary>
        /// Compliance module - Regulatory reporting and tracking
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - SAHPRA report generation
        /// - DALRRD report generation
        /// - SARS tax reporting
        /// - Audit log viewing
        /// - License/permit tracking
        /// - Compliance dashboard
        ///
        /// License: Add-on or included in Enterprise tier
        /// Required for: Commercial cannabis operations
        /// </remarks>
        Compliance = 5,

        /// <summary>
        /// Analytics module - Business intelligence and reporting
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Sales analytics
        /// - Inventory analytics
        /// - Customer analytics
        /// - Financial reports
        /// - Custom dashboards
        /// - Data exports
        ///
        /// License: Professional tier and above
        /// </remarks>
        Analytics = 6,

        /// <summary>
        /// Purchasing module - Supplier and purchase order management
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Supplier management
        /// - Purchase order creation
        /// - Goods receiving
        /// - Purchase invoicing
        /// - Supplier payments
        ///
        /// License: Professional tier and above
        /// </remarks>
        Purchasing = 7,

        /// <summary>
        /// System Administration module - System-level configuration
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - System settings
        /// - User management
        /// - Role/permission management
        /// - License management
        /// - Database maintenance
        /// - System logs
        ///
        /// License: Included in all tiers (Core)
        /// Access: Restricted to SuperAdmin role only
        /// </remarks>
        SystemAdmin = 99
    }
}
