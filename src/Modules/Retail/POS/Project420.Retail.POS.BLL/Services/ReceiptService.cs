using System.Text;
using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service for generating compliant receipts for retail transactions
/// Phase 9.8: Compliant Receipt Generation
/// </summary>
/// <remarks>
/// Implements SARS, SAHPRA, DALRRD, and Cannabis Act compliance requirements:
/// - VAT breakdown (15% SA standard rate)
/// - Batch/serial number traceability
/// - Age verification notices
/// - Legal disclaimers
/// - Masked customer ID display
/// </remarks>
public class ReceiptService : IReceiptService
{
    private const decimal VAT_RATE = 0.15m;
    private const int DEFAULT_RECEIPT_WIDTH = 48;

    private readonly BusinessReceiptSettings _defaultSettings;

    public ReceiptService()
    {
        _defaultSettings = new BusinessReceiptSettings();
    }

    /// <inheritdoc/>
    public Task<ReceiptDto> GenerateReceiptAsync(int transactionId, bool isReprint = false)
    {
        // This would typically load from database
        // For now, throw not implemented as we use GenerateReceiptFromCheckout
        throw new NotImplementedException(
            "Database-based receipt generation not yet implemented. " +
            "Use GenerateReceiptFromCheckout for immediate receipt generation.");
    }

    /// <inheritdoc/>
    public ReceiptDto GenerateReceiptFromCheckout(CheckoutResultDto checkoutResult, BusinessReceiptSettings? businessSettings = null)
    {
        var settings = businessSettings ?? _defaultSettings;

        var receipt = new ReceiptDto
        {
            // Business header
            BusinessName = settings.BusinessName,
            BusinessAddress = settings.FullAddress,
            BusinessPhone = settings.PhoneNumber,
            VATRegistrationNumber = settings.VATNumber,
            SAHPRALicenseNumber = settings.SAHPRALicense,
            DALRRDPermitNumber = settings.DALRRDPermit,

            // Transaction header
            ReceiptNumber = checkoutResult.TransactionNumber,
            TransactionNumber = checkoutResult.TransactionNumber,
            TransactionDateTime = checkoutResult.TransactionDate,
            TerminalId = settings.TerminalId,
            CashierName = checkoutResult.ProcessedBy ?? "Cashier",

            // Customer details
            CustomerName = checkoutResult.CustomerName ?? "Walk-In Customer",
            AgeVerified = checkoutResult.AgeVerificationDate.HasValue,
            AgeVerificationMethod = checkoutResult.AgeVerificationDate.HasValue ? "ID Verified" : null,

            // Financial summary
            ItemCount = checkoutResult.ItemCount,
            SubtotalExclVAT = checkoutResult.Subtotal,
            VATAmount = checkoutResult.VATAmount,
            VATRate = 15m,
            DiscountAmount = checkoutResult.DiscountAmount,
            TotalInclVAT = checkoutResult.TotalAmount,

            // Payment details
            AmountTendered = checkoutResult.AmountTendered ?? checkoutResult.TotalAmount,
            ChangeDue = checkoutResult.ChangeDue ?? 0m,

            // Footer
            FooterMessage = settings.FooterMessage,
            GeneratedAt = DateTime.UtcNow,
            IsReprint = false
        };

        // Convert line items with traceability info
        int lineNumber = 1;
        foreach (var item in checkoutResult.LineItems)
        {
            var lineItem = new ReceiptLineItemDto
            {
                LineNumber = lineNumber++,
                ProductSku = item.ProductSku,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitOfMeasure = "unit",
                UnitPriceInclVAT = item.UnitPriceInclVAT,
                UnitPriceExclVAT = Math.Round(item.UnitPriceInclVAT / (1 + VAT_RATE), 2),
                VATAmount = item.LineVATAmount,
                LineTotalInclVAT = item.LineTotal,
                BatchNumber = item.BatchNumber
            };

            receipt.LineItems.Add(lineItem);
        }

        // Add payment
        receipt.Payments.Add(new ReceiptPaymentDto
        {
            PaymentMethod = checkoutResult.PaymentMethod,
            Amount = checkoutResult.TotalAmount
        });

        // Add compliance notices
        receipt.ComplianceNotices = GetComplianceNotices(receipt.AgeVerified, receipt.MaskedCustomerId);

        // Add legal disclaimers
        receipt.LegalDisclaimers = GetCannabisLegalDisclaimers();

        return receipt;
    }

