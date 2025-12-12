using FluentValidation;
using Project420.OnlineOrders.BLL.DTOs.Request;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.Validators;

/// <summary>
/// FluentValidation validator for PaymentRequestDto.
/// Validates payment provider selection and URLs.
/// </summary>
public class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestDtoValidator()
    {
        // Order ID validation
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required");

        // Payment provider validation
        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Invalid payment provider")
            .Must(BeASupportedProvider).WithMessage("Payment provider is not supported");

        // Return URL validation
        RuleFor(x => x.ReturnUrl)
            .NotEmpty().WithMessage("Return URL is required")
            .Must(BeAValidUrl).WithMessage("Return URL must be a valid URL");

        // Cancel URL validation
        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithMessage("Cancel URL is required")
            .Must(BeAValidUrl).WithMessage("Cancel URL must be a valid URL");

        // Notify URL validation
        RuleFor(x => x.NotifyUrl)
            .NotEmpty().WithMessage("Notify URL is required")
            .Must(BeAValidUrl).WithMessage("Notify URL must be a valid URL");
    }

    /// <summary>
    /// Validates if payment provider is supported.
    /// Currently supported: Yoco, PayFast, Ozow (all SA providers).
    /// </summary>
    private bool BeASupportedProvider(PaymentProvider provider)
    {
        return provider == PaymentProvider.Yoco ||
               provider == PaymentProvider.PayFast ||
               provider == PaymentProvider.Ozow;
    }

    /// <summary>
    /// Validates if string is a valid URL.
    /// </summary>
    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
