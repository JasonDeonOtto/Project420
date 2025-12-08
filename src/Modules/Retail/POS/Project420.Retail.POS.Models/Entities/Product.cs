using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a cannabis product available for sale in the POS system
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance Requirements (SA Cannabis for Private Purposes Act 2024):
    /// - All cannabis products must have THC/CBD percentages labeled
    /// - Batch/lot tracking required for seed-to-sale traceability
    /// - Lab testing documentation must be maintained
    /// - Strain information must be tracked for medical cannabis (Section 21)
    ///
    /// POPIA Compliance:
    /// - Inherits from AuditableEntity for full audit trail
    /// - All product changes tracked (who, what, when)
    /// </remarks>
    public class Product : AuditableEntity
    {
        // ============================================================
        // BASIC INFORMATION
        // ============================================================

        /// <summary>
        /// Stock Keeping Unit - Unique product identifier/barcode
        /// </summary>
        /// <remarks>
        /// Best Practice: SKU should be unique across the system
        /// Example formats: "CBD-OIL-001", "FLOWER-IND-002", "EDIBLE-GUMMY-003"
        /// </remarks>
        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name/title
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed product description
        /// </summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this product is currently active and available for sale
        /// </summary>
        /// <remarks>
        /// Use this instead of deleting products to maintain referential integrity
        /// Inactive products won't show in POS but remain in transaction history
        /// </remarks>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // CANNABIS COMPLIANCE FIELDS (SA Cannabis Act 2024)
        // ============================================================

        /// <summary>
        /// THC (Tetrahydrocannabinol) percentage or content
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: REQUIRED on all product labels
        /// Format examples: "15%", "150mg", "15-18%"
        /// Used for medical dosing and legal compliance
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "THC Content")]
        public string? THCPercentage { get; set; }

        /// <summary>
        /// CBD (Cannabidiol) percentage or content
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: REQUIRED on all product labels
        /// Format examples: "0.5%", "50mg", "1-2%"
        /// Important for medical cannabis patients (Section 21 permits)
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "CBD Content")]
        public string? CBDPercentage { get; set; }

        /// <summary>
        /// Batch or lot number for seed-to-sale traceability
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: REQUIRED for SAHPRA reporting
        /// Enables product recalls if quality issues discovered
        /// Links product to cultivation/production records
        /// Example: "BATCH-2024-11-001"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Cannabis strain name (e.g., "Blue Dream", "OG Kush", "Charlotte's Web")
        /// </summary>
        /// <remarks>
        /// Important for:
        /// - Customer preference tracking
        /// - Medical cannabis prescriptions (specific strains for conditions)
        /// - Inventory differentiation
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Strain Name")]
        public string? StrainName { get; set; }

        /// <summary>
        /// Date when lab testing was performed on this batch
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Certificate of Analysis (COA) required
        /// COA must test for: potency, pesticides, heavy metals, microbials
        /// SAHPRA requires testing documentation for all medical cannabis
        /// </remarks>
        [Display(Name = "Lab Test Date")]
        public DateTime? LabTestDate { get; set; }

        /// <summary>
        /// Product expiry or best-before date
        /// </summary>
        /// <remarks>
        /// Important for:
        /// - Edibles (food safety regulations)
        /// - Cannabis oils (degradation over time)
        /// - Compliance with consumer protection laws
        /// </remarks>
        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        // ============================================================
        // PRICING
        // ============================================================

        /// <summary>
        /// Default selling price (used if product not on a pricelist)
        /// </summary>
        /// <remarks>
        /// Best Practice: Use Pricelist for flexible pricing
        /// This is the fallback price if no pricelist applies
        /// Includes VAT (15% in South Africa)
        /// </remarks>
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between R0.01 and R999,999.99")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Selling Price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price - what you paid for this product (excluding VAT)
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Profit margin calculations
        /// - Inventory valuation
        /// - Financial reporting
        /// - SARS tax compliance (Cost of Goods Sold)
        /// </remarks>
        [Required]
        [Range(0.00, 999999.99, ErrorMessage = "Cost price must be between R0 and R999,999.99")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost Price")]
        public decimal CostPrice { get; set; }

        // ============================================================
        // INVENTORY/STOCK
        // ============================================================

        /// <summary>
        /// Current quantity in stock (on hand)
        /// </summary>
        /// <remarks>
        /// Updated when:
        /// - Products received (increase)
        /// - Products sold (decrease)
        /// - Products returned (increase)
        /// - Stock adjustments (increase/decrease)
        /// </remarks>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock on hand cannot be negative")]
        [Display(Name = "Stock On Hand")]
        public int StockOnHand { get; set; } = 0;

        /// <summary>
        /// Minimum stock level before reorder alert triggers
        /// </summary>
        /// <remarks>
        /// Used for inventory management:
        /// - Low stock alerts
        /// - Automatic purchase order generation
        /// - Preventing stockouts
        /// </remarks>
        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; } = 0;

        // ============================================================
        // CATEGORY (Future Enhancement)
        // ============================================================

        /// <summary>
        /// Foreign key to Product Category (if/when implemented)
        /// </summary>
        /// <remarks>
        /// Future Enhancement: Link to Categories table
        /// Examples: "Flower", "Edibles", "Oils", "Concentrates", "Accessories"
        /// For now, nullable until Categories entity is created
        /// </remarks>
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        // Note: Add this navigation property when Category entity is created
        // public Category? Category { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Collection of transaction details (line items) that reference this product
        /// </summary>
        /// <remarks>
        /// EF Core will automatically populate this collection
        /// Shows all sales/transactions where this product was sold
        /// Used for sales history, reporting, analytics
        /// </remarks>
        public virtual ICollection<POSTransactionDetail> TransactionDetails { get; set; } = new List<POSTransactionDetail>();
    }
}

