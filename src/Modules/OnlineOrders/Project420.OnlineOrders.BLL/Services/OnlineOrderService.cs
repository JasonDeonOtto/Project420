using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;
using Project420.OnlineOrders.DAL.Repositories;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;
using Project420.Shared.Core.Enums;
using Project420.Shared.Core.Services;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service for online order business logic.
/// Cannabis Act compliance: Age verification and purchase tracking.
/// POPIA compliance: Audit trail for all order operations.
/// </summary>
public class OnlineOrderService : IOnlineOrderService
{
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly IOnlineOrderItemRepository _orderItemRepository;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
    private readonly ICustomerAccountRepository _customerRepository;
    private readonly ITransactionNumberGeneratorService _transactionNumberGenerator;
    private readonly IValidator<CreateOrderRequestDto> _createOrderValidator;

    public OnlineOrderService(
        IOnlineOrderRepository orderRepository,
        IOnlineOrderItemRepository orderItemRepository,
        IOrderStatusHistoryRepository statusHistoryRepository,
        ICustomerAccountRepository customerRepository,
        ITransactionNumberGeneratorService transactionNumberGenerator,
        IValidator<CreateOrderRequestDto> createOrderValidator)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _customerRepository = customerRepository;
        _transactionNumberGenerator = transactionNumberGenerator;
        _createOrderValidator = createOrderValidator;
    }

    /// <summary>
    /// Creates a new online order with validation and compliance checks.
    /// Cannabis Act: Verifies customer age (18+) before order creation.
    /// </summary>
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _createOrderValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Verify customer exists and is age verified (Cannabis Act requirement)
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
        {
            return new OrderResponseDto
            {
                Success = false,
                ErrorMessage = "Customer not found"
            };
        }

        if (!customer.AgeVerified)
        {
            return new OrderResponseDto
            {
                Success = false,
                ErrorMessage = "Age verification required. Customer must be 18+ years old (Cannabis Act compliance)."
            };
        }

        // STEP 3: Calculate totals
        decimal subtotal = dto.Items.Sum(item => item.PriceAtTimeOfOrder * item.Quantity);
        decimal vatAmount = subtotal * 0.15m; // SA VAT 15%
        decimal totalAmount = subtotal + vatAmount;

        // STEP 4: Generate order number
        string orderNumber = await _transactionNumberGenerator.GenerateAsync(TransactionTypeCode.ORD);

        // STEP 5: Create order entity
        var order = new OnlineOrder
        {
            OrderNumber = orderNumber,
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OnlineOrderStatus.PendingPayment,
            Subtotal = subtotal,
            VatAmount = vatAmount,
            DiscountAmount = 0,
            TotalAmount = totalAmount,
            PickupLocationId = dto.PickupLocationId,
            PreferredPickupDate = dto.PreferredPickupDate,
            PreferredPickupTime = dto.PreferredPickupTime,
            CustomerNotes = dto.CustomerNotes,
            AgeVerifiedAtOrder = true,
            AgeVerifiedAtPickup = false,
            CreatedBy = $"Customer_{dto.CustomerId}",
            ModifiedBy = $"Customer_{dto.CustomerId}"
        };

        // STEP 6: Save order
        var createdOrder = await _orderRepository.AddAsync(order);

        // STEP 7: Create order items
        foreach (var itemDto in dto.Items)
        {
            var orderItem = new OnlineOrderItem
            {
                OrderId = createdOrder.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.PriceAtTimeOfOrder,
                LineTotal = itemDto.PriceAtTimeOfOrder * itemDto.Quantity,
                CreatedBy = $"Customer_{dto.CustomerId}",
                ModifiedBy = $"Customer_{dto.CustomerId}"
            };
            await _orderItemRepository.AddAsync(orderItem);
        }

        // STEP 8: Create status history entry
        var statusHistory = new OrderStatusHistory
        {
            OrderId = createdOrder.Id,
            NewStatus = OnlineOrderStatus.PendingPayment,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = $"Customer_{dto.CustomerId}",
            ChangeReason = "Order created",
            CreatedBy = $"Customer_{dto.CustomerId}",
            ModifiedBy = $"Customer_{dto.CustomerId}"
        };
        await _statusHistoryRepository.AddAsync(statusHistory);

        // STEP 9: Return response with payment URL
        return new OrderResponseDto
        {
            Success = true,
            OrderId = createdOrder.Id,
            OrderNumber = orderNumber,
            Status = OnlineOrderStatus.PendingPayment,
            Subtotal = subtotal,
            VatAmount = vatAmount,
            TotalAmount = totalAmount,
            PaymentUrl = $"/payments/process/{createdOrder.Id}", // TODO: Generate actual payment URL
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        return order != null ? MapToDto(order) : null;
    }

    /// <summary>
    /// Gets an order by order number.
    /// </summary>
    public async Task<OrderResponseDto?> GetOrderByNumberAsync(string orderNumber)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        return order != null ? MapToDto(order) : null;
    }

    /// <summary>
    /// Gets all orders for a specific customer.
    /// </summary>
    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Updates order status.
    /// Tracks status changes for audit trail (POPIA compliance).
    /// </summary>
    public async Task<OrderStatusDto> UpdateOrderStatusAsync(int orderId, OnlineOrderStatus newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found");
        }

        var oldStatus = order.Status;
        order.Status = newStatus;
        order.ModifiedAt = DateTime.UtcNow;

        // Update specific fields based on status
        if (newStatus == OnlineOrderStatus.ReadyForPickup)
        {
            // Order is ready for customer to pickup
        }
        else if (newStatus == OnlineOrderStatus.Completed)
        {
            order.ActualPickupDate = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        // Create status history entry (POPIA audit trail)
        var statusHistory = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "System", // TODO: Get actual user
            ChangeReason = $"Status changed from {oldStatus} to {newStatus}",
            CreatedBy = "System",
            ModifiedBy = "System"
        };
        await _statusHistoryRepository.AddAsync(statusHistory);

        return new OrderStatusDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            PaymentCompleted = order.PaymentDate.HasValue,
            ReadyForPickup = order.Status == OnlineOrderStatus.ReadyForPickup,
            PreferredPickupDate = order.PreferredPickupDate,
            PreferredPickupTime = order.PreferredPickupTime
        };
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    public async Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return false;
        }

        // Can only cancel pending or confirmed orders
        if (order.Status != OnlineOrderStatus.PendingPayment && order.Status != OnlineOrderStatus.PaymentReceived)
        {
            throw new InvalidOperationException($"Cannot cancel order with status {order.Status}");
        }

        order.Status = OnlineOrderStatus.Cancelled;
        order.ModifiedAt = DateTime.UtcNow;
        order.InternalNotes = $"Cancelled: {reason}";

        await _orderRepository.UpdateAsync(order);

        // Create status history entry
        var statusHistory = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = order.Status,
            NewStatus = OnlineOrderStatus.Cancelled,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "System", // TODO: Get actual user
            ChangeReason = $"Order cancelled: {reason}",
            CreatedBy = "System",
            ModifiedBy = "System"
        };
        await _statusHistoryRepository.AddAsync(statusHistory);

        return true;
    }

    /// <summary>
    /// Gets orders ready for pickup at a specific location.
    /// </summary>
    public async Task<IEnumerable<OrderResponseDto>> GetReadyForPickupAsync(int pickupLocationId)
    {
        var orders = await _orderRepository.GetReadyForPickupAsync(pickupLocationId);
        return orders.Select(MapToDto);
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Maps OnlineOrder entity to OrderResponseDto.
    /// </summary>
    private OrderResponseDto MapToDto(OnlineOrder order)
    {
        return new OrderResponseDto
        {
            Success = true,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            Subtotal = order.Subtotal,
            VatAmount = order.VatAmount,
            TotalAmount = order.TotalAmount
        };
    }
}
