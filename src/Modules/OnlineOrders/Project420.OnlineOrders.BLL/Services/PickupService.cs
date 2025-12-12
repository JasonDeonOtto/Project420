using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.DAL.Repositories;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service for pickup confirmation business logic.
/// Cannabis Act CRITICAL: Age verification (18+) at pickup.
/// POPIA compliance: Audit trail for all pickup verifications.
/// </summary>
public class PickupService : IPickupService
{
    private readonly IPickupConfirmationRepository _pickupRepository;
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly ICustomerAccountRepository _customerRepository;
    private readonly IValidator<PickupConfirmationDto> _pickupValidator;

    public PickupService(
        IPickupConfirmationRepository pickupRepository,
        IOnlineOrderRepository orderRepository,
        ICustomerAccountRepository customerRepository,
        IValidator<PickupConfirmationDto> pickupValidator)
    {
        _pickupRepository = pickupRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _pickupValidator = pickupValidator;
    }

    /// <summary>
    /// Confirms order pickup with mandatory age verification.
    /// Cannabis Act CRITICAL: Must verify ID and age (18+) at pickup.
    /// </summary>
    public async Task<bool> ConfirmPickupAsync(PickupConfirmationDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _pickupValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Verify order exists and is ready for pickup
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {dto.OrderId} not found");
        }

        if (order.Status != OnlineOrderStatus.ReadyForPickup)
        {
            throw new InvalidOperationException($"Order is not ready for pickup. Current status: {order.Status}");
        }

        // STEP 3: Verify customer
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} not found");
        }

        // STEP 4: Cannabis Act CRITICAL - Verify age (18+)
        if (!dto.AgeConfirmed)
        {
            throw new InvalidOperationException("Age confirmation is REQUIRED (Cannabis Act compliance). Customer must be 18+ years old.");
        }

        // STEP 5: Calculate age from ID number
        var age = CalculateAgeFromIdNumber(dto.IdNumberVerified);
        if (age < 18)
        {
            throw new InvalidOperationException($"Customer is {age} years old. Cannabis Act requires 18+ years. Pickup DENIED.");
        }

        // STEP 6: Create pickup confirmation record
        var pickup = new PickupConfirmation
        {
            OrderId = dto.OrderId,
            PickupDate = DateTime.UtcNow,
            PickedUpByCustomerId = dto.CustomerId,
            IdVerificationMethod = dto.IdVerificationMethod,
            IdNumberVerified = dto.IdNumberVerified,
            AgeConfirmed = true,
            VerifiedByStaffId = dto.VerifiedByStaffId,
            VerificationNotes = dto.VerificationNotes,
            CreatedBy = $"Staff_{dto.VerifiedByStaffId}",
            ModifiedBy = $"Staff_{dto.VerifiedByStaffId}"
        };

        await _pickupRepository.AddAsync(pickup);

        // STEP 7: Update order status to Completed
        order.Status = OnlineOrderStatus.Completed;
        order.ActualPickupDate = DateTime.UtcNow;
        order.AgeVerifiedAtPickup = true;
        order.PickupVerifiedBy = dto.VerifiedByStaffId;
        order.IdVerificationMethod = dto.IdVerificationMethod;
        order.ModifiedAt = DateTime.UtcNow;
        order.ModifiedBy = $"Staff_{dto.VerifiedByStaffId}";

        await _orderRepository.UpdateAsync(order);

        // STEP 8: Update customer's ID verification status
        if (!customer.IdDocumentVerified)
        {
            customer.IdDocumentVerified = true;
            customer.IdNumber = dto.IdNumberVerified;
            await _customerRepository.UpdateAsync(customer);
        }

        return true;
    }

    /// <summary>
    /// Verifies customer age at pickup using ID document.
    /// Cannabis Act CRITICAL: Staff must verify ID and confirm 18+ years.
    /// </summary>
    public async Task<bool> VerifyAgeAtPickupAsync(int orderId, int staffId, PickupVerificationMethod verificationMethod)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return false;
        }

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            return false;
        }

        // Check if customer is 18+ based on date of birth
        var age = CalculateAge(customer.DateOfBirth);
        if (age < 18)
        {
            return false;
        }

        // Update order age verification flags
        order.AgeVerifiedAtPickup = true;
        order.PickupVerifiedBy = staffId;
        order.IdVerificationMethod = verificationMethod;
        order.ModifiedAt = DateTime.UtcNow;
        order.ModifiedBy = $"Staff_{staffId}";

        await _orderRepository.UpdateAsync(order);

        return true;
    }

    /// <summary>
    /// Gets pickup confirmation by order ID.
    /// </summary>
    public async Task<PickupConfirmation?> GetByOrderIdAsync(int orderId)
    {
        var pickups = await _pickupRepository.FindAsync(p => p.OrderId == orderId);
        return pickups.FirstOrDefault();
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Calculates age from date of birth.
    /// </summary>
    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    /// <summary>
    /// Calculates age from South African ID number.
    /// SA ID Format: YYMMDD SSSS C A Z
    /// - YYMMDD: Date of birth
    /// - SSSS: Gender sequence number
    /// - C: Citizenship
    /// - A: Race (no longer used)
    /// - Z: Check digit
    /// </summary>
    private int CalculateAgeFromIdNumber(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 6)
        {
            throw new ArgumentException("Invalid SA ID number format");
        }

        // Extract date parts from ID number
        string yearPart = idNumber.Substring(0, 2);
        string monthPart = idNumber.Substring(2, 2);
        string dayPart = idNumber.Substring(4, 2);

        // Parse year (assume 1900s for values >= 25, 2000s for values < 25)
        int year = int.Parse(yearPart);
        int currentYearLastTwoDigits = DateTime.Now.Year % 100;
        year = year <= currentYearLastTwoDigits ? 2000 + year : 1900 + year;

        int month = int.Parse(monthPart);
        int day = int.Parse(dayPart);

        var dateOfBirth = new DateTime(year, month, day);
        return CalculateAge(dateOfBirth);
    }
}
