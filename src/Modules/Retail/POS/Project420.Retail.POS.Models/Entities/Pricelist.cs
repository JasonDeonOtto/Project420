using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a pricing strategy or price list for products
    /// </summary>
    /// <remarks>
    /// Pricelist System Purpose:
    /// - Different prices for different customer groups (retail, wholesale, VIP, medical)
    /// - Seasonal or promotional pricing
    /// - Time-based pricing (happy hour, early bird)
    /// - Quantity-based pricing (bulk discounts)
    /// - Location-based pricing (different stores)
    ///
    /// How It Works:
    /// 1. Create Pricelist (e.g., "VIP Customer Pricing")
    /// 2. Add products to pricelist with specific prices (via PricelistItem entity)
    /// 3. Assign pricelist to customers or transactions
    /// 4. POS system checks pricelist at sale time
    ///
    /// Hierarchy:
    /// Pricelist → PricelistItem → Product (many-to-many relationship)
    /// Customer can have DefaultPricelistId (future enhancement)
    ///
    /// Example Pricelists:
    /// - "Standard Retail" (default for walk-in customers)
    /// - "Medical Patient Pricing" (Section 21 permit holders)
    /// - "Wholesale" (bulk buyers)
    /// - "VIP Club Member" (loyalty program)
    /// - "Black Friday 2024" (promotional)
    ///
    /// Cannabis Compliance:
    /// - Medical cannabis may have regulated pricing
    /// - Price changes must be audited for tax compliance
    /// </remarks>
    public class Pricelist : AuditableEntity
    {
        // ============================================================
        // BASIC INFORMATION
        // ============================================================

        /// <summary>
        /// Pricelist name/title
        /// </summary>
        /// <remarks>
        /// Examples: "Standard Retail", "VIP Pricing", "Medical Patient Rates", "December 2024 Special"
        /// </remarks>
        [Required(ErrorMessage = "Pricelist name is required")]
        [MaxLength(200, ErrorMessage = "Pricelist name cannot exceed 200 characters")]
        [Display(Name = "Pricelist Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of this pricelist purpose and rules
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Unique code for this pricelist (optional, for integrations/imports)
        /// </summary>
        /// <remarks>
        /// Example: "PL-STD-001", "PL-VIP-001", "PL-MED-001"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Pricelist Code")]
        public string? Code { get; set; }

        // ============================================================
        // STATUS AND ACTIVATION
        // ============================================================

        /// <summary>
        /// Whether this pricelist is currently active and available for use
        /// </summary>
        /// <remarks>
        /// Inactive pricelists are not available for selection but remain in history
        /// Use for: expired promotions, discontinued pricing strategies
        /// </remarks>
        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is the default pricelist for new customers
        /// </summary>
        /// <remarks>
        /// Best Practice: Only ONE pricelist should be default at a time
        /// Default pricelist is used when:
        /// - Walk-in customer (no account)
        /// - Customer has no assigned pricelist
        /// - No other pricelist applies
        ///
        /// Typically this is your "Standard Retail" pricelist
        /// </remarks>
        [Required]
        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; } = false;

        // ============================================================
        // DATE RANGE (Optional - for time-based pricing)
        // ============================================================

        /// <summary>
        /// Date when this pricelist becomes effective (optional)
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Future price increases (schedule in advance)
        /// - Promotional pricing start date
        /// - Seasonal pricing
        ///
        /// Null = effective immediately
        /// </remarks>
        [Display(Name = "Effective From")]
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Date when this pricelist expires (optional)
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Limited-time promotions
        /// - Seasonal pricing end dates
        /// - Trial pricing periods
        ///
        /// Null = no expiry (permanent)
        /// System should auto-deactivate when expires
        /// </remarks>
        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

        // ============================================================
        // PRICING STRATEGY (Optional - for future enhancement)
        // ============================================================

        /// <summary>
        /// Pricing strategy type (optional future enhancement)
        /// </summary>
        /// <remarks>
        /// Possible values:
        /// - "Fixed" = Specific prices per product
        /// - "Percentage" = Discount/markup percentage from base price
        /// - "Tiered" = Different prices for different quantities
        ///
        /// For POC, start with "Fixed" pricing only
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "Pricing Strategy")]
        public string PricingStrategy { get; set; } = "Fixed";

        /// <summary>
        /// Discount or markup percentage (if using percentage strategy)
        /// </summary>
        /// <remarks>
        /// Positive = markup (increase price by %)
        /// Negative = discount (decrease price by %)
        /// Example: -10.00 = 10% discount, +15.00 = 15% markup
        ///
        /// Only used if PricingStrategy = "Percentage"
        /// For "Fixed" pricing, this is ignored
        /// </remarks>
        [Range(-100, 1000)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Percentage Adjustment")]
        public decimal? PercentageAdjustment { get; set; }

        // ============================================================
        // PRIORITY (For overlapping pricelists)
        // ============================================================

        /// <summary>
        /// Priority level when multiple pricelists could apply
        /// </summary>
        /// <remarks>
        /// Lower number = higher priority
        /// Example: Customer is both VIP and has medical license
        /// - VIP Pricelist: Priority 10
        /// - Medical Pricelist: Priority 5
        /// → Medical Pricelist wins (lower number)
        ///
        /// Default priority: 100
        /// System priority (fallback): 999
        /// </remarks>
        [Required]
        [Range(1, 999)]
        [Display(Name = "Priority")]
        public int Priority { get; set; } = 100;

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of pricelist items (products with their prices in this list)
        /// </summary>
        /// <remarks>
        /// This is the many-to-many join table
        /// Each PricelistItem links a Product to this Pricelist with a specific price
        ///
        /// Example:
        /// Pricelist "VIP" contains:
        /// - PricelistItem: Product "Blue Dream" at R135
        /// - PricelistItem: Product "OG Kush" at R142
        /// - PricelistItem: Product "CBD Oil" at R450
        /// </remarks>
        public virtual ICollection<PricelistItem> PricelistItems { get; set; } = new List<PricelistItem>();

        // Future Enhancement: Link customers to pricelists
        // public virtual ICollection<Debtor> Customers { get; set; } = new List<Debtor>();
    }

    /// <summary>
    /// Join table linking Products to Pricelists with specific prices
    /// </summary>
    /// <remarks>
    /// This creates a many-to-many relationship:
    /// - One Product can be in multiple Pricelists (different prices)
    /// - One Pricelist contains multiple Products
    ///
    /// Example Data:
    /// | PricelistId | ProductId | Price  |
    /// |-------------|-----------|--------|
    /// | 1 (Retail)  | 5 (Blue)  | R150   |
    /// | 2 (VIP)     | 5 (Blue)  | R135   |
    /// | 2 (VIP)     | 7 (OG)    | R142   |
    ///
    /// Cannabis Compliance:
    /// - All price changes audited via AuditableEntity
    /// - Price history maintained for tax compliance
    /// </remarks>
    public class PricelistItem : AuditableEntity
    {
        /// <summary>
        /// Foreign key to Pricelist
        /// </summary>
        [Required]
        [Display(Name = "Pricelist")]
        public int PricelistId { get; set; }

        /// <summary>
        /// Foreign key to Product
        /// </summary>
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        /// <summary>
        /// Price for this product in this pricelist (includes VAT)
        /// </summary>
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between R0.01 and R999,999.99")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Minimum quantity required to get this price (for tiered pricing)
        /// </summary>
        /// <remarks>
        /// Future Enhancement: Quantity-based pricing
        /// Example: "Buy 10+, get this price"
        /// Default: 1 (no minimum)
        /// </remarks>
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum Quantity")]
        public int MinimumQuantity { get; set; } = 1;

        /// <summary>
        /// Maximum quantity this price applies to (for tiered pricing)
        /// </summary>
        /// <remarks>
        /// Future Enhancement: Tiered pricing bands
        /// Example: "Qty 1-9: R150, Qty 10-49: R135, Qty 50+: R120"
        /// Null = no maximum (unlimited)
        /// </remarks>
        [Display(Name = "Maximum Quantity")]
        public int? MaximumQuantity { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================

        /// <summary>
        /// Navigation property to parent Pricelist
        /// </summary>
        [ForeignKey(nameof(PricelistId))]
        public virtual Pricelist Pricelist { get; set; } = null!;

        /// <summary>
        /// Navigation property to Product
        /// </summary>
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}

