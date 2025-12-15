using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs
{
    /// <summary>
    /// Represents a single item in the shopping cart
    /// </summary>
    public class CartItemDto
    {
        /// <summary>
        /// Product ID being purchased
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product SKU (for display and audit)
        /// </summary>
        public string ProductSku { get; set; } = string.Empty;

        /// <summary>
        /// Product name (denormalized for historical accuracy)
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Unit price including VAT (as displayed to customer)
        /// </summary>
        public decimal UnitPriceInclVAT { get; set; }

        /// <summary>
        /// Quantity being purchased
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Batch/lot number for seed-to-sale traceability (Cannabis Act requirement)
        /// </summary>
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Cost price (for profit margin calculation - not shown to customer)
        /// </summary>
        public decimal CostPrice { get; set; }

        // ========================================
        // BARCODE/SERIAL NUMBER TRACKING (Phase 8.4)
        // ========================================

        /// <summary>
        /// Serial number for unique item tracking (SAHPRA compliance)
        /// </summary>
        /// <remarks>
        /// Populated when scanning a unique serial number barcode.
        /// Enables individual unit traceability for recalls and compliance.
        /// </remarks>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Raw barcode value that was scanned
        /// </summary>
        public string? BarcodeValue { get; set; }

        /// <summary>
        /// Type of barcode scanned (EAN13, Serial, QR, etc.)
        /// </summary>
        public BarcodeType? BarcodeType { get; set; }

        /// <summary>
        /// Indicates if this is a unique serialized item vs standard product
        /// </summary>
        public bool IsUniqueItem { get; set; }

        /// <summary>
        /// ProductBarcode ID for duplicate prevention (unique items only)
        /// </summary>
        public int? ProductBarcodeId { get; set; }

        /// <summary>
        /// Weight in grams (for cannabis products - SAHPRA compliance)
        /// </summary>
        public decimal? WeightGrams { get; set; }

        // ========================================
        // CALCULATED FIELDS (populated by service)
        // ========================================

        /// <summary>
        /// Line subtotal (excluding VAT) - calculated
        /// </summary>
        public decimal LineSubtotal { get; set; }

        /// <summary>
        /// Line VAT amount - calculated
        /// </summary>
        public decimal LineVATAmount { get; set; }

        /// <summary>
        /// Line total (including VAT) - calculated
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
