using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service interface for pickup confirmation business logic.
/// Cannabis Act CRITICAL: Age verification (18+) at pickup.
/// POPIA compliance: Audit trail for all pickup verifications.
/// </summary>
public interface IPickupService
{
    /// <summary>
    /// Confirms order pickup with mandatory age verification.
    /// Cannabis Act CRITICAL: Must verify ID and age (18+) at pickup.
    /// </summary>
    /// <param name="dto">Pickup confirmation data with ID verification</param>
    /// <returns>True if pickup confirmed successfully</returns>
    Task<bool> ConfirmPickupAsync(PickupConfirmationDto dto);

    /// <summary>
    /// Verifies customer age at pickup using ID document.
    /// Cannabis Act CRITICAL: Staff must verify ID and confirm 18+ years.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="staffId">Staff member performing verification</param>
    /// <param name="verificationMethod">ID verification method used</param>
    /// <returns>True if age verified (18+), false otherwise</returns>
    Task<bool> VerifyAgeAtPickupAsync(int orderId, int staffId, PickupVerificationMethod verificationMethod);

    /// <summary>
    /// Gets pickup confirmation by order ID.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Pickup confirmation details</returns>
    Task<PickupConfirmation?> GetByOrderIdAsync(int orderId);
}
