using FluentValidation;
using Project420.Management.BLL.Sales.Retail.DTOs;

namespace Project420.Management.BLL.Sales.Retail.Validators;

/// <summary>
/// FluentValidation validator for pricelist updates.
/// Validates all input data for updating an existing pricelist.
/// </summary>
public class UpdatePricelistValidator : AbstractValidator<UpdatePricelistDto>
{
    public UpdatePricelistValidator()
    {
        // ============================================================
        // IDENTITY VALIDATION
        // ============================================================

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Pricelist ID must be greater than 0");

        // ============================================================
        // BASIC INFORMATION VALIDATION
        // ============================================================

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pricelist name is required")
            .Length(2, 200).WithMessage("Pricelist name must be between 2 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Pricelist code cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Code can only contain uppercase letters, numbers, hyphens, and underscores")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

        // ============================================================
        // DATE RANGE VALIDATION
        // ============================================================

        RuleFor(x => x.EffectiveFrom)
            .LessThanOrEqualTo(x => x.EffectiveTo ?? DateTime.MaxValue)
            .WithMessage("Effective from date must be before effective to date")
            .When(x => x.EffectiveFrom.HasValue);

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom ?? DateTime.MinValue)
            .WithMessage("Effective to date must be after effective from date")
            .When(x => x.EffectiveTo.HasValue);

        // ============================================================
        // PRICING STRATEGY VALIDATION
        // ============================================================

        RuleFor(x => x.PricingStrategy)
            .NotEmpty().WithMessage("Pricing strategy is required")
            .Must(BeValidPricingStrategy).WithMessage("Pricing strategy must be 'Fixed', 'Percentage', or 'Tiered'");

        RuleFor(x => x.PercentageAdjustment)
            .InclusiveBetween(-100, 1000).WithMessage("Percentage adjustment must be between -100% and +1000%")
            .When(x => x.PercentageAdjustment.HasValue);

        // Percentage adjustment required if strategy is Percentage
        RuleFor(x => x.PercentageAdjustment)
            .NotNull().WithMessage("Percentage adjustment is required when pricing strategy is 'Percentage'")
            .When(x => x.PricingStrategy.Equals("Percentage", StringComparison.OrdinalIgnoreCase));

        // ============================================================
        // PRIORITY VALIDATION
        // ============================================================

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 999).WithMessage("Priority must be between 1 and 999");
    }

    /// <summary>
    /// Validates pricing strategy is one of the allowed values.
    /// </summary>
    private bool BeValidPricingStrategy(string strategy)
    {
        if (string.IsNullOrWhiteSpace(strategy))
            return false;

        var validStrategies = new[] { "Fixed", "Percentage", "Tiered" };
        return validStrategies.Contains(strategy, StringComparer.OrdinalIgnoreCase);
    }
}
