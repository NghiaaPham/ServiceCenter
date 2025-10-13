using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Validators
{
    /// <summary>
    /// Validator cho Purchase Package Request
    /// Validate input khi customer mua g�i d?ch v?
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
                .WithMessage("Ghi ch� kh�ng ???c v??t qu� 500 k� t?");
        }
    }
}
