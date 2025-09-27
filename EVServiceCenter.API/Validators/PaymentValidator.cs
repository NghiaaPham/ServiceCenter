using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class PaymentValidator : AbstractValidator<Payment>
{
    public PaymentValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.PaymentCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.InvoiceId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.MethodId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.PaymentCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.ProcessingFee).GreaterThanOrEqualTo(0).When(x => x.ProcessingFee.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.NetAmount).GreaterThanOrEqualTo(0).When(x => x.NetAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.PaymentDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.PaymentDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
        RuleFor(x => x.TransactionRef).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.BankRef).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Status).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.FailureReason).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.RefundAmount).GreaterThanOrEqualTo(0).When(x => x.RefundAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.RefundDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.RefundDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
        RuleFor(x => x.RefundReason).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ProcessedBy).GreaterThan(0).When(x => x.ProcessedBy.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.CreatedDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.CreatedDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
    }
}

