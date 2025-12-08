using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Order status change history
/// Complete audit trail for POPIA compliance
/// </summary>
[Table("order_status_history")]
public class OrderStatusHistory : AuditableEntity
{
    /// <summary>
    /// Order ID (foreign key)
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    // ===== Status Change =====

    /// <summary>
    /// Previous status
    /// </summary>
    [MaxLength(50)]
    public OnlineOrderStatus? OldStatus { get; set; }

    /// <summary>
    /// New status
    /// </summary>
    [Required]
    [MaxLength(50)]
    public OnlineOrderStatus NewStatus { get; set; }

    // ===== Reason/Notes =====

    /// <summary>
    /// Reason for status change
    /// </summary>
    [MaxLength(1000)]
    public string? ChangeReason { get; set; }

    /// <summary>
    /// Timestamp of status change
    /// </summary>
    [Required]
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// User who changed the status
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    // ===== Navigation Properties =====

    /// <summary>
    /// Parent order
    /// </summary>
    public virtual OnlineOrder Order { get; set; } = null!;
}
