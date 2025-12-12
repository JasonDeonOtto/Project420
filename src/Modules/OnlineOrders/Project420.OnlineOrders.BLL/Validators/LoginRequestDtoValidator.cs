using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;

namespace Project420.OnlineOrders.BLL.Validators;

/// <summary>
/// FluentValidation validator for LoginRequestDto.
/// </summary>
public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
