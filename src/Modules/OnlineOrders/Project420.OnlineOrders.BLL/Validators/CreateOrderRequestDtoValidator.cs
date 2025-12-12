using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;

namespace Project420.OnlineOrders.BLL.Validators;

/// <summary>
/// FluentValidation validator for CreateOrderRequestDto.
/// Cannabis Act compliance: Validates order requirements.
/// </summary>
public class CreateOrderRequestDtoValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderRequestDtoValidator()
    {
        // Customer ID validation
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        // Order items validation
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= 50).WithMessage("Order cannot contain more than 50 items");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Product ID is required");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000");

            item.RuleFor(i => i.PriceAtTimeOfOrder)
                .GreaterThan(0).WithMessage("Price must be greater than R0.00")
                .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed R999,999.99");
        });

        // Pickup location validation
        RuleFor(x => x.PickupLocationId)
            .GreaterThan(0).WithMessage("Pickup location is required");

        // Pickup date validation
        RuleFor(x => x.PreferredPickupDate)
            .Must(date => !date.HasValue || date.Value >= DateTime.Today)
            .WithMessage("Preferred pickup date cannot be in the past")
            .When(x => x.PreferredPickupDate.HasValue);

        // Customer notes validation
        RuleFor(x => x.CustomerNotes)
            .MaximumLength(1000).WithMessage("Customer notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerNotes));
    }
}
