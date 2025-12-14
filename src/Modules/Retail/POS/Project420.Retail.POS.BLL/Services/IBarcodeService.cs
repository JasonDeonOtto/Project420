using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service for barcode scanning, serial number lookup, and product search
    /// Phase 9.1: Barcode & Serial Number Scanning
    /// </summary>
    public interface IBarcodeService
    {
        /// <summary>
        /// Process a scanned barcode or serial number
        /// Supports: EAN-13, Code128, QR codes, SKU, Full SN (30 digits), Short SN (13 digits)
        /// </summary>
        /// <param name="scannedValue">The raw scanned value</param>
        /// <returns>Scan result with cart item if successful</returns>
        Task<BarcodeScanResultDto> ProcessScanAsync(string scannedValue);

        /// <summary>
        /// Validate a serial number and check if it's available for sale
        /// </summary>
        /// <param name="serialNumber">Full or short serial number</param>
        /// <returns>Validation result with serial number details</returns>
        Task<SerialNumberValidationDto> ValidateSerialNumberAsync(string serialNumber);

        /// <summary>
        /// Search products by name, SKU, or category
        /// Fallback when barcode scan doesn't find a match
        /// </summary>
        /// <param name="searchTerm">Search term (min 2 characters)</param>
        /// <param name="pageSize">Maximum results to return (default 20)</param>
        /// <returns>Matching products</returns>
        Task<ProductSearchResultDto> SearchProductsAsync(string searchTerm, int pageSize = 20);

        /// <summary>
        /// Get product by ID for adding to cart
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Cart item ready to add, or null if not found</returns>
        Task<CartItemDto?> GetProductForCartAsync(int productId);

        /// <summary>
        /// Get available serial numbers for a product
        /// Used when customer wants to select specific unit
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="limit">Maximum serial numbers to return (default 50)</param>
        /// <returns>List of available serial numbers</returns>
        Task<List<SerialNumberValidationDto>> GetAvailableSerialNumbersAsync(int productId, int limit = 50);

        /// <summary>
        /// Determine the type of barcode from its format
        /// </summary>
        /// <param name="value">Barcode value</param>
        /// <returns>Barcode type (EAN13, Code128, QR, SerialNumber, SKU, Unknown)</returns>
        string DetectBarcodeType(string value);

        /// <summary>
        /// Validate EAN-13 check digit
        /// </summary>
        /// <param name="ean13">13-digit EAN barcode</param>
        /// <returns>True if valid check digit</returns>
        bool ValidateEAN13CheckDigit(string ean13);

        /// <summary>
        /// Validate Luhn check digit (used in serial numbers)
        /// </summary>
        /// <param name="value">Value with check digit</param>
        /// <returns>True if valid Luhn check digit</returns>
        bool ValidateLuhnCheckDigit(string value);
    }
}