    /// <inheritdoc/>
    public string FormatReceiptAsText(ReceiptDto receipt)
    {
        var sb = new StringBuilder();
        int width = DEFAULT_RECEIPT_WIDTH;

        // Separator line
        string separator = new string('=', width);
        string thinSeparator = new string('-', width);

        // Header
        sb.AppendLine(separator);
        sb.AppendLine(CenterText(receipt.BusinessName.ToUpper(), width));
        sb.AppendLine(CenterText(receipt.BusinessAddress, width));
        sb.AppendLine(CenterText($"Tel: {receipt.BusinessPhone}", width));
        sb.AppendLine(CenterText($"VAT: {receipt.VATRegistrationNumber}", width));

        if (!string.IsNullOrEmpty(receipt.SAHPRALicenseNumber))
            sb.AppendLine(CenterText($"SAHPRA: {receipt.SAHPRALicenseNumber}", width));
        if (!string.IsNullOrEmpty(receipt.DALRRDPermitNumber))
            sb.AppendLine(CenterText($"DALRRD: {receipt.DALRRDPermitNumber}", width));

        sb.AppendLine(separator);

        // Transaction info
        if (receipt.IsReprint)
            sb.AppendLine(CenterText("*** DUPLICATE ***", width));

        sb.AppendLine($"Receipt #: {receipt.ReceiptNumber}");
        sb.AppendLine($"Date: {receipt.TransactionDateTime:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Terminal: {receipt.TerminalId}");
        sb.AppendLine($"Cashier: {receipt.CashierName}");
        sb.AppendLine($"Customer: {receipt.CustomerName}");
        sb.AppendLine(thinSeparator);

        // Line items
        sb.AppendLine("ITEM                        QTY    AMOUNT");
        sb.AppendLine(thinSeparator);

        foreach (var item in receipt.LineItems)
        {
            // Product name (truncate if too long)
            string name = TruncateText(item.ProductName, 26);
            string qty = item.Quantity.ToString("0");
            string amount = $"R{item.LineTotalInclVAT:F2}";

            sb.AppendLine($"{name,-26} {qty,4}  {amount,8}");

            // Show batch/serial number on next line if present
            if (!string.IsNullOrEmpty(item.BatchNumber))
            {
                sb.AppendLine($"  Batch: {item.BatchNumber}");
            }
            if (!string.IsNullOrEmpty(item.SerialNumber))
            {
                sb.AppendLine($"  S/N: {item.SerialNumber}");
            }
        }

        sb.AppendLine(thinSeparator);

        // Totals
        sb.AppendLine(FormatTotalLine("Subtotal (excl VAT):", receipt.SubtotalExclVAT, width));
        sb.AppendLine(FormatTotalLine($"VAT ({receipt.VATRate:F0}%):", receipt.VATAmount, width));

        if (receipt.DiscountAmount > 0)
            sb.AppendLine(FormatTotalLine("Discount:", -receipt.DiscountAmount, width));

        sb.AppendLine(thinSeparator);
        sb.AppendLine(FormatTotalLine("TOTAL:", receipt.TotalInclVAT, width, true));
        sb.AppendLine(thinSeparator);

        // Payment details
        foreach (var payment in receipt.Payments)
        {
            sb.AppendLine(FormatTotalLine($"{payment.PaymentMethod}:", payment.Amount, width));
        }

        if (receipt.ChangeDue > 0)
        {
            sb.AppendLine(FormatTotalLine("CHANGE:", receipt.ChangeDue, width, true));
        }

        sb.AppendLine(separator);

        // Compliance notices
        if (receipt.ComplianceNotices.Any())
        {
            foreach (var notice in receipt.ComplianceNotices)
            {
                sb.AppendLine($"* {notice}");
            }
            sb.AppendLine(thinSeparator);
        }

        // Legal disclaimers
        if (receipt.LegalDisclaimers.Any())
        {
            foreach (var disclaimer in receipt.LegalDisclaimers)
            {
                sb.AppendLine(WrapText(disclaimer, width));
            }
        }

        sb.AppendLine(separator);

        // Footer
        foreach (var line in receipt.FooterMessage.Split('\n'))
        {
            sb.AppendLine(CenterText(line, width));
        }

        sb.AppendLine(separator);

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string FormatReceiptAsHtml(ReceiptDto receipt)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Courier New', monospace; max-width: 400px; margin: 0 auto; padding: 20px; }
            .header { text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 10px; }
            .header h1 { margin: 0; font-size: 18px; }
            .header p { margin: 2px 0; font-size: 12px; }
            .license { font-size: 10px; color: #666; }
            .duplicate { color: red; font-weight: bold; text-align: center; font-size: 14px; }
            .transaction-info { font-size: 12px; margin-bottom: 10px; }
            table { width: 100%; border-collapse: collapse; font-size: 12px; }
            th, td { padding: 4px; text-align: left; }
            .qty { text-align: center; }
            .amount { text-align: right; }
            .batch-info { font-size: 10px; color: #666; padding-left: 20px; }
            .totals { border-top: 1px dashed #000; margin-top: 10px; padding-top: 10px; }
            .totals table { font-size: 13px; }
            .total-row { font-weight: bold; font-size: 14px; }
            .compliance { background: #f0f0f0; padding: 10px; margin: 10px 0; font-size: 11px; }
            .disclaimers { font-size: 10px; color: #666; margin: 10px 0; }
            .footer { text-align: center; border-top: 2px solid #000; padding-top: 10px; margin-top: 10px; }
        ");
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");

        // Header
        sb.AppendLine("<div class='header'>");
        sb.AppendLine($"<h1>{System.Net.WebUtility.HtmlEncode(receipt.BusinessName)}</h1>");
        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(receipt.BusinessAddress)}</p>");
        sb.AppendLine($"<p>Tel: {System.Net.WebUtility.HtmlEncode(receipt.BusinessPhone)}</p>");
        sb.AppendLine($"<p><strong>VAT: {System.Net.WebUtility.HtmlEncode(receipt.VATRegistrationNumber)}</strong></p>");

        if (!string.IsNullOrEmpty(receipt.SAHPRALicenseNumber))
            sb.AppendLine($"<p class='license'>SAHPRA: {System.Net.WebUtility.HtmlEncode(receipt.SAHPRALicenseNumber)}</p>");
        if (!string.IsNullOrEmpty(receipt.DALRRDPermitNumber))
            sb.AppendLine($"<p class='license'>DALRRD: {System.Net.WebUtility.HtmlEncode(receipt.DALRRDPermitNumber)}</p>");

        sb.AppendLine("</div>");

        // Duplicate notice
        if (receipt.IsReprint)
            sb.AppendLine("<p class='duplicate'>*** DUPLICATE ***</p>");

        // Transaction info
        sb.AppendLine("<div class='transaction-info'>");
        sb.AppendLine($"<p><strong>Receipt #:</strong> {System.Net.WebUtility.HtmlEncode(receipt.ReceiptNumber)}</p>");
        sb.AppendLine($"<p><strong>Date:</strong> {receipt.TransactionDateTime:yyyy-MM-dd HH:mm}</p>");
        sb.AppendLine($"<p><strong>Terminal:</strong> {System.Net.WebUtility.HtmlEncode(receipt.TerminalId)} | <strong>Cashier:</strong> {System.Net.WebUtility.HtmlEncode(receipt.CashierName)}</p>");
        sb.AppendLine($"<p><strong>Customer:</strong> {System.Net.WebUtility.HtmlEncode(receipt.CustomerName)}</p>");
        sb.AppendLine("</div>");

        // Line items
        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr><th>Item</th><th class='qty'>Qty</th><th class='amount'>Amount</th></tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (var item in receipt.LineItems)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.ProductName)}</td>");
            sb.AppendLine($"<td class='qty'>{item.Quantity}</td>");
            sb.AppendLine($"<td class='amount'>R {item.LineTotalInclVAT:F2}</td>");
            sb.AppendLine("</tr>");

            if (!string.IsNullOrEmpty(item.BatchNumber) || !string.IsNullOrEmpty(item.SerialNumber))
            {
                sb.AppendLine("<tr><td colspan='3' class='batch-info'>");
                if (!string.IsNullOrEmpty(item.BatchNumber))
                    sb.AppendLine($"Batch: {System.Net.WebUtility.HtmlEncode(item.BatchNumber)}");
                if (!string.IsNullOrEmpty(item.SerialNumber))
                    sb.AppendLine($" | S/N: {System.Net.WebUtility.HtmlEncode(item.SerialNumber)}");
                sb.AppendLine("</td></tr>");
            }
        }

        sb.AppendLine("</tbody></table>");

        // Totals
        sb.AppendLine("<div class='totals'>");
        sb.AppendLine("<table>");
        sb.AppendLine($"<tr><td>Subtotal (excl VAT):</td><td class='amount'>R {receipt.SubtotalExclVAT:F2}</td></tr>");
        sb.AppendLine($"<tr><td>VAT ({receipt.VATRate:F0}%):</td><td class='amount'>R {receipt.VATAmount:F2}</td></tr>");

        if (receipt.DiscountAmount > 0)
            sb.AppendLine($"<tr><td>Discount:</td><td class='amount' style='color:red'>-R {receipt.DiscountAmount:F2}</td></tr>");

        sb.AppendLine($"<tr class='total-row'><td>TOTAL:</td><td class='amount'>R {receipt.TotalInclVAT:F2}</td></tr>");
        sb.AppendLine("</table>");

        // Payments
        sb.AppendLine("<table style='margin-top:10px'>");
        foreach (var payment in receipt.Payments)
        {
            sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(payment.PaymentMethod)}:</td><td class='amount'>R {payment.Amount:F2}</td></tr>");
        }

        if (receipt.ChangeDue > 0)
            sb.AppendLine($"<tr class='total-row'><td>CHANGE:</td><td class='amount'>R {receipt.ChangeDue:F2}</td></tr>");

        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        // Compliance notices
        if (receipt.ComplianceNotices.Any())
        {
            sb.AppendLine("<div class='compliance'>");
            foreach (var notice in receipt.ComplianceNotices)
            {
                sb.AppendLine($"<p>&#10003; {System.Net.WebUtility.HtmlEncode(notice)}</p>");
            }
            sb.AppendLine("</div>");
        }

        // Legal disclaimers
        if (receipt.LegalDisclaimers.Any())
        {
            sb.AppendLine("<div class='disclaimers'>");
            foreach (var disclaimer in receipt.LegalDisclaimers)
            {
                sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(disclaimer)}</p>");
            }
            sb.AppendLine("</div>");
        }

        // Footer
        sb.AppendLine("<div class='footer'>");
        foreach (var line in receipt.FooterMessage.Split('\n'))
        {
            sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(line)}</p>");
        }
        sb.AppendLine("</div>");

        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public BusinessReceiptSettings GetDefaultBusinessSettings()
    {
        return new BusinessReceiptSettings();
    }

