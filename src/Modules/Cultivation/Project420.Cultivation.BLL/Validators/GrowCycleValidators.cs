using FluentValidation;
using Project420.Cultivation.BLL.DTOs;

namespace Project420.Cultivation.BLL.Validators;

public class CreateGrowCycleValidator : AbstractValidator<CreateGrowCycleDto>
{
    public CreateGrowCycleValidator()
    {
        RuleFor(x => x.CycleCode)
            .NotEmpty().WithMessage("Cycle code is required")
            .MaximumLength(50).WithMessage("Cycle code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.StrainName)
            .NotEmpty().WithMessage("Strain name is required")
            .MaximumLength(200).WithMessage("Strain name cannot exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.PlannedHarvestDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Planned harvest date must be after start date")
            .When(x => x.PlannedHarvestDate.HasValue);
    }
}

public class UpdateGrowCycleValidator : AbstractValidator<UpdateGrowCycleDto>
{
    public UpdateGrowCycleValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid grow cycle ID is required");

        RuleFor(x => x.CycleCode)
            .NotEmpty().WithMessage("Cycle code is required")
            .MaximumLength(50).WithMessage("Cycle code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.StrainName)
            .NotEmpty().WithMessage("Strain name is required")
            .MaximumLength(200).WithMessage("Strain name cannot exceed 200 characters");

        RuleFor(x => x.TotalPlantsStarted)
            .GreaterThanOrEqualTo(0).WithMessage("Total plants started cannot be negative");

        RuleFor(x => x.PlantsHarvested)
            .GreaterThanOrEqualTo(0).WithMessage("Plants harvested cannot be negative")
            .LessThanOrEqualTo(x => x.TotalPlantsStarted)
            .WithMessage("Plants harvested cannot exceed total plants started");

        RuleFor(x => x.TotalWetWeightGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Total wet weight cannot be negative")
            .When(x => x.TotalWetWeightGrams.HasValue);

        RuleFor(x => x.TotalDryWeightGrams)
            .GreaterThanOrEqualTo(0).WithMessage("Total dry weight cannot be negative")
            .When(x => x.TotalDryWeightGrams.HasValue);
    }
}
