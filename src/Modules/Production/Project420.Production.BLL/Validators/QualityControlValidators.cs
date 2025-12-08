using FluentValidation;
using Project420.Production.BLL.DTOs;

namespace Project420.Production.BLL.Validators;

public class CreateQualityControlValidator : AbstractValidator<CreateQualityControlDto>
{
    public CreateQualityControlValidator()
    {
        RuleFor(x => x.ProductionBatchId)
            .GreaterThan(0).WithMessage("Valid production batch is required");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Check date cannot be in the future");

        RuleFor(x => x.Inspector)
            .NotEmpty().WithMessage("Inspector is required")
            .MaximumLength(100).WithMessage("Inspector cannot exceed 100 characters");

        RuleFor(x => x.TestResults)
            .MaximumLength(2000).WithMessage("Test results cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.TestResults));
    }
}

public class UpdateQualityControlValidator : AbstractValidator<UpdateQualityControlDto>
{
    public UpdateQualityControlValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid quality control ID is required");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required");

        RuleFor(x => x.Inspector)
            .NotEmpty().WithMessage("Inspector is required")
            .MaximumLength(100).WithMessage("Inspector cannot exceed 100 characters");
    }
}
