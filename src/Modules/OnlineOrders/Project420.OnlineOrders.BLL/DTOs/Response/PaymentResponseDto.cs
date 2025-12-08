using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.DTOs.Response;

/// <summary>
/// DTO for payment response
/// </summary>
public class PaymentResponseDto
{
    public bool Success { get; set; }
    public int TransactionId { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public PaymentProvider Provider { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateTime ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}
