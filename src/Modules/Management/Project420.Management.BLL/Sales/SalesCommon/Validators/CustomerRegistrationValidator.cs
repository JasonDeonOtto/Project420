using FluentValidation;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Shared.Core.Compliance;
using Project420.Shared.Core.Compliance.Validators;

namespace Project420.Management.BLL.Sales.SalesCommon.Validators;

/// <summary>
/// FluentValidation validator for customer registration.
/// Validates all input data and enforces Cannabis Act compliance.
/// </summary>
public class CustomerRegistrationValidator : AbstractValidator<CustomerRegistrationDto>
{
    public CustomerRegistrationValidator()
    {
        // ============================================================
        // PERSONAL INFORMATION VALIDATION
        // ============================================================

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .Length(2, 200).WithMessage("Name must be between 2 and 200 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.IdNumber)
            .NotEmpty().WithMessage("ID number is required")
            .Length(13).WithMessage("SA ID number must be 13 digits")
            .Matches(@"^\d{13}$").WithMessage("ID number must contain only digits")
            .Must(AgeVerificationValidator.IsValidSAIdNumberDateFormat).WithMessage("Invalid South African ID number date format")
            .Must(AgeVerificationValidator.IsAtLeast18YearsOld).WithMessage(CannabisComplianceConstants.ValidationMessages.AGE_REQUIREMENT);

        // ============================================================
        // CONTACT INFORMATION VALIDATION
        // ============================================================

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("Mobile number is required")
            .Matches(@"^0[0-9]{9}$").WithMessage("Mobile number must be in format: 0XXXXXXXXX (10 digits starting with 0)");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(200).WithMessage("Email address is too long");

        // ============================================================
        // ADDRESS VALIDATION (Optional)
        // ============================================================

        RuleFor(x => x.PhysicalAddress)
            .MaximumLength(500).WithMessage("Physical address is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.PhysicalAddress));

        RuleFor(x => x.PostalAddress)
            .MaximumLength(500).WithMessage("Postal address is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalAddress));

        // ============================================================
        // MEDICAL CANNABIS VALIDATION (Section 21)
        // ============================================================

        RuleFor(x => x.MedicalPermitNumber)
            .MaximumLength(50).WithMessage("Medical permit number is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.MedicalPermitNumber));

        RuleFor(x => x.MedicalPermitExpiryDate)
            .NotEmpty().WithMessage("Medical permit expiry date is required for medical patients")
            .GreaterThan(DateTime.Today).WithMessage("Medical permit has expired")
            .When(x => !string.IsNullOrWhiteSpace(x.MedicalPermitNumber));

        RuleFor(x => x.PrescribingDoctor)
            .NotEmpty().WithMessage("Prescribing doctor is required for medical patients")
            .MaximumLength(200).WithMessage("Doctor name is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.MedicalPermitNumber));

        // ============================================================
        // CREDIT/ACCOUNT VALIDATION
        // ============================================================

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative")
            .LessThanOrEqualTo(1000000).WithMessage("Credit limit is too high (max R1,000,000)");

        RuleFor(x => x.PaymentTerms)
            .GreaterThanOrEqualTo(0).WithMessage("Payment terms cannot be negative")
            .LessThanOrEqualTo(90).WithMessage("Payment terms cannot exceed 90 days")
            .Must((dto, terms) => terms > 0 || dto.CreditLimit == 0)
                .WithMessage("Payment terms required for credit customers");

        // ============================================================
        // POPIA COMPLIANCE VALIDATION
        // ============================================================

        RuleFor(x => x.ConsentGiven)
            .Equal(true).WithMessage("POPIA consent is required to create customer account");

        RuleFor(x => x.ConsentPurpose)
            .NotEmpty().WithMessage("Consent purpose must be specified")
            .MaximumLength(500).WithMessage("Consent purpose is too long");

        // ============================================================
        // NOTES VALIDATION
        // ============================================================

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes are too long (max 2000 characters)")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }

}
