using FluentValidation;
using Project420.Cultivation.BLL.DTOs;

namespace Project420.Cultivation.BLL.Validators;

public class CreateGrowRoomValidator : AbstractValidator<CreateGrowRoomDto>
{
    public CreateGrowRoomValidator()
    {
        RuleFor(x => x.RoomCode)
            .NotEmpty().WithMessage("Room code is required")
            .MaximumLength(50).WithMessage("Room code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.SquareMeters)
            .GreaterThan(0).WithMessage("Square meters must be greater than 0")
            .When(x => x.SquareMeters.HasValue);

        RuleFor(x => x.MaxPlantCapacity)
            .GreaterThan(0).WithMessage("Max plant capacity must be greater than 0")
            .When(x => x.MaxPlantCapacity.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

public class UpdateGrowRoomValidator : AbstractValidator<UpdateGrowRoomDto>
{
    public UpdateGrowRoomValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid grow room ID is required");

        RuleFor(x => x.RoomCode)
            .NotEmpty().WithMessage("Room code is required")
            .MaximumLength(50).WithMessage("Room code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.SquareMeters)
            .GreaterThan(0).WithMessage("Square meters must be greater than 0")
            .When(x => x.SquareMeters.HasValue);

        RuleFor(x => x.MaxPlantCapacity)
            .GreaterThan(0).WithMessage("Max plant capacity must be greater than 0")
            .When(x => x.MaxPlantCapacity.HasValue);
    }
}
