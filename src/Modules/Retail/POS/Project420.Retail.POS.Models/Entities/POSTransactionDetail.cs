using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a single line item (product) within a POS transaction
    /// </summary>
    /// <remarks>
    /// RENAMED FROM TransactionDetail → POSTransactionDetail
    ///
    /// Reason for Rename:
    /// - POS transactions are quick counter sales (cash register)
    /// - Future: Regular invoicing (Order → Picking → Dispatching workflow)
    /// - Need to distinguish between POS and Invoicing transaction line items
    ///
    /// This is the "detail" or "line item" record for a POS transaction.
    /// Each POSTransactionDetail represents one product being sold/refunded at the point of sale.
    ///
    /// Header-Detail Pattern:
    /// POSTransactionHeader (1) → POSTransactionDetails (Many)
    ///
    /// Example Transaction:
    /// POSTransactionHeader #12345
    /// ├─ POSTransactionDetail #1: Product "Blue Dream" x2 @ R150 = R300
    /// ├─ POSTransactionDetail #2: Product "OG Kush" x1 @ R155 = R155
    /// └─ POSTransactionDetail #3: Product "CBD Oil" x1 @ R450 = R450
    ///
    /// Important Concepts:
    /// 1. Denormalization: Store product SKU/Name at time of sale
    ///    (what if product name changes later? We want receipt to show original)
    ///
    /// 2. Historical Pricing: Store UnitPrice at time of sale
    ///    (not a link to Product.Price which may change)
    ///
    /// 3. Cannabis Compliance: Each line item must track:
    ///    - What was sold (product, strain, batch)
    ///    - How much (quantity, THC/CBD content)
    ///    - For how much (price, tax)
    ///    - SAHPRA reporting requires product-level detail
    ///
    /// 4. POPIA Compliance:
    ///    - Audit trail via AuditableEntity
    ///    - Cannot delete transaction history (soft delete only)
    /// </remarks>
    public class POSTransactionDetail : AuditableEntity
    {
        // ============================================================
        // FOREIGN KEYS (Relationships)
        // ============================================================

        /// <summary>
        /// Foreign key to parent POSTransactionHeader (invoice/receipt)
        /// </summary>
        [Required]
        [Display(Name = "POS Transaction")]
        public int POSTransactionHeaderId { get; set; }

        /// <summary>
        /// Foreign key to Product being sold/refunded
        /// </summary>
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // ============================================================
        // PRODUCT INFORMATION (Denormalized - snapshot at time of sale)
        // ============================================================

        /// <summary>
        /// Product SKU at time of sale
        /// </summary>
        /// <remarks>
        /// Denormalization: Copy from Product.SKU when sale is made
        /// Why? Product SKU might change, but receipt should show original
        /// Speeds up reporting (don't need to join Product table)
        /// </remarks>
        [Required]
        [MaxLength(50)]
        [Display(Name = "SKU")]
        public string ProductSKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name at time of sale
        /// </summary>
        /// <remarks>
        /// Denormalization: Copy from Product.Name when sale is made
        /// Why? Product name might change, but receipt should show original
        /// Example: "Blue Dream Premium" is renamed to "Premium Blue Dream"
        /// Historical receipts should still show "Blue Dream Premium"
        /// </remarks>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Batch number at time of sale (cannabis compliance)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: SAHPRA requires batch tracking
        /// Copy from Product.BatchNumber at time of sale
        /// Critical for product recalls and quality issues
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        // ============================================================
        // QUANTITY AND PRICING
        // ============================================================

        /// <summary>
        /// Quantity of this product being sold/refunded
        /// </summary>
        /// <remarks>
        /// Positive = Sale (customer buys)
        /// Negative = Refund (customer returns)
        /// Example: Sale qty = 2, Refund qty = -2
        /// </remarks>
        [Required]
        [Range(-999999, 999999)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per unit at time of sale (historical price)
        /// </summary>
        /// <remarks>
        /// CRITICAL: This is NOT Product.Price (which may change)
        /// This is the ACTUAL price charged at time of sale
        ///
        /// Could come from:
        /// - Product.Price (default)
        /// - PricelistItem.Price (if pricelist applied)
        /// - Manager override
        /// - Promotional discount
        ///
        /// VAT-inclusive (South African standard)
        /// </remarks>
        [Required]
        [Range(0.00, 999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount amount per unit (optional)
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Line item discounts
        /// - Promotional pricing
        /// - Manager overrides
        ///
        /// Example: Regular price R150, discount R15, UnitPrice R135
        /// Or store UnitPrice as R135 and LineDiscountAmount as R15 for audit
        /// </remarks>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Line Discount Amount")]
        public decimal LineDiscountAmount { get; set; } = 0.00m;

        // ============================================================
        // CALCULATED AMOUNTS
        // ============================================================

        /// <summary>
        /// Subtotal before tax (Quantity × UnitPrice)
        /// </summary>
        /// <remarks>
        /// Calculated: Quantity * UnitPrice
        /// For VAT-inclusive pricing (South Africa standard):
        /// Subtotal = Total / 1.15
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax amount for this line item (VAT 15% in SA)
        /// </summary>
        /// <remarks>
        /// Calculated: Total - Subtotal
        /// Or: Total - (Total / 1.15)
        ///
        /// SARS Compliance: Must accurately calculate VAT
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total for this line item (Subtotal + Tax)
        /// </summary>
        /// <remarks>
        /// Calculated: Quantity * UnitPrice
        ///
        /// For refunds: This will be negative
        /// Example: Refund 2 items @ R150 = -R300
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        // ============================================================
        // OPTIONAL FIELDS (Future Enhancement)
        // ============================================================

        /// <summary>
        /// Cost price at time of sale (for profit calculations)
        /// </summary>
        /// <remarks>
        /// Future Enhancement: Profit analysis
        /// Copy from Product.CostPrice at time of sale
        /// Used to calculate: Profit = (UnitPrice - CostPrice) * Quantity
        ///
        /// Keep separate from UnitPrice for security
        /// (cashiers shouldn't see cost prices)
        /// </remarks>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost Price")]
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// Notes or special instructions for this line item
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Special requests
        /// - Reasons for discount
        /// - Manager approval notes
        /// - Cannabis-specific notes (e.g., "Medical patient discount")
        /// </remarks>
        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Navigation property to parent POSTransactionHeader
        /// </summary>
        [ForeignKey(nameof(POSTransactionHeaderId))]
        public virtual POSTransactionHeader POSTransactionHeader { get; set; } = null!;

        /// <summary>
        /// Navigation property to Product
        /// </summary>
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
