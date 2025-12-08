using FluentValidation;
using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Validators;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.MovementNumber)
            .NotEmpty().WithMessage("Movement number is required")
            .MaximumLength(100).WithMessage("Movement number cannot exceed 100 characters");

        RuleFor(x => x.MovementDate)
            .NotEmpty().WithMessage("Movement date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Movement date cannot be in the future");

        RuleFor(x => x.ProductSKU)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(50).WithMessage("Product SKU cannot exceed 50 characters");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero");

        RuleFor(x => x.UnitCost)
            .GreaterThan(0).WithMessage("Unit cost must be greater than 0")
            .When(x => x.UnitCost.HasValue);

        RuleFor(x => x.BatchNumber)
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));
    }
}

public class UpdateStockMovementValidator : AbstractValidator<UpdateStockMovementDto>
{
    public UpdateStockMovementValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid stock movement ID is required");

        RuleFor(x => x.MovementDate)
            .NotEmpty().WithMessage("Movement date is required");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero");

        RuleFor(x => x.UnitCost)
            .GreaterThan(0).WithMessage("Unit cost must be greater than 0")
            .When(x => x.UnitCost.HasValue);
    }
}
