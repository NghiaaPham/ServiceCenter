using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Invoices.Validators;

public class SendInvoiceValidator : AbstractValidator<SendInvoiceRequestDto>
{
    public SendInvoiceValidator()
    {
        RuleFor(x => x.SendMethod)
            .NotEmpty()
            .WithMessage("SendMethod is required")
            .Must(method => new[] { "Email", "SMS", "Both" }
                .Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SendMethod must be one of: Email, SMS, Both");

        RuleFor(x => x.EmailAddress)
            .EmailAddress()
            .WithMessage("Invalid email address format")
            .MaximumLength(100)
            .WithMessage("EmailAddress cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.EmailAddress));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,15}$")
            .WithMessage("Phone number must be 10-15 digits")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .WithMessage("Message cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Message));

        // Business rule: Email required if SendMethod is Email or Both
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.EmailAddress) ||
                      !x.SendMethod.Equals("Email", StringComparison.OrdinalIgnoreCase) &&
                      !x.SendMethod.Equals("Both", StringComparison.OrdinalIgnoreCase))
            .WithMessage("EmailAddress is required when SendMethod is Email or Both")
            .When(x => string.IsNullOrEmpty(x.EmailAddress));

        // Business rule: Phone required if SendMethod is SMS or Both
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.PhoneNumber) ||
                      !x.SendMethod.Equals("SMS", StringComparison.OrdinalIgnoreCase) &&
                      !x.SendMethod.Equals("Both", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PhoneNumber is required when SendMethod is SMS or Both")
            .When(x => string.IsNullOrEmpty(x.PhoneNumber));
    }
}
