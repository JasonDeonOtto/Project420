using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.DAL.Repositories;

/// <summary>
/// Repository interface for product barcode lookup and serial number tracking.
/// </summary>
/// <remarks>
/// SAHPRA Compliance:
/// - Supports seed-to-sale traceability via serial numbers
/// - Enables batch recall queries
/// - Tracks individual item lifecycle (created → sold → returned)
///
/// POS Integration:
/// - Barcode scanning at checkout
/// - Duplicate sale prevention for unique items
/// - Batch/serial number recording on transactions
/// </remarks>
public interface IProductBarcodeRepository
{
    // ========================================
    // BARCODE LOOKUP (POS Scanning)
    // ========================================

    /// <summary>
    /// Lookup a barcode by its scanned value.
    /// Primary method for POS barcode scanning.
    /// </summary>
    /// <param name="barcodeValue">The scanned barcode string</param>
    /// <returns>ProductBarcode with Product navigation if found</returns>
    Task<ProductBarcode?> GetByBarcodeValueAsync(string barcodeValue);

    /// <summary>
    /// Lookup by serial number (alternative to barcode value).
    /// </summary>
    /// <param name="serialNumber">Unique serial number</param>
    /// <returns>ProductBarcode with Product navigation if found</returns>
    Task<ProductBarcode?> GetBySerialNumberAsync(string serialNumber);

    /// <summary>
    /// Get primary barcode for a product (for label printing, etc.).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary ProductBarcode if exists</returns>
    Task<ProductBarcode?> GetPrimaryBarcodeAsync(int productId);

    /// <summary>
    /// Get all barcodes for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of all barcodes (standard and serial)</returns>
    Task<List<ProductBarcode>> GetByProductIdAsync(int productId);

    // ========================================
    // AVAILABILITY CHECKS (Duplicate Prevention)
    // ========================================

    /// <summary>
    /// Check if a serial number is available for sale.
    /// Returns false if already sold or doesn't exist.
    /// </summary>
    /// <param name="serialNumber">Serial number to check</param>
    /// <returns>True if available, false if sold or not found</returns>
    Task<bool> IsSerialNumberAvailableAsync(string serialNumber);

    /// <summary>
    /// Check if a barcode value already exists in the system.
    /// </summary>
    /// <param name="barcodeValue">Barcode value to check</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> BarcodeValueExistsAsync(string barcodeValue);

    // ========================================
    // BATCH OPERATIONS (Traceability)
    // ========================================

    /// <summary>
    /// Get all barcodes/serial numbers for a batch.
    /// Used for batch recall queries and traceability.
    /// </summary>
    /// <param name="batchNumber">Batch number</param>
    /// <returns>List of ProductBarcodes in the batch</returns>
    Task<List<ProductBarcode>> GetByBatchNumberAsync(string batchNumber);

    /// <summary>
    /// Get unsold serial numbers for a product.
    /// Used for inventory checks and available stock.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of unsold unique items</returns>
    Task<List<ProductBarcode>> GetUnsoldSerialNumbersAsync(int productId);

    /// <summary>
    /// Get sold serial numbers for a product (audit/recall).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start of date range</param>
    /// <param name="endDate">End of date range</param>
    /// <returns>List of sold unique items with sale details</returns>
    Task<List<ProductBarcode>> GetSoldSerialNumbersAsync(
        int productId,
        DateTime startDate,
        DateTime endDate);

    // ========================================
    // LIFECYCLE MANAGEMENT
    // ========================================

    /// <summary>
    /// Mark a serial number as sold (after checkout completes).
    /// </summary>
    /// <param name="productBarcodeId">ProductBarcode ID</param>
    /// <param name="transactionId">Transaction ID where sold</param>
    /// <param name="soldDate">Date/time of sale</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkAsSoldAsync(int productBarcodeId, int transactionId, DateTime soldDate);

    /// <summary>
    /// Unmark a serial number as sold (for refund/void).
    /// </summary>
    /// <param name="productBarcodeId">ProductBarcode ID</param>
    /// <returns>True if successful</returns>
    Task<bool> UnmarkAsSoldAsync(int productBarcodeId);

    // ========================================
    // CRUD OPERATIONS
    // ========================================

    /// <summary>
    /// Add a new barcode/serial number.
    /// </summary>
    Task<ProductBarcode> AddAsync(ProductBarcode productBarcode);

    /// <summary>
    /// Add multiple barcodes/serial numbers (bulk insert).
    /// </summary>
    Task<int> AddRangeAsync(IEnumerable<ProductBarcode> productBarcodes);

    /// <summary>
    /// Update a barcode record.
    /// </summary>
    Task UpdateAsync(ProductBarcode productBarcode);

    /// <summary>
    /// Deactivate a barcode (soft delete).
    /// </summary>
    Task<bool> DeactivateAsync(int productBarcodeId);
}
