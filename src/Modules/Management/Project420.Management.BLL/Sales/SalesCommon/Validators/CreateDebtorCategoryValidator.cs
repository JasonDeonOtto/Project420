using FluentValidation;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;

namespace Project420.Management.BLL.Sales.SalesCommon.Validators;

/// <summary>
/// FluentValidation validator for creating customer (debtor) categories.
/// Ensures all required fields are valid before category creation.
/// Cannabis Act & POPIA Compliance: Validates category codes and special rules.
/// </summary>
public class CreateDebtorCategoryValidator : AbstractValidator<CreateDebtorCategoryDto>
{
    public CreateDebtorCategoryValidator()
    {
        // ============================================================
        // NAME VALIDATION
        // ============================================================

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer category name is required")
            .Length(2, 50).WithMessage("Category name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z0-9\s\-&/()]+$")
                .WithMessage("Category name can only contain letters, numbers, spaces, and basic punctuation (- & / ( ))");

        // ============================================================
        // DEBTOR CODE VALIDATION
        // ============================================================

        RuleFor(x => x.DebtorCode)
            .NotEmpty().WithMessage("Customer category code is required")
            .Length(2, 20).WithMessage("Category code must be between 2 and 20 characters")
            .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("Category code must be uppercase letters, numbers, hyphens, or underscores only (e.g., RETAIL, VIP, MEDICAL)");

        // ============================================================
        // BUSINESS RULES VALIDATION
        // ============================================================

        // IsActive and SpecialRules are booleans with default values
        // No validation needed as they cannot be invalid
    }
}
