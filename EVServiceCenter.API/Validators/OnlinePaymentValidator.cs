using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class OnlinePaymentValidator : AbstractValidator<OnlinePayment>
{
    public OnlinePaymentValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.PaymentId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.GatewayTransactionId)
                .NotEmpty().WithMessage(ValidationRules.Messages.Required)
                .MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.GatewayTransactionId)
                .NotEmpty().WithMessage(ValidationRules.Messages.Required)
                .MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.GatewayName).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.PaymentStatus).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ResponseCode).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ResponseMessage).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ProcessingTime).GreaterThanOrEqualTo(0).When(x => x.ProcessingTime.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.Ipaddress).MaximumLength(45).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.UserAgent).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CreatedDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.CreatedDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
        RuleFor(x => x.UpdatedDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.UpdatedDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
    }
}

