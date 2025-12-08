using FluentValidation;
using Project420.Management.BLL.Sales.Retail.DTOs;

namespace Project420.Management.BLL.Sales.Retail.Validators;

/// <summary>
/// FluentValidation validator for pricelist item updates.
/// Validates all input data for updating a pricelist item.
/// </summary>
public class UpdatePricelistItemValidator : AbstractValidator<UpdatePricelistItemDto>
{
    public UpdatePricelistItemValidator()
    {
        // ============================================================
        // IDENTITY VALIDATION
        // ============================================================

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Pricelist item ID must be greater than 0");

        // ============================================================
        // PRICING VALIDATION
        // ============================================================

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than R0")
            .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed R999,999.99");

        // ============================================================
        // QUANTITY VALIDATION (Tiered Pricing)
        // ============================================================

        RuleFor(x => x.MinimumQuantity)
            .GreaterThan(0).WithMessage("Minimum quantity must be at least 1");

        RuleFor(x => x.MaximumQuantity)
            .GreaterThanOrEqualTo(x => x.MinimumQuantity)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity")
            .When(x => x.MaximumQuantity.HasValue);
    }
}
