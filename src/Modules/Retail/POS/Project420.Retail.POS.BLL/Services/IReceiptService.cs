using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service for generating compliant receipts for retail transactions
/// Phase 9.8: Compliant Receipt Generation
/// </summary>
/// <remarks>
/// Generates receipts that comply with:
/// - SARS requirements (VAT invoicing)
/// - SAHPRA regulations (cannabis sales tracking)
/// - DALRRD requirements (hemp product traceability)
/// - Cannabis for Private Purposes Act 2024
/// </remarks>
public interface IReceiptService
{
    /// <summary>
    /// Generates a compliant receipt for a completed transaction
    /// </summary>
    /// <param name="transactionId">The transaction ID</param>
    /// <param name="isReprint">Whether this is a reprint (adds "DUPLICATE" watermark)</param>
    /// <returns>Complete receipt data</returns>
    Task<ReceiptDto> GenerateReceiptAsync(int transactionId, bool isReprint = false);

    /// <summary>
    /// Generates a compliant receipt from checkout result
    /// </summary>
    /// <param name="checkoutResult">The checkout result</param>
    /// <param name="businessSettings">Optional business settings override</param>
    /// <returns>Complete receipt data</returns>
    ReceiptDto GenerateReceiptFromCheckout(CheckoutResultDto checkoutResult, BusinessReceiptSettings? businessSettings = null);

    /// <summary>
    /// Generates receipt as formatted text (for thermal printing)
    /// </summary>
    /// <param name="receipt">The receipt data</param>
    /// <returns>Formatted text receipt</returns>
    string FormatReceiptAsText(ReceiptDto receipt);

    /// <summary>
    /// Generates receipt as HTML (for display/PDF generation)
    /// </summary>
    /// <param name="receipt">The receipt data</param>
    /// <returns>HTML receipt</returns>
    string FormatReceiptAsHtml(ReceiptDto receipt);

    /// <summary>
    /// Gets default business settings for receipts
    /// </summary>
    /// <returns>Business receipt settings</returns>
    BusinessReceiptSettings GetDefaultBusinessSettings();

    /// <summary>
    /// Gets standard legal disclaimers for cannabis receipts
    /// </summary>
    /// <returns>List of legal disclaimer text</returns>
    List<string> GetCannabisLegalDisclaimers();

    /// <summary>
    /// Gets compliance notices for a transaction
    /// </summary>
    /// <param name="ageVerified">Whether age was verified</param>
    /// <param name="maskedIdNumber">Masked customer ID</param>
    /// <returns>List of compliance notices</returns>
    List<string> GetComplianceNotices(bool ageVerified, string? maskedIdNumber);
}

/// <summary>
/// Business settings for receipt generation
/// </summary>
public class BusinessReceiptSettings
{
    /// <summary>
    /// Business/store name
    /// </summary>
    public string BusinessName { get; set; } = "Green Leaf Dispensary";

    /// <summary>
    /// Business address line 1
    /// </summary>
    public string AddressLine1 { get; set; } = "123 Main Street";

    /// <summary>
    /// Business address line 2 (city, postal code)
    /// </summary>
    public string AddressLine2 { get; set; } = "Johannesburg, 2000";

    /// <summary>
    /// Full address (formatted)
    /// </summary>
    public string FullAddress => $"{AddressLine1}, {AddressLine2}";

    /// <summary>
    /// Business phone number
    /// </summary>
    public string PhoneNumber { get; set; } = "011 123 4567";

    /// <summary>
    /// SARS VAT registration number
    /// </summary>
    public string VATNumber { get; set; } = "VAT-4123456789";

    /// <summary>
    /// SAHPRA cannabis license number
    /// </summary>
    public string? SAHPRALicense { get; set; } = "SL-2024-001";

    /// <summary>
    /// DALRRD permit number
    /// </summary>
    public string? DALRRDPermit { get; set; } = "DP-2024-JHB-001";

    /// <summary>
    /// Default terminal ID
    /// </summary>
    public string TerminalId { get; set; } = "POS-001";

    /// <summary>
    /// Receipt footer message
    /// </summary>
    public string FooterMessage { get; set; } = "Thank you for your purchase!\nPlease consume responsibly and legally.";

    /// <summary>
    /// Receipt width in characters (for text formatting)
    /// </summary>
    public int ReceiptWidth { get; set; } = 48;
}
