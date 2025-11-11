using EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Invoices.Validators;

public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceRequestDto>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0)
            .WithMessage("WorkOrderId must be greater than 0");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("DueDate cannot be in the past")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(200)
            .WithMessage("PaymentTerms cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.AdditionalDiscountPercent)
            .InclusiveBetween(0, 100)
            .WithMessage("AdditionalDiscountPercent must be between 0 and 100")
            .When(x => x.AdditionalDiscountPercent.HasValue);

        RuleFor(x => x.AdditionalDiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("AdditionalDiscountAmount cannot be negative")
            .When(x => x.AdditionalDiscountAmount.HasValue);

        // Business rule: Cannot have both percent and amount discount
        RuleFor(x => x)
            .Must(x => !x.AdditionalDiscountPercent.HasValue || !x.AdditionalDiscountAmount.HasValue)
            .WithMessage("Cannot apply both percentage and fixed amount discount");

        RuleFor(x => x.SendMethod)
            .Must(method => string.IsNullOrEmpty(method) || new[] { "Email", "SMS", "Both" }
                .Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SendMethod must be one of: Email, SMS, Both")
            .When(x => x.SendToCustomer);
    }
}
