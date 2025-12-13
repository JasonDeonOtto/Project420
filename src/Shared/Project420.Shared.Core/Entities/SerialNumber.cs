using Project420.Shared.Core.Enums;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Tracks individual serial numbers with full traceability data.
/// Links short serial numbers to their full serial number representations.
/// </summary>
/// <remarks>
/// Purpose:
/// - Maps Short SN (barcode) ↔ Full SN (QR code)
/// - Tracks serial number lifecycle (Created → Assigned → Sold → Destroyed)
/// - Provides traceability queries by batch, strain, date, etc.
///
/// Serial Number Formats:
/// - Full SN (31 digits): Embeds all product info (site, strain, batch, unit, weight)
/// - Short SN (13 digits): Compact format for barcode labels
///
/// Cannabis Compliance:
/// - SAHPRA unit-level traceability
/// - Seed-to-sale audit trail
/// - Recall management capability
/// - Destruction documentation
/// </remarks>
public class SerialNumber : AuditableEntity
{
    /// <summary>
    /// Full serial number (31 digits including check digit).
    /// Contains all embedded product information.
    /// </summary>
    public string FullSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Short serial number (13 digits) for barcode/manual entry.
    /// Maps one-to-one with FullSerialNumber via database.
    /// </summary>
    public string ShortSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Batch number this serial belongs to (16-digit batch number).
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Site ID where the serial was generated.
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Strain code (100-999).
    /// First digit indicates type: 1xx=Sativa, 2xx=Indica, 3xx=Hybrid.
    /// </summary>
    public int StrainCode { get; set; }

    /// <summary>
    /// Batch type (Production, Transfer, etc.).
    /// </summary>
    public BatchType BatchType { get; set; }

    /// <summary>
    /// Production/generation date.
    /// </summary>
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// Batch sequence within site/type/date.
    /// </summary>
    public int BatchSequence { get; set; }

    /// <summary>
    /// Unit sequence within the batch.
    /// </summary>
    public int UnitSequence { get; set; }

    /// <summary>
    /// Weight in grams.
    /// </summary>
    public decimal WeightGrams { get; set; }

    /// <summary>
    /// Pack quantity code (1=single, 2=pair, etc., 0=bulk).
    /// </summary>
    public int PackQty { get; set; }

    /// <summary>
    /// Current lifecycle status of this serial number.
    /// </summary>
    public SerialStatus Status { get; set; } = SerialStatus.Created;

    /// <summary>
    /// Product ID this serial is assigned to (after assignment).
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Product SKU (denormalized for reporting).
    /// </summary>
    public string? ProductSKU { get; set; }

    /// <summary>
    /// Product name (denormalized for reporting).
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Current location ID (site/warehouse).
    /// </summary>
    public int? CurrentLocationId { get; set; }

    /// <summary>
    /// Date when status last changed.
    /// </summary>
    public DateTime? StatusChangedAt { get; set; }

    /// <summary>
    /// User who last changed the status.
    /// </summary>
    public string? StatusChangedBy { get; set; }

    /// <summary>
    /// Transaction ID where this serial was sold (if sold).
    /// </summary>
    public int? SoldTransactionId { get; set; }

    /// <summary>
    /// Date when sold (if sold).
    /// </summary>
    public DateTime? SoldAt { get; set; }

    /// <summary>
    /// Reason for destruction (if destroyed).
    /// SAHPRA requires documented reason.
    /// </summary>
    public string? DestructionReason { get; set; }

    /// <summary>
    /// Destruction witness (if destroyed).
    /// SAHPRA may require witnessed destruction.
    /// </summary>
    public string? DestructionWitness { get; set; }

    /// <summary>
    /// Additional notes or metadata.
    /// </summary>
    public string? Notes { get; set; }
}