    /// <inheritdoc/>
    public List<string> GetCannabisLegalDisclaimers()
    {
        return new List<string>
        {
            "Cannabis products are for adult use only (18+).",
            "Keep out of reach of children.",
            "Do not operate machinery or drive after consumption.",
            "This product is sold in compliance with the Cannabis for Private Purposes Act 2024.",
            "Keep this receipt for traceability records."
        };
    }

    /// <inheritdoc/>
    public List<string> GetComplianceNotices(bool ageVerified, string? maskedIdNumber)
    {
        var notices = new List<string>();

        if (ageVerified)
        {
            if (!string.IsNullOrEmpty(maskedIdNumber))
                notices.Add($"Valid ID verified: {maskedIdNumber}");
            else
                notices.Add("Age verification completed (18+)");
        }

        notices.Add("This purchase counts towards your daily limit");
        notices.Add("Retain this receipt for product traceability");

        return notices;
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text;

        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text;
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }

    private static string FormatTotalLine(string label, decimal amount, int width, bool bold = false)
    {
        string amountStr = $"R{amount:F2}";
        int spacesNeeded = width - label.Length - amountStr.Length;
        if (spacesNeeded < 1) spacesNeeded = 1;

        return label + new string(' ', spacesNeeded) + amountStr;
    }

    private static string WrapText(string text, int maxWidth)
    {
        if (text.Length <= maxWidth)
            return text;

        var sb = new StringBuilder();
        var words = text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                sb.AppendLine(currentLine.ToString().TrimEnd());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(' ');
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            sb.Append(currentLine.ToString());

        return sb.ToString();
    }
}
