using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.BLL.DTOs.Response;
using Project420.OnlineOrders.DAL.Repositories;
using Project420.OnlineOrders.Models.Entities;
using Project420.OnlineOrders.Models.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Project420.OnlineOrders.BLL.Services;

/// <summary>
/// Service for customer account business logic.
/// Cannabis Act compliance: Age verification (18+).
/// POPIA compliance: Consent tracking and data protection.
/// </summary>
public class CustomerAccountService : ICustomerAccountService
{
    private readonly ICustomerAccountRepository _customerRepository;
    private readonly IValidator<CustomerRegistrationDto> _registrationValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public CustomerAccountService(
        ICustomerAccountRepository customerRepository,
        IValidator<CustomerRegistrationDto> registrationValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _customerRepository = customerRepository;
        _registrationValidator = registrationValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Registers a new customer account.
    /// Cannabis Act: Verifies age requirement (18+).
    /// POPIA: Requires explicit consent.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(CustomerRegistrationDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _registrationValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        // STEP 2: Check if email already exists
        if (await _customerRepository.EmailExistsAsync(dto.Email))
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "Email address already registered"
            };
        }

        // STEP 3: Verify age (Cannabis Act requirement: 18+)
        var age = CalculateAge(dto.DateOfBirth);
        if (age < 18)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "You must be 18 years or older to register (Cannabis Act compliance)"
            };
        }

        // STEP 4: Hash password (never store plain text)
        string passwordHash = HashPassword(dto.Password);

        // STEP 5: Create customer account
        var customer = new CustomerAccount
        {
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdNumber = dto.IdNumber,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,

            // Age Verification (Cannabis Act)
            AgeVerified = true,
            AgeVerificationDate = DateTime.UtcNow,
            AgeVerificationMethod = PickupVerificationMethod.IDDocument,
            IdDocumentVerified = false, // Requires in-person verification

            // POPIA Compliance
            ConsentToPOPIA = dto.ConsentToPOPIA,
            ConsentToPOPIADate = DateTime.UtcNow,
            ConsentToMarketing = dto.ConsentToMarketing,
            ConsentToMarketingDate = dto.ConsentToMarketing ? DateTime.UtcNow : null,

            // Account Status
            IsActive = true,
            IsLocked = false,
            EmailVerified = false, // Requires email verification
            EmailVerificationToken = GenerateVerificationToken(),

            // Audit
            CreatedBy = dto.Email,
            ModifiedBy = dto.Email
        };

        var createdCustomer = await _customerRepository.AddAsync(customer);

        // STEP 6: Generate JWT token (TODO: Implement actual JWT generation)
        return new AuthResponseDto
        {
            Success = true,
            CustomerId = createdCustomer.Id,
            Token = GenerateJwtToken(createdCustomer), // TODO: Implement JWT
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = 3600, // 1 hour
            AgeVerified = true,
            RequiresIDVerification = true // Still needs in-person ID verification at first pickup
        };
    }

    /// <summary>
    /// Authenticates a customer.
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "Invalid login credentials"
            };
        }

        // STEP 2: Find customer by email
        var customer = await _customerRepository.GetByEmailAsync(dto.Email.ToLowerInvariant());
        if (customer == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        // STEP 3: Check if account is locked
        if (customer.IsLocked)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "Account is locked. Please contact support."
            };
        }

        // STEP 4: Verify password
        if (!VerifyPassword(dto.Password, customer.PasswordHash))
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        // STEP 5: Update last login timestamp
        await _customerRepository.UpdateLastLoginAsync(customer.Id);

        // STEP 6: Generate JWT token
        return new AuthResponseDto
        {
            Success = true,
            CustomerId = customer.Id,
            Token = GenerateJwtToken(customer), // TODO: Implement JWT
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = dto.RememberMe ? 86400 : 3600, // 24h or 1h
            AgeVerified = customer.AgeVerified,
            RequiresIDVerification = !customer.IdDocumentVerified
        };
    }

    /// <summary>
    /// Gets customer account by ID.
    /// </summary>
    public async Task<CustomerAccount?> GetByIdAsync(int customerId)
    {
        return await _customerRepository.GetByIdAsync(customerId);
    }

    /// <summary>
    /// Gets customer account by email.
    /// </summary>
    public async Task<CustomerAccount?> GetByEmailAsync(string email)
    {
        return await _customerRepository.GetByEmailAsync(email.ToLowerInvariant());
    }

    /// <summary>
    /// Verifies customer age based on date of birth.
    /// Cannabis Act: Must be 18+ years old.
    /// </summary>
    public async Task<bool> VerifyAgeAsync(int customerId, DateTime dateOfBirth)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            return false;
        }

        var age = CalculateAge(dateOfBirth);
        if (age >= 18)
        {
            customer.AgeVerified = true;
            customer.AgeVerificationDate = DateTime.UtcNow;
            customer.DateOfBirth = dateOfBirth;
            await _customerRepository.UpdateAsync(customer);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates last login timestamp.
    /// </summary>
    public async Task UpdateLastLoginAsync(int customerId)
    {
        await _customerRepository.UpdateLastLoginAsync(customerId);
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
    /// Hashes password using SHA256 (TODO: Use BCrypt or Argon2 in production).
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifies password against hash.
    /// </summary>
    private bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    /// <summary>
    /// Generates JWT token (placeholder implementation).
    /// TODO: Implement actual JWT token generation.
    /// </summary>
    private string GenerateJwtToken(CustomerAccount customer)
    {
        // TODO: Implement JWT token generation
        return $"jwt_token_{customer.Id}_{DateTime.UtcNow.Ticks}";
    }

    /// <summary>
    /// Generates refresh token.
    /// </summary>
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    /// <summary>
    /// Generates email verification token.
    /// </summary>
    private string GenerateVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
