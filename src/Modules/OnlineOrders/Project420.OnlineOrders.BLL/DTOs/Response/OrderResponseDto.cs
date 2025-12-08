using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.DTOs.Response;

/// <summary>
/// DTO for order response
/// </summary>
public class OrderResponseDto
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OnlineOrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PickupLocationDto? PickupLocation { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Pickup location details
/// </summary>
public class PickupLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
