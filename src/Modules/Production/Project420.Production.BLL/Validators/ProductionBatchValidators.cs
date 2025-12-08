using FluentValidation;
using Project420.Production.BLL.DTOs;

namespace Project420.Production.BLL.Validators;

public class CreateProductionBatchValidator : AbstractValidator<CreateProductionBatchDto>
{
    public CreateProductionBatchValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

        RuleFor(x => x.HarvestBatchNumber)
            .NotEmpty().WithMessage("Harvest batch number is required")
            .MaximumLength(100).WithMessage("Harvest batch number cannot exceed 100 characters");

        RuleFor(x => x.StrainName)
            .NotEmpty().WithMessage("Strain name is required")
            .MaximumLength(200).WithMessage("Strain name cannot exceed 200 characters");

        RuleFor(x => x.StartingWeightGrams)
            .GreaterThan(0).WithMessage("Starting weight must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Start date cannot be in the future");
    }
}

public class UpdateProductionBatchValidator : AbstractValidator<UpdateProductionBatchDto>
{
    public UpdateProductionBatchValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid production batch ID is required");

        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

        RuleFor(x => x.CurrentWeightGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Current weight cannot be negative")
            .When(x => x.CurrentWeightGrams.HasValue);

        RuleFor(x => x.FinalWeightGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Final weight cannot be negative")
            .When(x => x.FinalWeightGrams.HasValue);

        RuleFor(x => x.WasteWeightGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Waste weight cannot be negative")
            .When(x => x.WasteWeightGrams.HasValue);

        RuleFor(x => x.Status)
            .MaximumLength(100).WithMessage("Status cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.UnitsPackaged)
            .GreaterThan(0).WithMessage("Units packaged must be greater than 0")
            .When(x => x.UnitsPackaged.HasValue);
    }
}
