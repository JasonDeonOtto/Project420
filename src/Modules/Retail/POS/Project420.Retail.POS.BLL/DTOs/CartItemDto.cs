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
