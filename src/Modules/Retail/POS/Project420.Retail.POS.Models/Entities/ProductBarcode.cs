using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a barcode associated with a product
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance Requirements:
    /// - Support both standard product barcodes (EAN/UPC) and unique serial numbers
    /// - Serial numbers enable seed-to-sale traceability (SAHPRA requirement)
    /// - Batch tracking for lab test results and recalls
    /// - Individual item tracking for high-value cannabis products
    ///
    /// Use Cases:
    /// 1. Standard Product Barcode (IsUnique = false):
    ///    - One barcode for all units of a product type
    ///    - Example: All "Blue Dream 3.5g" share barcode "6001234567890"
    ///    - Stock decrements on sale
    ///
    /// 2. Unique Serial Number (IsUnique = true):
    ///    - Each individual item has its own barcode/serial number
    ///    - Example: "BD-2024-001-00042" for specific batch item #42
    ///    - Prevents duplicate sales (item marked as sold)
    ///    - Required for high-value products or strict compliance
    ///
    /// POPIA Compliance:
    /// - Inherits from AuditableEntity for full audit trail
    /// - Tracks who created/modified barcode records
    /// </remarks>
    public class ProductBarcode : AuditableEntity
    {
        // ============================================================
        // PRIMARY KEY
        // ============================================================

        /// <summary>
        /// Primary key identifier
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // PRODUCT RELATIONSHIP
        // ============================================================

        /// <summary>
        /// Foreign key to Product table
        /// </summary>
        /// <remarks>
        /// Links this barcode to a specific product.
        /// A product can have multiple barcodes (primary EAN + alternate formats).
        /// </remarks>
        [Required(ErrorMessage = "Product ID is required")]
        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property to the associated Product
        /// </summary>
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        // ============================================================
        // BARCODE INFORMATION
        // ============================================================

        /// <summary>
        /// The actual barcode value that gets scanned
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - EAN13: "6001234567890"
        /// - UPC: "012345678905"
        /// - Serial: "BD-2024-001-00042"
        /// - QR: "{\"batch\":\"BD-2024-001\",\"item\":42,\"thc\":18.5}"
        ///
        /// Best Practice: Store as string to support all barcode formats
        /// (numeric barcodes can have leading zeros)
        /// </remarks>
        [Required(ErrorMessage = "Barcode value is required")]
        [MaxLength(100, ErrorMessage = "Barcode value cannot exceed 100 characters")]
        [Display(Name = "Barcode Value")]
        public string BarcodeValue { get; set; } = string.Empty;

        /// <summary>
        /// Type/format of the barcode
        /// </summary>
        /// <remarks>
        /// Determines how the barcode should be encoded/decoded.
        /// See BarcodeType enum for supported formats.
        /// </remarks>
        [Required(ErrorMessage = "Barcode type is required")]
        [Display(Name = "Barcode Type")]
        public BarcodeType BarcodeType { get; set; }

        /// <summary>
        /// Indicates if this barcode represents a unique item (serial number)
        /// </summary>
        /// <remarks>
        /// - False: Standard product barcode (multiple items share this barcode)
        /// - True: Unique serial number (one barcode per individual item)
        ///
        /// If true, the following fields must be populated:
        /// - SerialNumber (unique identifier)
        /// - BatchNumber (manufacturing batch)
        /// - IsSold (track if this specific item has been sold)
        /// </remarks>
        [Display(Name = "Is Unique Item")]
        public bool IsUnique { get; set; } = false;

        // ============================================================
        // SERIAL NUMBER TRACKING (for IsUnique = true)
        // ============================================================

        /// <summary>
        /// Unique serial number for individual item tracking
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = true.
        /// Example formats:
        /// - "00042" (simple sequential)
        /// - "BD-2024-001-00042" (batch-year-item)
        /// - "2024-12-05-0042" (date-based)
        ///
        /// Cannabis Compliance: Serial numbers enable:
        /// - Seed-to-sale traceability
        /// - Batch recall identification
        /// - Individual item expiry tracking
        /// </remarks>
        [MaxLength(50, ErrorMessage = "Serial number cannot exceed 50 characters")]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Manufacturing batch number
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = true.
        /// Links to lab test results and quality control.
        /// Critical for cannabis compliance (batch recalls, potency verification).
        /// </remarks>
        [MaxLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Indicates if this specific item has been sold
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = true.
        /// Prevents duplicate sales of the same serial number.
        /// If true, scanning this barcode again should trigger a warning.
        /// </remarks>
        [Display(Name = "Is Sold")]
        public bool IsSold { get; set; } = false;

        /// <summary>
        /// Transaction ID where this item was sold
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = true and IsSold = true.
        /// Links to the POSTransactionHeader that sold this specific item.
        /// Enables complete traceability (which customer bought which item when).
        /// </remarks>
        [Display(Name = "Sold In Transaction ID")]
        public int? SoldInTransactionId { get; set; }

        /// <summary>
        /// Date/time when this item was sold
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = true and IsSold = true.
        /// Automatically populated when item is marked as sold.
        /// </remarks>
        [Display(Name = "Sold Date")]
        public DateTime? SoldDate { get; set; }

        // ============================================================
        // STANDARD BARCODE SETTINGS (for IsUnique = false)
        // ============================================================

        /// <summary>
        /// Indicates if this barcode is currently active and can be used
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = false.
        /// Allows temporary deactivation of barcodes without deletion.
        /// Example: Old packaging barcode being phased out.
        /// </remarks>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indicates if this is the primary/default barcode for the product
        /// </summary>
        /// <remarks>
        /// Only used when IsUnique = false.
        /// A product can have multiple barcodes (different formats, old packaging),
        /// but only one should be marked as primary.
        /// Primary barcode is used in reports, labels, etc.
        /// </remarks>
        [Display(Name = "Is Primary Barcode")]
        public bool IsPrimaryBarcode { get; set; } = false;
    }
}
