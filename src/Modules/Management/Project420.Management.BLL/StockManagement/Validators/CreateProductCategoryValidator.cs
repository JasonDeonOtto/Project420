using FluentValidation;
using Project420.Management.BLL.StockManagement.DTOs;

namespace Project420.Management.BLL.StockManagement.Validators;

/// <summary>
/// FluentValidation validator for creating product categories.
/// Ensures all required fields are valid before category creation.
/// </summary>
public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryDto>
{
    public CreateProductCategoryValidator()
    {
        // ============================================================
        // NAME VALIDATION
        // ============================================================

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .Length(2, 50).WithMessage("Category name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z0-9\s\-&/()]+$")
                .WithMessage("Category name can only contain letters, numbers, spaces, and basic punctuation (- & / ( ))");

        // ============================================================
        // CATEGORY CODE VALIDATION
        // ============================================================

        RuleFor(x => x.CategoryCode)
            .NotEmpty().WithMessage("Category code is required")
            .Length(2, 20).WithMessage("Category code must be between 2 and 20 characters")
            .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("Category code can only contain letters, numbers, hyphens, or underscores (e.g., FLWR, EDBL, ACCS) - it will be automatically uppercased");

        // ============================================================
        // BUSINESS RULES VALIDATION
        // ============================================================

        // IsActive and SpecialRules are booleans, no validation needed
        // They have default values and cannot be invalid
    }
}
