using System.ComponentModel;

namespace Project420.Shared.Core.Enums;

/// <summary>
/// Represents the type/format of a barcode
/// </summary>
/// <remarks>
/// Cannabis retail requires dual barcode support:
/// 1. Standard EAN/UPC for product types (e.g., "Blue Dream 3.5g")
/// 2. Unique Serial Numbers for individual item tracking (batch compliance)
///
/// Compliance:
/// - Serial numbers enable seed-to-sale traceability
/// - Batch tracking for lab test results and recalls
/// - Expiry date tracking per individual item
/// </remarks>
public enum BarcodeType
{
    /// <summary>
    /// EAN-13 barcode (13 digits) - Standard retail barcode
    /// </summary>
    /// <remarks>
    /// Most common retail barcode format worldwide
    /// Example: 6001234567890
    /// </remarks>
    [Description("EAN-13")]
    EAN13 = 1,

    /// <summary>
    /// UPC barcode (12 digits) - Universal Product Code
    /// </summary>
    /// <remarks>
    /// Common in North America
    /// Example: 012345678905
    /// </remarks>
    [Description("UPC")]
    UPC = 2,

    /// <summary>
    /// EAN-8 barcode (8 digits) - Short EAN for small products
    /// </summary>
    /// <remarks>
    /// Used for small product packaging
    /// Example: 12345670
    /// </remarks>
    [Description("EAN-8")]
    EAN8 = 3,

    /// <summary>
    /// Code 128 barcode - Alphanumeric barcode (variable length)
    /// </summary>
    /// <remarks>
    /// Supports letters, numbers, and symbols
    /// Good for custom serial numbers
    /// Example: BD-2024-001-00042
    /// </remarks>
    [Description("Code 128")]
    Code128 = 4,

    /// <summary>
    /// QR Code - 2D barcode (can store more data)
    /// </summary>
    /// <remarks>
    /// Can store URLs, JSON data, batch info, etc.
    /// Good for cannabis compliance (batch, lab results, THC/CBD)
    /// </remarks>
    [Description("QR Code")]
    QRCode = 5,

    /// <summary>
    /// Data Matrix - 2D barcode (compact, high density)
    /// </summary>
    /// <remarks>
    /// Good for small labels
    /// Common in pharmaceutical packaging
    /// </remarks>
    [Description("Data Matrix")]
    DataMatrix = 6,

    /// <summary>
    /// Serial Number - Custom format unique identifier
    /// </summary>
    /// <remarks>
    /// Used for individual item tracking
    /// Format determined by business (e.g., "BATCH-YEAR-ITEM")
    /// Critical for cannabis seed-to-sale tracking
    /// </remarks>
    [Description("Serial Number")]
    Serial = 7
}
