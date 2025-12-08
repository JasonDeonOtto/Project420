using FluentValidation;
using Project420.Management.BLL.StockManagement.DTOs;

namespace Project420.Management.BLL.StockManagement.Validators;

/// <summary>
/// FluentValidation validator for updating product categories.
/// Ensures all fields are valid before category update.
/// </summary>
public class UpdateProductCategoryValidator : AbstractValidator<UpdateProductCategoryDto>
{
    public UpdateProductCategoryValidator()
    {
        // ============================================================
        // ID VALIDATION
        // ============================================================

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");

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
    }
}
