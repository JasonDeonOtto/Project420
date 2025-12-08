using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Pickup confirmation with age verification
/// CRITICAL for Cannabis Act compliance
/// </summary>
[Table("pickup_confirmations")]
public class PickupConfirmation : AuditableEntity
{
    /// <summary>
    /// Order ID (foreign key, unique - one confirmation per order)
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    // ===== Pickup Details =====

    /// <summary>
    /// Pickup timestamp
    /// </summary>
    [Required]
    public DateTime PickupDate { get; set; }

    /// <summary>
    /// Customer ID who picked up the order
    /// </summary>
    [Required]
    public int PickedUpByCustomerId { get; set; }

    // ===== Age Verification (Cannabis Act CRITICAL) =====

    /// <summary>
    /// ID verification method used
    /// </summary>
    [Required]
    [MaxLength(50)]
    public PickupVerificationMethod IdVerificationMethod { get; set; }

    /// <summary>
    /// ID number verified at pickup
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string IdNumberVerified { get; set; } = string.Empty;

    /// <summary>
    /// Age confirmed (18+)
    /// </summary>
    [Required]
    public bool AgeConfirmed { get; set; }

    /// <summary>
    /// Staff member ID who verified the pickup
    /// </summary>
    [Required]
    public int VerifiedByStaffId { get; set; }

    // ===== Digital Signature/Photo (Optional) =====

    /// <summary>
    /// Customer signature (digital)
    /// </summary>
    public byte[]? CustomerSignature { get; set; }

    /// <summary>
    /// Photo of ID document (for audit trail)
    /// </summary>
    public byte[]? IdPhoto { get; set; }

    // ===== Notes =====

    /// <summary>
    /// Verification notes (any issues or comments)
    /// </summary>
    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Parent order
    /// </summary>
    public virtual OnlineOrder Order { get; set; } = null!;
}
