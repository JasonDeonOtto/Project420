using FluentValidation;
using Project420.Production.BLL.DTOs;

namespace Project420.Production.BLL.Validators;

public class CreateProcessingStepValidator : AbstractValidator<CreateProcessingStepDto>
{
    public CreateProcessingStepValidator()
    {
        RuleFor(x => x.ProductionBatchId)
            .GreaterThan(0).WithMessage("Valid production batch is required");

        RuleFor(x => x.StepDate)
            .NotEmpty().WithMessage("Step date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Step date cannot be in the future");

        RuleFor(x => x.PerformedBy)
            .NotEmpty().WithMessage("Performed by is required")
            .MaximumLength(100).WithMessage("Performed by cannot exceed 100 characters");

        RuleFor(x => x.StepDetails)
            .MaximumLength(1000).WithMessage("Step details cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.StepDetails));
    }
}

public class UpdateProcessingStepValidator : AbstractValidator<UpdateProcessingStepDto>
{
    public UpdateProcessingStepValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid processing step ID is required");

        RuleFor(x => x.StepDate)
            .NotEmpty().WithMessage("Step date is required");

        RuleFor(x => x.PerformedBy)
            .NotEmpty().WithMessage("Performed by is required")
            .MaximumLength(100).WithMessage("Performed by cannot exceed 100 characters");
    }
}
