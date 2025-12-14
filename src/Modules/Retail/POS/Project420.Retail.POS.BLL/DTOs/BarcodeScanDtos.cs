namespace Project420.Retail.POS.BLL.DTOs
{
    /// <summary>
    /// Result of a barcode or serial number scan
    /// </summary>
    public class BarcodeScanResultDto
    {
        /// <summary>
        /// Whether the scan found a valid product
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if scan failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Warning message (e.g., low stock, near expiry)
        /// </summary>
        public string? WarningMessage { get; set; }

        /// <summary>
        /// The barcode value that was scanned
        /// </summary>
        public string ScannedValue { get; set; } = string.Empty;

        /// <summary>
        /// Type of barcode detected (EAN13, Code128, QR, SerialNumber, SKU)
        /// </summary>
        public string BarcodeType { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a unique/serialized item
        /// </summary>
        public bool IsSerializedItem { get; set; }

        /// <summary>
        /// Cart item ready to add to cart (null if scan failed)
        /// </summary>
        public CartItemDto? CartItem { get; set; }
    }

    /// <summary>
    /// Result of a product search
    /// </summary>
    public class ProductSearchResultDto
    {
        /// <summary>
        /// Total number of matching products
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Matching products (limited to page size)
        /// </summary>
        public List<ProductSearchItemDto> Products { get; set; } = new();
    }

    /// <summary>
    /// Single product in search results
    /// </summary>
    public class ProductSearchItemDto
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product SKU
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Selling price (VAT inclusive)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Current stock on hand
        /// </summary>
        public decimal StockOnHand { get; set; }

        /// <summary>
        /// Primary barcode (EAN-13 or similar)
        /// </summary>
        public string? PrimaryBarcode { get; set; }

        /// <summary>
        /// Cannabis strain name (if applicable)
        /// </summary>
        public string? StrainName { get; set; }

        /// <summary>
        /// THC percentage (cannabis compliance)
        /// </summary>
        public decimal? THCPercentage { get; set; }

        /// <summary>
        /// CBD percentage (cannabis compliance)
        /// </summary>
        public decimal? CBDPercentage { get; set; }

        /// <summary>
        /// Default batch number (latest batch in stock)
        /// </summary>
        public string? DefaultBatchNumber { get; set; }

        /// <summary>
        /// Whether this product requires serial number scanning
        /// </summary>
        public bool RequiresSerialNumber { get; set; }

        /// <summary>
        /// Number of available serial numbers in stock
        /// </summary>
        public int AvailableSerialNumbers { get; set; }
    }

    /// <summary>
    /// Serial number validation result
    /// </summary>
    public class SerialNumberValidationDto
    {
        /// <summary>
        /// Whether the serial number is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if invalid
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Full serial number (30 digits)
        /// </summary>
        public string? FullSerialNumber { get; set; }

        /// <summary>
        /// Short serial number (13 digits for barcodes)
        /// </summary>
        public string? ShortSerialNumber { get; set; }

        /// <summary>
        /// Linked product ID
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Linked batch number
        /// </summary>
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Serial number status (Created, Assigned, Sold, Destroyed)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Whether this SN has already been sold
        /// </summary>
        public bool IsSold { get; set; }

        /// <summary>
        /// Weight in grams (embedded in SN)
        /// </summary>
        public decimal? WeightGrams { get; set; }
    }
}
