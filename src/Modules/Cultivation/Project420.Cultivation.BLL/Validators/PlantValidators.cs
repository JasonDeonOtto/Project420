using FluentValidation;
using Project420.Cultivation.BLL.DTOs;

namespace Project420.Cultivation.BLL.Validators;

public class CreatePlantValidator : AbstractValidator<CreatePlantDto>
{
    public CreatePlantValidator()
    {
        RuleFor(x => x.PlantTag)
            .NotEmpty().WithMessage("Plant tag is required")
            .MaximumLength(100).WithMessage("Plant tag cannot exceed 100 characters");

        RuleFor(x => x.GrowCycleId)
            .GreaterThan(0).WithMessage("Valid grow cycle is required");

        RuleFor(x => x.StrainName)
            .MaximumLength(200).WithMessage("Strain name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.StrainName));

        RuleFor(x => x.PlantedDate)
            .NotEmpty().WithMessage("Planted date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Planted date cannot be in the future");

        RuleFor(x => x.PlantSource)
            .MaximumLength(50).WithMessage("Plant source cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.PlantSource));

        RuleFor(x => x.PlantSex)
            .MaximumLength(50).WithMessage("Plant sex cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.PlantSex));
    }
}

public class UpdatePlantValidator : AbstractValidator<UpdatePlantDto>
{
    public UpdatePlantValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid plant ID is required");

        RuleFor(x => x.PlantTag)
            .NotEmpty().WithMessage("Plant tag is required")
            .MaximumLength(100).WithMessage("Plant tag cannot exceed 100 characters");

        RuleFor(x => x.HealthStatus)
            .MaximumLength(200).WithMessage("Health status cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.HealthStatus));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
