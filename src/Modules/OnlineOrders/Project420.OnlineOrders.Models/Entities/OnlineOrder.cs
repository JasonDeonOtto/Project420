using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Online order entity for Click & Collect orders
/// Cannabis Act compliant with age verification
/// </summary>
[Table("online_orders")]
public class OnlineOrder : AuditableEntity
{
    /// <summary>
    /// Unique order number (human-readable)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer account ID
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Order creation date
    /// </summary>
    [Required]
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Current order status
    /// </summary>
    [Required]
    [MaxLength(50)]
    public OnlineOrderStatus Status { get; set; }

    // ===== Order Totals =====

    /// <summary>
    /// Subtotal (before VAT and discounts)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// VAT amount (15% in South Africa)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Discount amount (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total amount to pay (Subtotal + VAT - Discounts)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // ===== Pickup Information =====

    /// <summary>
    /// Pickup location ID (store/branch)
    /// </summary>
    [Required]
    public int PickupLocationId { get; set; }

    /// <summary>
    /// Customer's preferred pickup date
    /// </summary>
    public DateTime? PreferredPickupDate { get; set; }

    /// <summary>
    /// Customer's preferred pickup time
    /// </summary>
    public TimeSpan? PreferredPickupTime { get; set; }

    /// <summary>
    /// Actual pickup timestamp
    /// </summary>
    public DateTime? ActualPickupDate { get; set; }

    // ===== Customer Notes =====

    /// <summary>
    /// Notes from customer (e.g., "Please call when ready")
    /// </summary>
    [MaxLength(1000)]
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Internal notes for staff only
    /// </summary>
    [MaxLength(1000)]
    public string? InternalNotes { get; set; }

    // ===== Payment Information =====

    /// <summary>
    /// Payment provider used (Yoco, PayFast, Ozow)
    /// </summary>
    [MaxLength(50)]
    public PaymentProvider? PaymentProvider { get; set; }

    /// <summary>
    /// Payment reference from provider
    /// </summary>
    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Payment completion timestamp
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    // ===== Compliance (POPIA + Cannabis Act) =====

    /// <summary>
    /// Age verified at order creation
    /// </summary>
    public bool AgeVerifiedAtOrder { get; set; }

    /// <summary>
    /// Age verified at pickup (CRITICAL for Cannabis Act)
    /// </summary>
    public bool AgeVerifiedAtPickup { get; set; }

    /// <summary>
    /// Staff member who verified pickup and age
    /// </summary>
    public int? PickupVerifiedBy { get; set; }

    /// <summary>
    /// ID verification method used at pickup
    /// </summary>
    [MaxLength(50)]
    public PickupVerificationMethod? IdVerificationMethod { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Order line items
    /// </summary>
    public virtual ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();

    /// <summary>
    /// Payment transactions for this order
    /// </summary>
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    /// <summary>
    /// Order status history (audit trail)
    /// </summary>
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

    /// <summary>
    /// Pickup confirmation (if picked up)
    /// </summary>
    public virtual PickupConfirmation? PickupConfirmation { get; set; }
}
