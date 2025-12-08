using FluentValidation;
using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Validators;

public class CreateStockTransferValidator : AbstractValidator<CreateStockTransferDto>
{
    public CreateStockTransferValidator()
    {
        RuleFor(x => x.TransferNumber)
            .NotEmpty().WithMessage("Transfer number is required")
            .MaximumLength(100).WithMessage("Transfer number cannot exceed 100 characters");

        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("Transfer date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transfer date cannot be in the future");

        RuleFor(x => x.FromLocation)
            .NotEmpty().WithMessage("From location is required")
            .MaximumLength(200).WithMessage("From location cannot exceed 200 characters");

        RuleFor(x => x.ToLocation)
            .NotEmpty().WithMessage("To location is required")
            .MaximumLength(200).WithMessage("To location cannot exceed 200 characters")
            .NotEqual(x => x.FromLocation).WithMessage("To location must be different from From location");

        RuleFor(x => x.RequestedBy)
            .MaximumLength(100).WithMessage("Requested by cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.RequestedBy));
    }
}

public class UpdateStockTransferValidator : AbstractValidator<UpdateStockTransferDto>
{
    public UpdateStockTransferValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid stock transfer ID is required");

        RuleFor(x => x.TransferNumber)
            .NotEmpty().WithMessage("Transfer number is required")
            .MaximumLength(100).WithMessage("Transfer number cannot exceed 100 characters");

        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("Transfer date is required");

        RuleFor(x => x.FromLocation)
            .NotEmpty().WithMessage("From location is required")
            .MaximumLength(200).WithMessage("From location cannot exceed 200 characters");

        RuleFor(x => x.ToLocation)
            .NotEmpty().WithMessage("To location is required")
            .MaximumLength(200).WithMessage("To location cannot exceed 200 characters")
            .NotEqual(x => x.FromLocation).WithMessage("To location must be different from From location");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.AuthorizedBy)
            .MaximumLength(100).WithMessage("Authorized by cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.AuthorizedBy));
    }
}
