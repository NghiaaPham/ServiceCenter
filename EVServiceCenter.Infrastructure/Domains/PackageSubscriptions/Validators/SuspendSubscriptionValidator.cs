using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Validators
{
    public class SuspendSubscriptionValidator : AbstractValidator<SuspendSubscriptionRequestDto>
    {
        public SuspendSubscriptionValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Lý do t?m d?ng là b?t bu?c")
                .MinimumLength(10)
                .WithMessage("Lý do t?m d?ng ph?i có ít nh?t 10 ký t?")
                .MaximumLength(500)
                .WithMessage("Lý do t?m d?ng không ???c v??t quá 500 ký t?");
        }
    }
}
