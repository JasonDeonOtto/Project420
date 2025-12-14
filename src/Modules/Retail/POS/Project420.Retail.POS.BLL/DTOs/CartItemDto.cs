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
        /// Serial number for unique item tracking (Cannabis Act requirement)
        /// </summary>
        /// <remarks>
        /// Phase 9.1: Serial numbers enable unit-level traceability.
        /// Populated when scanning serialized items (pre-rolls, packaged flower, etc.)
        /// </remarks>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Whether this item was added via serial number scan (unique item)
        /// </summary>
        public bool IsSerializedItem { get; set; }

        /// <summary>
        /// Cost price (for profit margin calculation - not shown to customer)
        /// </summary>
        public decimal CostPrice { get; set; }

        // ========================================
        // DISCOUNT FIELDS (Phase 9.2)
        // ========================================

        /// <summary>
        /// Discount amount applied to this line item (in Rands).
        /// Applied before VAT recalculation for SA compliance.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Discount percentage applied (for display purposes).
        /// If set, DiscountAmount is calculated as: (UnitPriceInclVAT * Quantity) * (DiscountPercentage / 100)
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Reason for discount (e.g., "Staff discount", "Promotion", "Manager override")
        /// Required for audit trail.
        /// </summary>
        public string? DiscountReason { get; set; }

        // ========================================
        // CALCULATED FIELDS (populated by service)
        // ========================================

        /// <summary>
        /// Original line total before any discount (Quantity * UnitPriceInclVAT)
        /// </summary>
        public decimal OriginalLineTotal => Quantity * UnitPriceInclVAT;

        /// <summary>
        /// Line subtotal (excluding VAT) after discount - calculated
        /// Formula: (OriginalLineTotal - DiscountAmount) / 1.15
        /// </summary>
        public decimal LineSubtotal { get; set; }

        /// <summary>
        /// Line VAT amount after discount - calculated
        /// Formula: (OriginalLineTotal - DiscountAmount) - LineSubtotal
        /// </summary>
        public decimal LineVATAmount { get; set; }

        /// <summary>
        /// Line total (including VAT) after discount - calculated
        /// Formula: OriginalLineTotal - DiscountAmount
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
