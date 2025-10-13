using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Validators
{
    /// <summary>
    /// Validator cho Purchase Package Request
    /// Validate input khi customer mua gói d?ch v?
    /// </summary>
    public class PurchasePackageValidator : AbstractValidator<PurchasePackageRequestDto>
    {
        public PurchasePackageValidator()
        {
            RuleFor(x => x.PackageId)
                .GreaterThan(0)
                .WithMessage("PackageId ph?i l?n h?n 0");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0)
                .WithMessage("VehicleId ph?i l?n h?n 0");

            RuleFor(x => x.CustomerNotes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.CustomerNotes))
                .WithMessage("Ghi chú không ???c v??t quá 500 ký t?");
        }
    }
}
