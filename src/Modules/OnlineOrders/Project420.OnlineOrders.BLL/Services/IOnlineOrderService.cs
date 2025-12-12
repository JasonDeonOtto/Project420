using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service interface for online order business logic.
/// Cannabis Act compliance: Age verification and purchase tracking.
/// </summary>
public interface IOnlineOrderService
{
    /// <summary>
    /// Creates a new online order with validation and compliance checks.
    /// Cannabis Act: Verifies customer age (18+) before order creation.
    /// </summary>
    /// <param name="dto">Order creation data</param>
    /// <returns>Order response with payment URL</returns>
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto);

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    Task<OrderResponseDto?> GetOrderByIdAsync(int orderId);

    /// <summary>
    /// Gets an order by order number.
    /// </summary>
    /// <param name="orderNumber">Order number (e.g., "ORD-20241208-001")</param>
    /// <returns>Order details</returns>
    Task<OrderResponseDto?> GetOrderByNumberAsync(string orderNumber);

    /// <summary>
    /// Gets all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer orders</returns>
    Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerIdAsync(int customerId);

    /// <summary>
    /// Updates order status.
    /// Tracks status changes for audit trail (POPIA compliance).
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New status</param>
    /// <returns>Updated status information</returns>
    Task<OrderStatusDto> UpdateOrderStatusAsync(int orderId, OnlineOrderStatus newStatus);

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>True if cancelled successfully</returns>
    Task<bool> CancelOrderAsync(int orderId, string reason);

    /// <summary>
    /// Gets orders ready for pickup at a specific location.
    /// </summary>
    /// <param name="pickupLocationId">Pickup location ID</param>
    /// <returns>List of orders ready for pickup</returns>
    Task<IEnumerable<OrderResponseDto>> GetReadyForPickupAsync(int pickupLocationId);
}
