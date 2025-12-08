using FluentValidation;
using Project420.Cultivation.BLL.DTOs;

namespace Project420.Cultivation.BLL.Validators;

public class CreateHarvestBatchValidator : AbstractValidator<CreateHarvestBatchDto>
{
    public CreateHarvestBatchValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

        RuleFor(x => x.GrowCycleId)
            .GreaterThan(0).WithMessage("Valid grow cycle is required");

        RuleFor(x => x.StrainName)
            .NotEmpty().WithMessage("Strain name is required")
            .MaximumLength(200).WithMessage("Strain name cannot exceed 200 characters");

        RuleFor(x => x.HarvestDate)
            .NotEmpty().WithMessage("Harvest date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Harvest date cannot be in the future");

        RuleFor(x => x.TotalWetWeightGrams)
            .GreaterThan(0).WithMessage("Total wet weight must be greater than 0");

        RuleFor(x => x.PlantCount)
            .GreaterThan(0).WithMessage("Plant count must be greater than 0");
    }
}

public class UpdateHarvestBatchValidator : AbstractValidator<UpdateHarvestBatchDto>
{
    public UpdateHarvestBatchValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid harvest batch ID is required");

        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

        RuleFor(x => x.TotalDryWeightGrams)
            .GreaterThan(0).WithMessage("Total dry weight must be greater than 0")
            .When(x => x.TotalDryWeightGrams.HasValue);

        RuleFor(x => x.DryDate)
            .GreaterThanOrEqualTo(x => x.CureDate)
            .WithMessage("Dry date must be before or equal to cure date")
            .When(x => x.DryDate.HasValue && x.CureDate.HasValue);

        RuleFor(x => x.THCPercentage)
            .MaximumLength(20).WithMessage("THC percentage cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.THCPercentage));

        RuleFor(x => x.CBDPercentage)
            .MaximumLength(20).WithMessage("CBD percentage cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.CBDPercentage));

        RuleFor(x => x.LabTestCertificateNumber)
            .MaximumLength(100).WithMessage("Lab test certificate number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LabTestCertificateNumber));

        RuleFor(x => x.ProcessingStatus)
            .MaximumLength(100).WithMessage("Processing status cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ProcessingStatus));
    }
}
