using System.ComponentModel.DataAnnotations;

namespace Project420.OnlineOrders.BLL.DTOs.Request;

/// <summary>
/// DTO for creating a new online order
/// </summary>
public class CreateOrderRequestDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemDto> Items { get; set; } = new();

    [Required]
    public int PickupLocationId { get; set; }

    public DateTime? PreferredPickupDate { get; set; }

    public TimeSpan? PreferredPickupTime { get; set; }

    [MaxLength(1000)]
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// Order item within create order request
/// </summary>
public class OrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    /// <summary>
    /// Price at time of order (for verification)
    /// </summary>
    [Required]
    public decimal PriceAtTimeOfOrder { get; set; }
}
