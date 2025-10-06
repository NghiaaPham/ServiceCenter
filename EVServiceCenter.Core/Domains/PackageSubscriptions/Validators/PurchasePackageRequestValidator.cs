using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Validators
{
    public class PurchasePackageRequestValidator : AbstractValidator<PurchasePackageRequestDto>
    {
        public PurchasePackageRequestValidator()
        {
            // ========== PACKAGE ID ==========
            RuleFor(x => x.PackageId)
                .GreaterThan(0).WithMessage("PackageId phải > 0");

            // ========== VEHICLE ID ==========
            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("VehicleId phải > 0");

            // ========== PAYMENT METHOD ==========
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Phương thức thanh toán không được để trống")
                .MaximumLength(50).WithMessage("Phương thức thanh toán không được vượt quá 50 ký tự")
                .Must(BeValidPaymentMethod).WithMessage(
                    "Phương thức thanh toán không hợp lệ. Chỉ chấp nhận: Cash, BankTransfer, CreditCard, MoMo, ZaloPay");

            // ========== AMOUNT PAID ==========
            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Số tiền thanh toán phải > 0")
                .LessThanOrEqualTo(1000000000).WithMessage("Số tiền thanh toán không được vượt quá 1 tỷ VNĐ");

            // ========== PAYMENT TRANSACTION ID (optional) ==========
            When(x => !string.IsNullOrWhiteSpace(x.PaymentTransactionId), () =>
            {
                RuleFor(x => x.PaymentTransactionId)
                    .MaximumLength(200).WithMessage("Transaction ID không được vượt quá 200 ký tự");
            });

            // ========== CUSTOMER NOTES (optional) ==========
            When(x => !string.IsNullOrWhiteSpace(x.CustomerNotes), () =>
            {
                RuleFor(x => x.CustomerNotes)
                    .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự");
            });
        }

        private bool BeValidPaymentMethod(string? paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return false;

            var validMethods = new[]
            {
                "Cash", "BankTransfer", "CreditCard", "DebitCard",
                "MoMo", "ZaloPay", "VNPay", "ShopeePay"
            };

            return validMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
        }
    }
}
