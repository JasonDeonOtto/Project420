namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Represents the breakdown of a financial amount into VAT-exclusive, VAT, and VAT-inclusive components.
/// Used across all transaction types (POS Sales, GRVs, Invoices, Credits, Returns).
/// </summary>
/// <remarks>
/// South African VAT Context:
/// - Standard VAT Rate: 15%
/// - Retail prices are VAT-inclusive by default
/// - VAT must be extracted for SARS reporting (VAT201 returns)
///
/// Example Breakdown:
/// - Total (VAT-incl): R10.00
/// - Subtotal (VAT-excl): R8.70
/// - TaxAmount (VAT): R1.30
/// </remarks>
public class VATBreakdown
{
    /// <summary>
    /// Gets or sets the subtotal amount excluding VAT.
    /// </summary>
    /// <example>
    /// For a R10.00 VAT-inclusive price, Subtotal = R8.70
    /// </example>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the VAT (tax) portion of the amount.
    /// </summary>
    /// <example>
    /// For a R10.00 VAT-inclusive price at 15% VAT, TaxAmount = R1.30
    /// </example>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the total amount including VAT.
    /// </summary>
    /// <example>
    /// Subtotal (R8.70) + TaxAmount (R1.30) = Total (R10.00)
    /// </example>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets any rounding adjustment applied to balance the amounts.
    /// </summary>
    /// <remarks>
    /// When aggregating many line items, small rounding differences may occur.
    /// This field tracks the adjustment made to ensure Total = Subtotal + TaxAmount.
    /// Typically absorbed into TaxAmount for SARS compliance.
    /// </remarks>
    public decimal RoundingAdjustment { get; set; }

    /// <summary>
    /// Validates that the breakdown is mathematically correct.
    /// </summary>
    /// <returns>True if Total = Subtotal + TaxAmount (within 1 cent tolerance), otherwise false.</returns>
    public bool IsValid()
    {
        decimal calculated = Subtotal + TaxAmount;
        decimal variance = Math.Abs(Total - calculated);
        return variance <= 0.01m; // 1 cent tolerance
    }

    /// <summary>
    /// Returns a string representation of the VAT breakdown.
    /// </summary>
    public override string ToString()
    {
        return $"Subtotal: R{Subtotal:F2}, VAT: R{TaxAmount:F2}, Total: R{Total:F2}";
    }
}
