using EVServiceCenter.Core.Domains.Payments.Constants;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Payments.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequestDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0)
            .WithMessage("InvoiceId must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1_000_000_000) // 1 billion VND max
            .WithMessage("Amount cannot exceed 1,000,000,000 VND");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("PaymentMethod is required")
            .Must(BeValidPaymentMethod)
            .WithMessage("PaymentMethod must be one of: Cash, BankTransfer, VNPay, MoMo, Card");

        // ReturnUrl required for gateway payments
        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .When(x => IsGatewayPayment(x.PaymentMethod))
            .WithMessage("ReturnUrl is required for VNPay and MoMo payments")
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ReturnUrl))
            .WithMessage("ReturnUrl must be a valid URL");

        // TransactionRef optional but max length
        RuleFor(x => x.TransactionRef)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.TransactionRef))
            .WithMessage("TransactionRef cannot exceed 100 characters");

        // Notes optional but max length
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");

        // Email validation for gateway payments (optional but validated if provided)
        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("CustomerEmail must be a valid email address");

        // Phone validation (optional)
        RuleFor(x => x.CustomerPhone)
            .Matches(@"^\+?[0-9]{10,15}$")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone))
            .WithMessage("CustomerPhone must be 10-15 digits");
    }

    private static bool BeValidPaymentMethod(string? method)
    {
        if (string.IsNullOrEmpty(method)) return false;

        return method switch
        {
            PaymentMethodType.Cash => true,
            PaymentMethodType.BankTransfer => true,
            PaymentMethodType.VNPay => true,
            PaymentMethodType.MoMo => true,
            PaymentMethodType.Card => true,
            _ => false
        };
    }

    private static bool IsGatewayPayment(string? method)
    {
        return method == PaymentMethodType.VNPay || method == PaymentMethodType.MoMo;
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
