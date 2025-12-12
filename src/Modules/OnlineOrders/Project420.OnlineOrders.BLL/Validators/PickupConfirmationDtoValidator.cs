using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;

namespace Project420.OnlineOrders.BLL.Validators;

/// <summary>
/// FluentValidation validator for PickupConfirmationDto.
/// Cannabis Act CRITICAL: Validates age verification at pickup.
/// </summary>
public class PickupConfirmationDtoValidator : AbstractValidator<PickupConfirmationDto>
{
    public PickupConfirmationDtoValidator()
    {
        // Order ID validation
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required");

        // Customer ID validation
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        // ID verification method validation
        RuleFor(x => x.IdVerificationMethod)
            .IsInEnum().WithMessage("Invalid ID verification method");

        // ID number validation (SA ID format: 13 digits)
        RuleFor(x => x.IdNumberVerified)
            .NotEmpty().WithMessage("ID number is required for age verification (Cannabis Act compliance)")
            .MaximumLength(20).WithMessage("ID number cannot exceed 20 characters")
            .Matches(@"^\d{13}$").WithMessage("ID number must be 13 digits (SA ID format)")
            .Must(BeValidSAIdNumber).WithMessage("ID number is not a valid South African ID number");

        // Age confirmation validation (Cannabis Act CRITICAL)
        RuleFor(x => x.AgeConfirmed)
            .Equal(true).WithMessage("Age confirmation is REQUIRED (Cannabis Act compliance). Customer must be 18+ years old.");

        // Staff ID validation
        RuleFor(x => x.VerifiedByStaffId)
            .GreaterThan(0).WithMessage("Staff ID is required");

        // Verification notes validation
        RuleFor(x => x.VerificationNotes)
            .MaximumLength(1000).WithMessage("Verification notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.VerificationNotes));
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
