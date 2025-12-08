using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories.Common;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.DAL.Repositories.Sales.SalesCommon;

/// <summary>
/// Repository implementation for Customer (Debtor) operations.
/// Handles all database access for customer management.
/// </summary>
public class DebtorRepository : Repository<Debtor>, IDebtorRepository
{
    public DebtorRepository(ManagementDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<Debtor?> GetByIdNumberAsync(string idNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.IdNumber == idNumber);
    }

    /// <inheritdoc/>
    public async Task<Debtor?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    /// <inheritdoc/>
    public async Task<Debtor?> GetByMobileAsync(string mobile)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Mobile == mobile);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Debtor>> GetAccountCustomersAsync()
    {
        return await _dbSet
            .Where(c => c.CreditLimit > 0)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Debtor>> GetMedicalPatientsAsync()
    {
        return await _dbSet
            .Where(c => c.MedicalPermitNumber != null
                     && c.MedicalPermitExpiryDate != null
                     && c.MedicalPermitExpiryDate > DateTime.Today)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Debtor>> GetExpiringMedicalPermitsAsync(int daysUntilExpiry)
    {
        var expiryDate = DateTime.Today.AddDays(daysUntilExpiry);

        return await _dbSet
            .Where(c => c.MedicalPermitNumber != null
                     && c.MedicalPermitExpiryDate != null
                     && c.MedicalPermitExpiryDate <= expiryDate
                     && c.MedicalPermitExpiryDate > DateTime.Today)
            .OrderBy(c => c.MedicalPermitExpiryDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Debtor>> GetCustomersWithoutConsentAsync()
    {
        return await _dbSet
            .Where(c => !c.ConsentGiven || c.ConsentDate == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Debtor>> SearchCustomersAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();

        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(term)
                     || c.Email != null && c.Email.ToLower().Contains(term)
                     || c.Mobile != null && c.Mobile.Contains(term)
                     || c.IdNumber != null && c.IdNumber.Contains(term))
            .OrderBy(c => c.Name)
            .Take(50) // Limit results for performance
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsAgeVerifiedAsync(string idNumber)
    {
        // SA ID format: YYMMDD-SSSS-C-ZZ
        // First 6 digits are date of birth
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 6)
            return false;

        try
        {
            // Extract date of birth from ID
            var year = int.Parse(idNumber.Substring(0, 2));
            var month = int.Parse(idNumber.Substring(2, 2));
            var day = int.Parse(idNumber.Substring(4, 2));

            // Determine century (00-24 = 2000s, 25-99 = 1900s)
            var fullYear = year <= 24 ? 2000 + year : 1900 + year;

            var dateOfBirth = new DateTime(fullYear, month, day);
            var age = DateTime.Today.Year - dateOfBirth.Year;

            // Adjust if birthday hasn't occurred this year
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;

            // Cannabis Act requirement: 18+
            return age >= 18;
        }
        catch
        {
            // Invalid ID format
            return false;
        }
    }
}
