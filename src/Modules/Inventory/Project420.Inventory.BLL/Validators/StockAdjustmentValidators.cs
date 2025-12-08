using FluentValidation;
using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Validators;

public class CreateStockAdjustmentValidator : AbstractValidator<CreateStockAdjustmentDto>
{
    public CreateStockAdjustmentValidator()
    {
        RuleFor(x => x.AdjustmentNumber)
            .NotEmpty().WithMessage("Adjustment number is required")
            .MaximumLength(100).WithMessage("Adjustment number cannot exceed 100 characters");

        RuleFor(x => x.AdjustmentDate)
            .NotEmpty().WithMessage("Adjustment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Adjustment date cannot be in the future");

        RuleFor(x => x.ProductSKU)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(50).WithMessage("Product SKU cannot exceed 50 characters");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.AdjustmentQuantity)
            .NotEqual(0).WithMessage("Adjustment quantity cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for stock adjustments (SAHPRA/SARS compliance)")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.BatchNumber)
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));
    }
}

public class UpdateStockAdjustmentValidator : AbstractValidator<UpdateStockAdjustmentDto>
{
    public UpdateStockAdjustmentValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid stock adjustment ID is required");

        RuleFor(x => x.AdjustmentNumber)
            .NotEmpty().WithMessage("Adjustment number is required")
            .MaximumLength(100).WithMessage("Adjustment number cannot exceed 100 characters");

        RuleFor(x => x.AdjustmentDate)
            .NotEmpty().WithMessage("Adjustment date is required");

        RuleFor(x => x.AdjustmentQuantity)
            .NotEqual(0).WithMessage("Adjustment quantity cannot be zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for stock adjustments")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}
