using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Online order line item
/// Stores snapshot of product details at time of order for audit trail
/// </summary>
[Table("online_order_items")]
public class OnlineOrderItem : AuditableEntity
{
    /// <summary>
    /// Order ID (foreign key)
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Product ID (reference to master products table)
    /// </summary>
    [Required]
    public int ProductId { get; set; }

    // ===== Product Details (Snapshot at Time of Order) =====

    /// <summary>
    /// Product name at time of order
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product code at time of order
    /// </summary>
    [MaxLength(50)]
    public string? ProductCode { get; set; }

    /// <summary>
    /// Category code at time of order
    /// </summary>
    [MaxLength(50)]
    public string? CategoryCode { get; set; }

    // ===== Pricing (Snapshot at Time of Order) =====

    /// <summary>
    /// Quantity ordered
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of order (before VAT)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line subtotal (Quantity × UnitPrice)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineSubtotal { get; set; }

    /// <summary>
    /// Line VAT amount
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineVat { get; set; }

    /// <summary>
    /// Line total (LineSubtotal + LineVat)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    // ===== Cannabis Compliance (Snapshot at Time of Order) =====

    /// <summary>
    /// THC content percentage at time of order
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? ThcContent { get; set; }

    /// <summary>
    /// CBD content percentage at time of order
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? CbdContent { get; set; }

    /// <summary>
    /// Strain type (Indica, Sativa, Hybrid)
    /// </summary>
    [MaxLength(50)]
    public string? StrainType { get; set; }

    /// <summary>
    /// Batch number for traceability
    /// </summary>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Lab test date for this batch
    /// </summary>
    public DateTime? LabTestDate { get; set; }

    // ===== Fulfillment =====

    /// <summary>
    /// Inventory reserved for this item
    /// </summary>
    public bool Reserved { get; set; }

    /// <summary>
    /// Timestamp when inventory was reserved
    /// </summary>
    public DateTime? ReservedAt { get; set; }

    /// <summary>
    /// Item has been picked from inventory
    /// </summary>
    public bool Picked { get; set; }

    /// <summary>
    /// Timestamp when item was picked
    /// </summary>
    public DateTime? PickedAt { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Parent order
    /// </summary>
    public virtual OnlineOrder Order { get; set; } = null!;
}
