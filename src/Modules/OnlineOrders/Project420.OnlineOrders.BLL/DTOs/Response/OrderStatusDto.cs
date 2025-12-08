using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.DTOs.Response;

/// <summary>
/// DTO for order status
/// </summary>
public class OrderStatusDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OnlineOrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool PaymentCompleted { get; set; }
    public bool ReadyForPickup { get; set; }
    public DateTime? PreferredPickupDate { get; set; }
    public TimeSpan? PreferredPickupTime { get; set; }
}
