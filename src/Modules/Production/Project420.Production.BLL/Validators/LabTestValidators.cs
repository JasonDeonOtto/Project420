using FluentValidation;
using Project420.Production.BLL.DTOs;

namespace Project420.Production.BLL.Validators;

public class CreateLabTestValidator : AbstractValidator<CreateLabTestDto>
{
    public CreateLabTestValidator()
    {
        RuleFor(x => x.ProductionBatchId)
            .GreaterThan(0).WithMessage("Valid production batch is required");

        RuleFor(x => x.LabName)
            .NotEmpty().WithMessage("Lab name is required")
            .MaximumLength(200).WithMessage("Lab name cannot exceed 200 characters");

        RuleFor(x => x.COANumber)
            .NotEmpty().WithMessage("COA number is required (Certificate of Analysis)")
            .MaximumLength(100).WithMessage("COA number cannot exceed 100 characters");

        RuleFor(x => x.SampleDate)
            .NotEmpty().WithMessage("Sample date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sample date cannot be in the future");

        RuleFor(x => x.ResultsDate)
            .GreaterThanOrEqualTo(x => x.SampleDate)
            .WithMessage("Results date must be on or after sample date")
            .When(x => x.ResultsDate.HasValue);

        RuleFor(x => x.LabCertificateNumber)
            .MaximumLength(100).WithMessage("Lab certificate number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LabCertificateNumber));

        RuleFor(x => x.THCPercentage)
            .MaximumLength(20).WithMessage("THC percentage cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.THCPercentage));

        RuleFor(x => x.CBDPercentage)
            .MaximumLength(20).WithMessage("CBD percentage cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.CBDPercentage));
    }
}

public class UpdateLabTestValidator : AbstractValidator<UpdateLabTestDto>
{
    public UpdateLabTestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid lab test ID is required");

        RuleFor(x => x.LabName)
            .NotEmpty().WithMessage("Lab name is required")
            .MaximumLength(200).WithMessage("Lab name cannot exceed 200 characters");

        RuleFor(x => x.COANumber)
            .NotEmpty().WithMessage("COA number is required")
            .MaximumLength(100).WithMessage("COA number cannot exceed 100 characters");

        RuleFor(x => x.SampleDate)
            .NotEmpty().WithMessage("Sample date is required");

        RuleFor(x => x.ResultsDate)
            .GreaterThanOrEqualTo(x => x.SampleDate)
            .WithMessage("Results date must be on or after sample date")
            .When(x => x.ResultsDate.HasValue);
    }
}
