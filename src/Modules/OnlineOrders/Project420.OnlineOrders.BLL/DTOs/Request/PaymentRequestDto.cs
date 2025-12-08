using System.ComponentModel.DataAnnotations;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.DTOs.Request;

/// <summary>
/// DTO for initiating payment
/// </summary>
public class PaymentRequestDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public PaymentProvider Provider { get; set; }

    /// <summary>
    /// Return URL after successful payment
    /// </summary>
    [Required]
    [Url]
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Cancel URL if payment cancelled
    /// </summary>
    [Required]
    [Url]
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Notify URL for webhooks
    /// </summary>
    [Required]
    [Url]
    public string NotifyUrl { get; set; } = string.Empty;
}
