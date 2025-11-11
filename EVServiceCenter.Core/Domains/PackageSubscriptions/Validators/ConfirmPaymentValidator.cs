using FluentValidation;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Validators
{
    public class ConfirmPaymentValidator : AbstractValidator<DTOs.Requests.ConfirmPaymentRequestDto>
    {
        public ConfirmPaymentValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .GreaterThan(0)
                .WithMessage("SubscriptionId ph?i l?n h?n 0");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .WithMessage("PaymentMethod là b?t bu?c")
                .Must(method => method == "Cash" || method == "BankTransfer")
                .WithMessage("PaymentMethod ph?i là 'Cash' ho?c 'BankTransfer'");

            RuleFor(x => x.PaidAmount)
                .GreaterThan(0)
                .WithMessage("S? ti?n ph?i l?n h?n 0");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Ghi chú không ???c v??t quá 1000 ký t?");

            // BankTransfer validation
            When(x => x.PaymentMethod == "BankTransfer", () =>
            {
                RuleFor(x => x.BankTransactionId)
                    .NotEmpty()
                    .WithMessage("Mã giao d?ch ngân hàng là b?t bu?c khi thanh toán qua BankTransfer")
                    .MaximumLength(100)
                    .WithMessage("Mã giao d?ch không ???c v??t quá 100 ký t?");

                RuleFor(x => x.TransferDate)
                    .NotNull()
                    .WithMessage("Ngày chuy?n kho?n là b?t bu?c khi thanh toán qua BankTransfer")
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("Ngày chuy?n kho?n không ???c là t??ng lai");
            });
        }
    }
}
