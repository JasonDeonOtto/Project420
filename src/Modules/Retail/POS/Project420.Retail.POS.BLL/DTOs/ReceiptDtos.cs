namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Complete receipt data for compliant receipt generation
/// Phase 9.8: Compliant Receipt Generation
/// </summary>
/// <remarks>
/// Includes all SARS, SAHPRA, and DALRRD compliance requirements:
/// - Business details with VAT registration
/// - Transaction reference and date/time
/// - Line items with batch/serial numbers for traceability
/// - VAT breakdown (15% SA standard rate)
/// - Cannabis compliance notices
/// - Legal disclaimers
/// </remarks>
public class ReceiptDto
{
    // ========================================
    // BUSINESS HEADER
    // ========================================

    /// <summary>
    /// Business/store name
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Business address (multiline)
    /// </summary>
    public string BusinessAddress { get; set; } = string.Empty;

    /// <summary>
    /// Business phone number
    /// </summary>
    public string BusinessPhone { get; set; } = string.Empty;

    /// <summary>
    /// SARS VAT registration number (required for VAT invoices)
    /// </summary>
    public string VATRegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// SAHPRA license number for cannabis sales
    /// </summary>
    public string? SAHPRALicenseNumber { get; set; }

    /// <summary>
    /// DALRRD permit number for cannabis/hemp
    /// </summary>
    public string? DALRRDPermitNumber { get; set; }

    // ========================================
    // TRANSACTION HEADER
    // ========================================

    /// <summary>
    /// Receipt/invoice number (unique)
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction reference number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date and time
    /// </summary>
    public DateTime TransactionDateTime { get; set; }

    /// <summary>
    /// POS terminal/station identifier
    /// </summary>
    public string TerminalId { get; set; } = "POS-001";

    /// <summary>
    /// Cashier/operator name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    // ========================================
    // CUSTOMER DETAILS
    // ========================================

    /// <summary>
    /// Customer name (optional)
    /// </summary>
    public string CustomerName { get; set; } = "Walk-In Customer";

    /// <summary>
    /// Masked customer ID for compliance display (e.g., "850101*****86")
    /// </summary>
    public string? MaskedCustomerId { get; set; }

    /// <summary>
    /// Whether age was verified for this transaction
    /// </summary>
    public bool AgeVerified { get; set; }

    /// <summary>
    /// Age verification method used
    /// </summary>
    public string? AgeVerificationMethod { get; set; }

    // ========================================
    // LINE ITEMS
    // ========================================

    /// <summary>
    /// Receipt line items with batch/serial numbers
    /// </summary>
    public List<ReceiptLineItemDto> LineItems { get; set; } = new();

    // ========================================
    // FINANCIAL SUMMARY
    // ========================================

    /// <summary>
    /// Number of items/lines
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Subtotal excluding VAT
    /// </summary>
    public decimal SubtotalExclVAT { get; set; }

    /// <summary>
    /// Total VAT amount (15%)
    /// </summary>
    public decimal VATAmount { get; set; }

    /// <summary>
    /// VAT rate applied (15% for SA)
    /// </summary>
    public decimal VATRate { get; set; } = 15m;

    /// <summary>
    /// Total discount amount applied
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Grand total including VAT
    /// </summary>
    public decimal TotalInclVAT { get; set; }

    // ========================================
    // PAYMENT DETAILS
    // ========================================

    /// <summary>
    /// List of payments (supports multi-tender)
    /// </summary>
    public List<ReceiptPaymentDto> Payments { get; set; } = new();

    /// <summary>
    /// Total amount tendered
    /// </summary>
    public decimal AmountTendered { get; set; }

    /// <summary>
    /// Change due to customer
    /// </summary>
    public decimal ChangeDue { get; set; }

    // ========================================
    // COMPLIANCE & LEGAL
    // ========================================

    /// <summary>
    /// Cannabis-specific legal disclaimers
    /// </summary>
    public List<string> LegalDisclaimers { get; set; } = new();

    /// <summary>
    /// Compliance notices (age verification, daily limits, etc.)
    /// </summary>
    public List<string> ComplianceNotices { get; set; } = new();

    /// <summary>
    /// Footer message (thank you, consume responsibly, etc.)
    /// </summary>
    public string FooterMessage { get; set; } = string.Empty;

    /// <summary>
    /// Receipt generation timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this is a reprint
    /// </summary>
    public bool IsReprint { get; set; }
}

/// <summary>
/// Receipt line item with full traceability information
/// </summary>
public class ReceiptLineItemDto
{
    /// <summary>
    /// Line number (1-based)
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Product SKU/code
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name/description
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity sold
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of measure (e.g., "g", "unit", "pack")
    /// </summary>
    public string UnitOfMeasure { get; set; } = "unit";

    /// <summary>
    /// Unit price including VAT
    /// </summary>
    public decimal UnitPriceInclVAT { get; set; }

    /// <summary>
    /// Unit price excluding VAT
    /// </summary>
    public decimal UnitPriceExclVAT { get; set; }

    /// <summary>
    /// Line discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Line VAT amount
    /// </summary>
    public decimal VATAmount { get; set; }

    /// <summary>
    /// Line total including VAT
    /// </summary>
    public decimal LineTotalInclVAT { get; set; }

    // ========================================
    // TRACEABILITY (SEED-TO-SALE)
    // ========================================

    /// <summary>
    /// Batch number for traceability (Cannabis Act requirement)
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Serial number (for serialized items like pre-rolls)
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Weight in grams (for weight-based products)
    /// </summary>
    public decimal? WeightGrams { get; set; }

    /// <summary>
    /// THC percentage (if applicable)
    /// </summary>
    public decimal? THCPercentage { get; set; }

    /// <summary>
    /// CBD percentage (if applicable)
    /// </summary>
    public decimal? CBDPercentage { get; set; }

    /// <summary>
    /// Strain name (for cannabis flower)
    /// </summary>
    public string? StrainName { get; set; }

    /// <summary>
    /// Expiry date (for compliance)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Payment method details for receipt
/// </summary>
public class ReceiptPaymentDto
{
    /// <summary>
    /// Payment method (Cash, Card, EFT, Mobile)
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Amount paid with this method
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Card type if card payment (Visa, Mastercard, etc.)
    /// </summary>
    public string? CardType { get; set; }

    /// <summary>
    /// Last 4 digits of card (masked)
    /// </summary>
    public string? CardLastFour { get; set; }

    /// <summary>
    /// Payment reference/authorization code
    /// </summary>
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Request to generate a receipt
/// </summary>
public class GenerateReceiptRequest
{
    /// <summary>
    /// Transaction ID to generate receipt for
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Whether this is a reprint
    /// </summary>
    public bool IsReprint { get; set; }

    /// <summary>
    /// Optional custom footer message
    /// </summary>
    public string? CustomFooterMessage { get; set; }
}
