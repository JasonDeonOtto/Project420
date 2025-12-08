using System.ComponentModel.DataAnnotations;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities.SystemAdministration
{
    /// <summary>
    /// Represents a physical station/terminal in this specific company (business-specific configuration)
    /// </summary>
    /// <remarks>
    /// Company-Specific Configuration:
    /// - This is NOT the global connection metadata (that's StationConnection in Shared DB)
    /// - This is the business-specific configuration for a terminal in THIS company
    ///
    /// Example:
    /// - StationConnection: "POS-STORE-01 can access Company A and Company B"
    /// - Station (Company A): "POS-STORE-01 is configured as 'Main Counter' in Front Area with Epson printer"
    /// - Station (Company B): "POS-STORE-01 is configured as 'Backup Terminal' in Stockroom with different printer"
    ///
    /// Managed by: Management module (back-office)
    /// Used by: POS module (reads configuration at startup)
    ///
    /// Phase 1 (Current): Basic POS station information
    /// - Name, Type, Location, Department
    ///
    /// Phase 2 (Future): Operational configuration
    /// - DefaultPricelist, AllowOfflineMode, ReceiptTemplate
    ///
    /// Phase 3 (Future): Advanced features
    /// - ShiftManagement, CashFloatAmount, MaxDiscountPercent
    ///
    /// Architecture Decision:
    /// - User per company model means each company configures stations independently
    /// - Same physical terminal (POS-STORE-01) has different Station records per company
    /// </remarks>
    public class Station : AuditableEntity
    {
        // ============================================================
        // BASIC STATION INFO (Phase 1)
        // ============================================================

        /// <summary>
        /// Station name (user-friendly identifier specific to this company)
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Main Counter POS"
        /// - "Back Office Terminal"
        /// - "Mobile Sales Rep Device"
        /// - "Self-Service Kiosk 1"
        ///
        /// Business Context:
        /// - Different companies may give the same physical terminal different names
        /// - POS-STORE-01 might be "Main Counter" in Company A, "Backup Terminal" in Company B
        /// </remarks>
        [Required(ErrorMessage = "Station name is required")]
        [MaxLength(200, ErrorMessage = "Station name cannot exceed 200 characters")]
        [Display(Name = "Station Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Station type/category
        /// </summary>
        /// <remarks>
        /// Standard Types:
        /// - POS: Point of Sale terminal (desktop)
        /// - Mobile: Tablet or handheld device
        /// - Kiosk: Self-service terminal
        /// - BackOffice: Administrative workstation
        /// - Warehouse: Inventory management terminal
        ///
        /// Usage:
        /// - Determines UI layout (POS vs BackOffice)
        /// - Feature availability (Kiosks have limited functions)
        /// - Reporting (transactions by station type)
        /// </remarks>
        [Required(ErrorMessage = "Station type is required")]
        [MaxLength(50, ErrorMessage = "Station type cannot exceed 50 characters")]
        [Display(Name = "Station Type")]
        public string StationType { get; set; } = "POS"; // Default to POS

        /// <summary>
        /// Physical location of this station within the company
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Front Counter - Left Side"
        /// - "Stockroom"
        /// - "Drive-Through Window"
        /// - "Mobile Sales Van"
        ///
        /// Business Value:
        /// - Inventory allocation (which warehouse serves this station)
        /// - Customer flow analysis
        /// - Staff scheduling
        /// - Security/access control
        /// </remarks>
        [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        /// <summary>
        /// Department this station belongs to
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Retail Sales"
        /// - "Cannabis Dispensary"
        /// - "Warehouse Operations"
        /// - "Customer Service"
        ///
        /// Business Value:
        /// - Cost allocation
        /// - Departmental reporting
        /// - Permission management
        /// - Performance tracking
        /// </remarks>
        [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        // ============================================================
        // PHASE 2 PROPERTIES (Future)
        // ============================================================
        // TODO Phase 2: Add these properties
        // - public int? DefaultPricelistId { get; set; }
        // - public bool AllowOfflineMode { get; set; }
        // - public string? ReceiptTemplate { get; set; }

        // ============================================================
        // PHASE 3 PROPERTIES (Future)
        // ============================================================
        // TODO Phase 3: Add these properties
        // - public bool RequireShiftManagement { get; set; }
        // - public decimal? CashFloatAmount { get; set; }
        // - public decimal? MaxDiscountPercent { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================

        /// <summary>
        /// Navigation property to peripherals configured for this station
        /// </summary>
        /// <remarks>
        /// One station can have multiple peripherals:
        /// - Receipt printer
        /// - Label printer
        /// - Barcode scanner
        /// - Cash drawer
        /// - Card reader
        /// - Scale
        /// </remarks>
        public virtual ICollection<StationPeripheral>? Peripherals { get; set; }
    }
}
