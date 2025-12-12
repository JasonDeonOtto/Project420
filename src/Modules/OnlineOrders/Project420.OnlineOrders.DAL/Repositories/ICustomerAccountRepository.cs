using Project420.OnlineOrders.DAL.Repositories.Common;
using Project420.OnlineOrders.Models.Entities;

namespace Project420.OnlineOrders.DAL.Repositories;

/// <summary>
/// Repository interface for CustomerAccount entity.
/// POPIA compliant customer account management.
/// </summary>
public interface ICustomerAccountRepository : IRepository<CustomerAccount>
{
    /// <summary>
    /// Get customer by email (unique login)
    /// </summary>
    Task<CustomerAccount?> GetByEmailAsync(string email);

    /// <summary>
    /// Get customer by ID number
    /// </summary>
    Task<CustomerAccount?> GetByIdNumberAsync(string idNumber);

    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Get customers requiring age verification
    /// </summary>
    Task<IEnumerable<CustomerAccount>> GetPendingAgeVerificationAsync();

    /// <summary>
    /// Get customers with active orders
    /// </summary>
    Task<IEnumerable<CustomerAccount>> GetCustomersWithActiveOrdersAsync();

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int customerId);
}
