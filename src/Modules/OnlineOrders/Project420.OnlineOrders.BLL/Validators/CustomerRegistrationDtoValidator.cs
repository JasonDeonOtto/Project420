using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;

namespace Project420.OnlineOrders.BLL.Validators;

/// <summary>
/// FluentValidation validator for CustomerRegistrationDto.
/// Cannabis Act compliance: Age verification (18+).
/// POPIA compliance: Consent validation.
/// </summary>
public class CustomerRegistrationDtoValidator : AbstractValidator<CustomerRegistrationDto>
{
    public CustomerRegistrationDtoValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");

        // First name validation
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        // Last name validation
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        // ID number validation (SA ID format: 13 digits)
        RuleFor(x => x.IdNumber)
            .NotEmpty().WithMessage("ID number is required")
            .MaximumLength(20).WithMessage("ID number cannot exceed 20 characters")
            .Matches(@"^\d{13}$").WithMessage("ID number must be 13 digits (SA ID format)")
            .Must(BeValidSAIdNumber).WithMessage("ID number is not a valid South African ID number");

        // Date of birth validation
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .Must(BeAtLeast18YearsOld).WithMessage("You must be at least 18 years old to register (Cannabis Act compliance)");

        // Phone number validation
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[0-9\+\-\s()]+$").WithMessage("Phone number can only contain digits, +, -, spaces, and parentheses")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // POPIA consent validation (CRITICAL)
        RuleFor(x => x.ConsentToPOPIA)
            .Equal(true).WithMessage("You must consent to POPIA terms to register (legal requirement)");
    }

    /// <summary>
    /// Validates if person is at least 18 years old.
    /// Cannabis Act requirement: Must be 18+ years old.
    /// </summary>
    private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        return age >= 18;
    }

    /// <summary>
    /// Validates South African ID number using Luhn algorithm.
    /// SA ID Format: YYMMDD SSSS C A Z
    /// </summary>
    private bool BeValidSAIdNumber(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13)
        {
            return false;
        }

        if (!idNumber.All(char.IsDigit))
        {
            return false;
        }

        // Validate date of birth part (first 6 digits: YYMMDD)
        try
        {
            int year = int.Parse(idNumber.Substring(0, 2));
            int month = int.Parse(idNumber.Substring(2, 2));
            int day = int.Parse(idNumber.Substring(4, 2));

            if (month < 1 || month > 12 || day < 1 || day > 31)
            {
                return false;
            }

            // Determine full year (assume 1900s for values >= 25, 2000s for values < 25)
            int currentYearLastTwoDigits = DateTime.Now.Year % 100;
            year = year <= currentYearLastTwoDigits ? 2000 + year : 1900 + year;

            // Validate date
            var birthDate = new DateTime(year, month, day);
            if (birthDate > DateTime.Today)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        // Validate check digit using Luhn algorithm
        int sum = 0;
        for (int i = 0; i < 13; i++)
        {
            int digit = int.Parse(idNumber[i].ToString());

            if (i % 2 == 0) // Odd position (1st, 3rd, 5th, etc.)
            {
                sum += digit;
            }
            else // Even position (2nd, 4th, 6th, etc.)
            {
                int doubled = digit * 2;
                sum += doubled > 9 ? doubled - 9 : doubled;
            }
        }

        return sum % 10 == 0;
    }
}
