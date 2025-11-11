using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Validators
{
    /// <summary>
    /// Validator cho PurchaseWithPaymentRequestDto
    /// </summary>
    public class PurchaseWithPaymentValidator : AbstractValidator<PurchaseWithPaymentRequestDto>
    {
        public PurchaseWithPaymentValidator()
        {
            RuleFor(x => x.PackageId)
                .GreaterThan(0)
                .WithMessage("PackageId ph?i l?n h?n 0");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0)
                .WithMessage("VehicleId ph?i l?n h?n 0");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .WithMessage("PaymentMethod là b?t bu?c")
                .Must(method => new[] { "VNPay", "MoMo", "Cash", "BankTransfer" }
                    .Contains(method, StringComparer.OrdinalIgnoreCase))
                .WithMessage("PaymentMethod ph?i là: VNPay, MoMo, Cash, ho?c BankTransfer");

            // ReturnUrl required for online payment methods
            When(x => x.PaymentMethod?.Equals("VNPay", StringComparison.OrdinalIgnoreCase) == true ||
                     x.PaymentMethod?.Equals("MoMo", StringComparison.OrdinalIgnoreCase) == true, () =>
            {
                RuleFor(x => x.ReturnUrl)
                    .NotEmpty()
                    .WithMessage("ReturnUrl là b?t bu?c khi thanh toán online")
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("ReturnUrl ph?i là URL h?p l?");
            });

            RuleFor(x => x.CustomerNotes)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.CustomerNotes))
                .WithMessage("CustomerNotes không ???c v??t quá 1000 ký t?");
        }
    }
}
