using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Result from scanning a barcode at POS.
/// </summary>
/// <remarks>
/// Contains product information, pricing, and validation status.
/// Enables POS to display product details and handle errors gracefully.
/// </remarks>
public class BarcodeScanResultDto
{
    /// <summary>
    /// Indicates if the scan was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if scan failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Scanned barcode value
    /// </summary>
    public string BarcodeValue { get; set; } = string.Empty;

    /// <summary>
    /// Product ID if found
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    public string? ProductSku { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Unit price including VAT
    /// </summary>
    public decimal? UnitPriceInclVAT { get; set; }

    /// <summary>
    /// Cost price (for margin calculation)
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Type of barcode scanned
    /// </summary>
    public BarcodeType? BarcodeType { get; set; }

    /// <summary>
    /// Indicates if this is a unique serial number
    /// </summary>
    public bool IsUniqueItem { get; set; }

    /// <summary>
    /// ProductBarcode ID (for unique items)
    /// </summary>
    public int? ProductBarcodeId { get; set; }

    /// <summary>
    /// Serial number (for unique items)
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Batch number (for unique items)
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Warning message (e.g., item already sold but override allowed)
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Indicates if item is already sold (for unique items)
    /// </summary>
    public bool IsAlreadySold { get; set; }

    /// <summary>
    /// Current stock on hand for the product
    /// </summary>
    public int? StockOnHand { get; set; }
}

/// <summary>
/// Result from validating a serial number availability.
/// </summary>
public class SerialNumberValidationResultDto
{
    /// <summary>
    /// Indicates if the serial number is valid and available for sale
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Indicates if the serial number exists in the system
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Indicates if the serial number is available (not sold)
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// ProductBarcode ID if found
    /// </summary>
    public int? ProductBarcodeId { get; set; }

    /// <summary>
    /// Product ID if found
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Product name if found
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Batch number for traceability
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// If sold, the transaction ID where it was sold
    /// </summary>
    public int? SoldInTransactionId { get; set; }

    /// <summary>
    /// If sold, the date it was sold
    /// </summary>
    public DateTime? SoldDate { get; set; }
}

/// <summary>
/// DTO for registering a new barcode for a product.
/// </summary>
public class RegisterBarcodeDto
{
    /// <summary>
    /// Product ID to associate barcode with
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Barcode value
    /// </summary>
    public string BarcodeValue { get; set; } = string.Empty;

    /// <summary>
    /// Type of barcode
    /// </summary>
    public BarcodeType BarcodeType { get; set; }

    /// <summary>
    /// Is this a unique serial number?
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Serial number (required if IsUnique = true)
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Batch number (required if IsUnique = true)
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Is this the primary barcode for the product?
    /// </summary>
    public bool IsPrimaryBarcode { get; set; }
}

/// <summary>
/// DTO for bulk registering serial numbers for a batch.
/// </summary>
public class RegisterBatchSerialNumbersDto
{
    /// <summary>
    /// Product ID to associate serial numbers with
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Batch number for all serial numbers
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Barcode type for the serial numbers
    /// </summary>
    public BarcodeType BarcodeType { get; set; } = BarcodeType.Serial;

    /// <summary>
    /// List of serial numbers to register
    /// </summary>
    public List<string> SerialNumbers { get; set; } = new();
}

/// <summary>
/// DTO for returning product barcode information.
/// </summary>
public class ProductBarcodeDto
{
    /// <summary>
    /// ProductBarcode ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Barcode value
    /// </summary>
    public string BarcodeValue { get; set; } = string.Empty;

    /// <summary>
    /// Barcode type
    /// </summary>
    public BarcodeType BarcodeType { get; set; }

    /// <summary>
    /// Is this a unique serial number?
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Serial number (for unique items)
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Batch number (for unique items)
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Is this barcode active?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Is this the primary barcode for the product?
    /// </summary>
    public bool IsPrimaryBarcode { get; set; }

    /// <summary>
    /// Is this item sold? (for unique items)
    /// </summary>
    public bool IsSold { get; set; }

    /// <summary>
    /// Transaction ID where sold (for unique items)
    /// </summary>
    public int? SoldInTransactionId { get; set; }

    /// <summary>
    /// Date sold (for unique items)
    /// </summary>
    public DateTime? SoldDate { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Created by user
    /// </summary>
    public string? CreatedBy { get; set; }
}
