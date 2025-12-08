using FluentValidation;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.DAL.Repositories.Sales.SalesCommon;
using Project420.Management.Models.Entities.Sales.Common;
using Project420.Shared.Core.Compliance.Services;

namespace Project420.Management.BLL.Sales.SalesCommon.Services;

/// <summary>
/// Service for customer (debtor) business logic.
/// Handles registration, validation, and customer management operations.
/// </summary>
public class CustomerService
{
    private readonly IDebtorRepository _customerRepository;
    private readonly IValidator<CustomerRegistrationDto> _validator;
    private readonly ICannabisComplianceService _complianceService;

    public CustomerService(
        IDebtorRepository customerRepository,
        IValidator<CustomerRegistrationDto> validator,
        ICannabisComplianceService complianceService)
    {
        _customerRepository = customerRepository;
        _validator = validator;
        _complianceService = complianceService;
    }

    /// <summary>
    /// Registers a new customer with full validation and compliance checks.
    /// </summary>
    /// <param name="dto">Customer registration data</param>
    /// <returns>Registered customer with generated ID</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If duplicate customer exists</exception>
    public async Task<Debtor> RegisterCustomerAsync(CustomerRegistrationDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check for duplicate customers
        await CheckForDuplicatesAsync(dto);

        // STEP 3: Extract date of birth from ID number using compliance service
        var dateOfBirth = _complianceService.ExtractDateOfBirth(dto.IdNumber);

        // STEP 4: Map DTO to Entity
        var customer = new Debtor
        {
            // Personal Information
            Name = dto.Name,
            IdNumber = dto.IdNumber,
            DateOfBirth = dateOfBirth,

            // Contact Information
            Mobile = dto.Mobile,
            Email = dto.Email,
            PhysicalAddress = dto.PhysicalAddress,
            PostalAddress = dto.PostalAddress,

            // Medical Cannabis (Section 21)
            MedicalPermitNumber = dto.MedicalPermitNumber,
            MedicalPermitExpiryDate = dto.MedicalPermitExpiryDate,
            PrescribingDoctor = dto.PrescribingDoctor,

            // Account/Credit
            CreditLimit = dto.CreditLimit,
            PaymentTerms = dto.PaymentTerms,
            CurrentBalance = 0, // New customer starts with 0 balance

            // POPIA Compliance
            ConsentGiven = dto.ConsentGiven,
            ConsentDate = dto.ConsentGiven ? DateTime.UtcNow : null,
            ConsentPurpose = dto.ConsentPurpose,
            MarketingConsent = dto.MarketingConsent,

            // Status
            IsActive = true,

            // Notes
            Notes = dto.Notes
        };

        // STEP 5: Save to database
        var registeredCustomer = await _customerRepository.AddAsync(customer);

        return registeredCustomer;
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    public async Task<Debtor?> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all customers.
    /// </summary>
    public async Task<IEnumerable<Debtor>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }

    /// <summary>
    /// Searches for customers by name, email, or mobile.
    /// </summary>
    public async Task<IEnumerable<Debtor>> SearchCustomersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Debtor>();

        return await _customerRepository.SearchCustomersAsync(searchTerm);
    }

    /// <summary>
    /// Gets all medical cannabis patients.
    /// </summary>
    public async Task<IEnumerable<Debtor>> GetMedicalPatientsAsync()
    {
        return await _customerRepository.GetMedicalPatientsAsync();
    }

    /// <summary>
    /// Gets customers with expiring medical permits.
    /// </summary>
    public async Task<IEnumerable<Debtor>> GetExpiringMedicalPermitsAsync(int daysUntilExpiry = 30)
    {
        return await _customerRepository.GetExpiringMedicalPermitsAsync(daysUntilExpiry);
    }

    /// <summary>
    /// Updates customer information.
    /// </summary>
    public async Task UpdateCustomerAsync(Debtor customer)
    {
        await _customerRepository.UpdateAsync(customer);
    }

    /// <summary>
    /// Deactivates a customer (soft delete).
    /// POPIA requires data retention, so we use soft delete.
    /// </summary>
    public async Task DeactivateCustomerAsync(int id)
    {
        await _customerRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Verifies if customer meets age requirement (18+).
    /// </summary>
    public async Task<bool> IsAgeVerifiedAsync(string idNumber)
    {
        return await _customerRepository.IsAgeVerifiedAsync(idNumber);
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Checks for duplicate customers by ID number, email, or mobile.
    /// </summary>
    private async Task CheckForDuplicatesAsync(CustomerRegistrationDto dto)
    {
        // Check ID number
        var existingByIdNumber = await _customerRepository.GetByIdNumberAsync(dto.IdNumber);
        if (existingByIdNumber != null)
        {
            throw new InvalidOperationException($"Customer with ID number {dto.IdNumber} already exists");
        }

        // Check email
        var existingByEmail = await _customerRepository.GetByEmailAsync(dto.Email);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException($"Customer with email {dto.Email} already exists");
        }

        // Check mobile
        var existingByMobile = await _customerRepository.GetByMobileAsync(dto.Mobile);
        if (existingByMobile != null)
        {
            throw new InvalidOperationException($"Customer with mobile {dto.Mobile} already exists");
        }
    }

}
