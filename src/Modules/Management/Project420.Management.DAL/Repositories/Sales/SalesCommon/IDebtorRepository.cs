using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.DAL.Repositories.Sales.SalesCommon;

/// <summary>
/// Repository interface for Customer (Debtor) operations.
/// Extends generic repository with customer-specific methods.
/// </summary>
public interface IDebtorRepository : IRepository<Debtor>
{
    /// <summary>
    /// Finds a customer by their SA ID number.
    /// Used for duplicate detection during registration.
    /// </summary>
    /// <param name="idNumber">SA ID number (format: YYMMDD-SSSS-C-ZZ)</param>
    /// <returns>Customer if found, null otherwise</returns>
    Task<Debtor?> GetByIdNumberAsync(string idNumber);

    /// <summary>
    /// Finds a customer by email address.
    /// Used for duplicate detection and login.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Customer if found, null otherwise</returns>
    Task<Debtor?> GetByEmailAsync(string email);

    /// <summary>
    /// Finds a customer by mobile number.
    /// Used for duplicate detection and SMS notifications.
    /// </summary>
    /// <param name="mobile">Mobile number</param>
    /// <returns>Customer if found, null otherwise</returns>
    Task<Debtor?> GetByMobileAsync(string mobile);

    /// <summary>
    /// Gets all customers with account status (credit customers).
    /// </summary>
    /// <returns>Customers who have credit accounts</returns>
    Task<IEnumerable<Debtor>> GetAccountCustomersAsync();

    /// <summary>
    /// Gets all medical cannabis patients (Section 21 permit holders).
    /// </summary>
    /// <returns>Customers with valid medical permits</returns>
    Task<IEnumerable<Debtor>> GetMedicalPatientsAsync();

    /// <summary>
    /// Gets customers with expiring medical permits (within specified days).
    /// Used for renewal reminders.
    /// </summary>
    /// <param name="daysUntilExpiry">Number of days before expiry</param>
    /// <returns>Customers with permits expiring soon</returns>
    Task<IEnumerable<Debtor>> GetExpiringMedicalPermitsAsync(int daysUntilExpiry);

    /// <summary>
    /// Gets customers who haven't given POPIA consent.
    /// Used for compliance tracking and follow-up.
    /// </summary>
    /// <returns>Customers without consent</returns>
    Task<IEnumerable<Debtor>> GetCustomersWithoutConsentAsync();

    /// <summary>
    /// Searches customers by name, email, or mobile.
    /// Used for quick lookup in POS system.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Matching customers</returns>
    Task<IEnumerable<Debtor>> SearchCustomersAsync(string searchTerm);

    /// <summary>
    /// Validates that customer meets age requirement (18+).
    /// Cannabis Act compliance check.
    /// </summary>
    /// <param name="idNumber">SA ID number</param>
    /// <returns>True if 18 or older, false otherwise</returns>
    Task<bool> IsAgeVerifiedAsync(string idNumber);
}
