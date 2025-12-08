using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.Models.Entities;

/// <summary>
/// Payment transaction audit trail
/// Tracks all payment attempts and their results
/// </summary>
[Table("payment_transactions")]
public class PaymentTransaction : AuditableEntity
{
    /// <summary>
    /// Order ID (foreign key)
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    // ===== Payment Provider =====

    /// <summary>
    /// Payment provider (Yoco, PayFast, Ozow)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public PaymentProvider Provider { get; set; }

    /// <summary>
    /// Transaction ID from payment provider
    /// </summary>
    [MaxLength(200)]
    public string? ProviderTransactionId { get; set; }

    /// <summary>
    /// Payment reference from provider
    /// </summary>
    [MaxLength(200)]
    public string? ProviderReference { get; set; }

    // ===== Transaction Details =====

    /// <summary>
    /// Payment amount
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (default: ZAR)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "ZAR";

    /// <summary>
    /// Transaction status
    /// </summary>
    [Required]
    [MaxLength(50)]
    public PaymentStatus Status { get; set; }

    // ===== Payment Method =====

    /// <summary>
    /// Payment method used (Card, EFT, Instant EFT)
    /// </summary>
    [MaxLength(50)]
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Last 4 digits of card (if card payment)
    /// </summary>
    [MaxLength(4)]
    public string? CardLastFour { get; set; }

    /// <summary>
    /// Card type (Visa, Mastercard, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? CardType { get; set; }

    // ===== Timestamps =====

    /// <summary>
    /// Payment initiated timestamp
    /// </summary>
    [Required]
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Payment completed timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Payment failed timestamp
    /// </summary>
    public DateTime? FailedAt { get; set; }

    // ===== Error Information =====

    /// <summary>
    /// Error code from payment provider
    /// </summary>
    [MaxLength(50)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error message from payment provider
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Raw webhook payload from payment provider (JSON)
    /// Stored for audit and debugging purposes
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? WebhookPayload { get; set; }

    // ===== SARS Compliance =====

    /// <summary>
    /// Receipt number for tax purposes
    /// </summary>
    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }

    /// <summary>
    /// Receipt generation timestamp
    /// </summary>
    public DateTime? ReceiptGeneratedAt { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Parent order
    /// </summary>
    public virtual OnlineOrder Order { get; set; } = null!;
}
