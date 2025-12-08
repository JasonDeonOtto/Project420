using FluentValidation;
using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Validators;

public class CreateStockCountValidator : AbstractValidator<CreateStockCountDto>
{
    public CreateStockCountValidator()
    {
        RuleFor(x => x.CountNumber)
            .NotEmpty().WithMessage("Count number is required")
            .MaximumLength(100).WithMessage("Count number cannot exceed 100 characters");

        RuleFor(x => x.CountDate)
            .NotEmpty().WithMessage("Count date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Count date cannot be in the future");

        RuleFor(x => x.ProductSKU)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(50).WithMessage("Product SKU cannot exceed 50 characters");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.SystemQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("System quantity cannot be negative");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Actual quantity cannot be negative");

        RuleFor(x => x.CountedBy)
            .NotEmpty().WithMessage("Counted by is required")
            .MaximumLength(100).WithMessage("Counted by cannot exceed 100 characters");

        RuleFor(x => x.CountType)
            .MaximumLength(50).WithMessage("Count type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CountType));

        RuleFor(x => x.BatchNumber)
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

public class UpdateStockCountValidator : AbstractValidator<UpdateStockCountDto>
{
    public UpdateStockCountValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid stock count ID is required");

        RuleFor(x => x.CountNumber)
            .NotEmpty().WithMessage("Count number is required")
            .MaximumLength(100).WithMessage("Count number cannot exceed 100 characters");

        RuleFor(x => x.CountDate)
            .NotEmpty().WithMessage("Count date is required");

        RuleFor(x => x.SystemQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("System quantity cannot be negative");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Actual quantity cannot be negative");

        RuleFor(x => x.CountedBy)
            .NotEmpty().WithMessage("Counted by is required")
            .MaximumLength(100).WithMessage("Counted by cannot exceed 100 characters");

        RuleFor(x => x.VerifiedBy)
            .MaximumLength(100).WithMessage("Verified by cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.VerifiedBy));
    }
}
