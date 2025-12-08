using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Management.Models.Entities.SystemAdministration;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities
{
    /// <summary>
    /// Represents a peripheral device configured for a station
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Track printers, scanners, cash drawers, card readers, scales
    /// - Store connection information (IP, port, serial port)
    /// - Configuration settings per device
    ///
    /// Phase 1 (Current): Basic structure
    /// - PeripheralType, Model, ConnectionType
    ///
    /// Phase 2 (Future): Connection details
    /// - IPAddress, Port, Settings (JSON config)
    ///
    /// Phase 3 (Future): Monitoring and management
    /// - Status monitoring, error logging, auto-recovery
    ///
    /// Examples:
    /// - Receipt Printer: Epson TM-T88VI via Network (192.168.1.100:9100)
    /// - Cash Drawer: APG Vasario connected via printer port
    /// - Barcode Scanner: Zebra DS2278 via USB
    /// - Card Reader: Ingenico iCT250 via COM3
    /// - Scale: Mettler Toledo via RS232
    /// </remarks>
    public class StationPeripheral : AuditableEntity
    {
        // ============================================================
        // FOREIGN KEY
        // ============================================================

        /// <summary>
        /// Foreign key to Station
        /// </summary>
        [Required(ErrorMessage = "Station ID is required")]
        [Display(Name = "Station ID")]
        public int StationId { get; set; }

        // ============================================================
        // BASIC PERIPHERAL INFO (Phase 1)
        // ============================================================

        /// <summary>
        /// Type of peripheral device
        /// </summary>
        /// <remarks>
        /// Standard Types:
        /// - Printer: Receipt, label, or invoice printer
        /// - Scanner: Barcode or QR code scanner
        /// - CashDrawer: Cash register drawer
        /// - CardReader: Credit/debit card payment terminal
        /// - Scale: Weight scale for products
        /// - Display: Customer-facing display
        ///
        /// Cannabis-Specific:
        /// - Scale: Weight verification for compliance
        /// - LabelPrinter: THC/CBD warning labels
        /// </remarks>
        [Required(ErrorMessage = "Peripheral type is required")]
        [MaxLength(50, ErrorMessage = "Peripheral type cannot exceed 50 characters")]
        [Display(Name = "Peripheral Type")]
        public string PeripheralType { get; set; } = string.Empty;

        /// <summary>
        /// Device model/brand
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Epson TM-T88VI"
        /// - "Zebra DS2278"
        /// - "APG Vasario 1616"
        /// - "Ingenico iCT250"
        /// - "Mettler Toledo BC-60"
        ///
        /// Business Value:
        /// - Driver installation
        /// - Troubleshooting
        /// - Warranty tracking
        /// - Replacement planning
        /// </remarks>
        [MaxLength(200, ErrorMessage = "Model cannot exceed 200 characters")]
        [Display(Name = "Model")]
        public string? Model { get; set; }

        /// <summary>
        /// How the peripheral connects to the station
        /// </summary>
        /// <remarks>
        /// Connection Types:
        /// - USB: Direct USB connection
        /// - Network: TCP/IP network connection
        /// - Serial: RS232/COM port
        /// - Bluetooth: Wireless connection
        /// - WiFi: Wireless network
        ///
        /// Determines:
        /// - Configuration fields needed
        /// - Driver requirements
        /// - Troubleshooting steps
        /// </remarks>
        [MaxLength(50, ErrorMessage = "Connection type cannot exceed 50 characters")]
        [Display(Name = "Connection Type")]
        public string? ConnectionType { get; set; }

        // ============================================================
        // PHASE 2 PROPERTIES (Future)
        // ============================================================
        // TODO Phase 2: Add these properties
        // - public string? IPAddress { get; set; }
        // - public int? Port { get; set; }
        // - public string? SerialPort { get; set; } // e.g., "COM3"
        // - public string? Settings { get; set; } // JSON configuration

        // ============================================================
        // PHASE 3 PROPERTIES (Future)
        // ============================================================
        // TODO Phase 3: Add these properties
        // - public string? Status { get; set; } // Online, Offline, Error
        // - public DateTime? LastStatusCheck { get; set; }
        // - public string? LastError { get; set; }
        // - public bool AutoReconnect { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================

        /// <summary>
        /// Navigation property to Station
        /// </summary>
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }
    }
}
