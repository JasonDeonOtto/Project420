using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service interface for product barcode operations including POS scanning and serial number management.
/// </summary>
/// <remarks>
/// SAHPRA Compliance:
/// - Supports seed-to-sale traceability via serial numbers
/// - Enables batch recall queries
/// - Validates serial number availability before sale
///
/// POS Integration:
/// - Barcode scanning at checkout (returns product with pricing)
/// - Duplicate sale prevention for unique items
/// - Integration with TransactionService for sale recording
/// </remarks>
public interface IProductBarcodeService
{
    // ========================================
    // BARCODE SCANNING (POS Primary Use Case)
    // ========================================

    /// <summary>
    /// Scan a barcode and return product information for checkout.
    /// Primary method for POS barcode scanning.
    /// </summary>
    /// <param name="barcodeValue">The scanned barcode string</param>
    /// <returns>Barcode scan result with product info and validation</returns>
    Task<BarcodeScanResultDto> ScanBarcodeAsync(string barcodeValue);

    /// <summary>
    /// Validate a serial number is available for sale.
    /// Called before adding unique item to cart.
    /// </summary>
    /// <param name="serialNumber">Serial number to validate</param>
    /// <returns>Validation result with error message if unavailable</returns>
    Task<SerialNumberValidationResultDto> ValidateSerialNumberAsync(string serialNumber);

    // ========================================
    // CART ITEM CREATION
    // ========================================

    /// <summary>
    /// Create a cart item from a scanned barcode.
    /// Populates CartItemDto with product details, pricing, and traceability info.
    /// </summary>
    /// <param name="barcodeValue">The scanned barcode string</param>
    /// <param name="pricelistId">Optional pricelist ID for custom pricing</param>
    /// <returns>CartItemDto ready for checkout</returns>
    Task<CartItemDto?> CreateCartItemFromBarcodeAsync(string barcodeValue, int? pricelistId = null);

    // ========================================
    // SERIAL NUMBER LIFECYCLE
    // ========================================

    /// <summary>
    /// Mark a serial number as sold after successful checkout.
    /// Called by TransactionService after payment confirmed.
    /// </summary>
    /// <param name="productBarcodeId">ProductBarcode ID</param>
    /// <param name="transactionId">Transaction ID where sold</param>
    /// <param name="soldDate">Date/time of sale</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkSerialNumberAsSoldAsync(int productBarcodeId, int transactionId, DateTime soldDate);

    /// <summary>
    /// Unmark a serial number as sold (for refund/void).
    /// Called by TransactionService during refund processing.
    /// </summary>
    /// <param name="productBarcodeId">ProductBarcode ID</param>
    /// <returns>True if successful</returns>
    Task<bool> UnmarkSerialNumberAsSoldAsync(int productBarcodeId);

    // ========================================
    // BARCODE MANAGEMENT
    // ========================================

    /// <summary>
    /// Register a new barcode for a product.
    /// </summary>
    /// <param name="dto">Barcode registration data</param>
    /// <returns>Created ProductBarcode ID</returns>
    Task<int> RegisterBarcodeAsync(RegisterBarcodeDto dto);

    /// <summary>
    /// Register multiple serial numbers for a batch (bulk insert).
    /// Used when receiving inventory with individual serial numbers.
    /// </summary>
    /// <param name="dto">Bulk serial number registration data</param>
    /// <returns>Number of serial numbers registered</returns>
    Task<int> RegisterBatchSerialNumbersAsync(RegisterBatchSerialNumbersDto dto);

    /// <summary>
    /// Deactivate a barcode (soft delete).
    /// </summary>
    /// <param name="productBarcodeId">ProductBarcode ID to deactivate</param>
    /// <returns>True if successful</returns>
    Task<bool> DeactivateBarcodeAsync(int productBarcodeId);

    // ========================================
    // QUERIES
    // ========================================

    /// <summary>
    /// Get all barcodes for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of barcodes for the product</returns>
    Task<List<ProductBarcodeDto>> GetBarcodesByProductIdAsync(int productId);

    /// <summary>
    /// Get all serial numbers for a batch (traceability query).
    /// </summary>
    /// <param name="batchNumber">Batch number</param>
    /// <returns>List of serial numbers in the batch</returns>
    Task<List<ProductBarcodeDto>> GetSerialNumbersByBatchAsync(string batchNumber);

    /// <summary>
    /// Get available (unsold) serial numbers for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of available serial numbers</returns>
    Task<List<ProductBarcodeDto>> GetAvailableSerialNumbersAsync(int productId);

    /// <summary>
    /// Get sold serial numbers for a product within date range (audit query).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start of date range</param>
    /// <param name="endDate">End of date range</param>
    /// <returns>List of sold serial numbers with sale details</returns>
    Task<List<ProductBarcodeDto>> GetSoldSerialNumbersAsync(int productId, DateTime startDate, DateTime endDate);
}
