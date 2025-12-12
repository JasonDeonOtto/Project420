using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service interface for customer account business logic.
/// Cannabis Act compliance: Age verification (18+).
/// POPIA compliance: Consent tracking and data protection.
/// </summary>
public interface ICustomerAccountService
{
    /// <summary>
    /// Registers a new customer account.
    /// Cannabis Act: Verifies age requirement (18+).
    /// POPIA: Requires explicit consent.
    /// </summary>
    /// <param name="dto">Customer registration data</param>
    /// <returns>Authentication response with JWT token</returns>
    Task<AuthResponseDto> RegisterAsync(CustomerRegistrationDto dto);

    /// <summary>
    /// Authenticates a customer.
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);

    /// <summary>
    /// Gets customer account by ID.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer account</returns>
    Task<CustomerAccount?> GetByIdAsync(int customerId);

    /// <summary>
    /// Gets customer account by email.
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <returns>Customer account</returns>
    Task<CustomerAccount?> GetByEmailAsync(string email);

    /// <summary>
    /// Verifies customer age based on date of birth.
    /// Cannabis Act: Must be 18+ years old.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="dateOfBirth">Date of birth from ID document</param>
    /// <returns>True if 18+, false otherwise</returns>
    Task<bool> VerifyAgeAsync(int customerId, DateTime dateOfBirth);

    /// <summary>
    /// Updates last login timestamp.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    Task UpdateLastLoginAsync(int customerId);
}
