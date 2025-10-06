using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Enums;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Validators
{
    public class UpdateMaintenancePackageRequestValidator : AbstractValidator<UpdateMaintenancePackageRequestDto>
    {
        public UpdateMaintenancePackageRequestValidator()
        {
            // ========== PACKAGE ID ==========
            RuleFor(x => x.PackageId)
                .GreaterThan(0).WithMessage("PackageId phải > 0");

            // ========== PACKAGE CODE ==========
            RuleFor(x => x.PackageCode)
                .NotEmpty().WithMessage("Mã gói không được để trống")
                .MaximumLength(50).WithMessage("Mã gói không được vượt quá 50 ký tự")
                .Matches(@"^[A-Z0-9-]+$").WithMessage("Mã gói chỉ chứa chữ in hoa, số và dấu gạch ngang");

            // ========== PACKAGE NAME ==========
            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("Tên gói không được để trống")
                .MaximumLength(200).WithMessage("Tên gói không được vượt quá 200 ký tự");

            // ========== DESCRIPTION ==========
            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự");
            });

            // ========== VALIDITY PERIOD ==========
            When(x => x.ValidityPeriodInDays.HasValue, () =>
            {
                RuleFor(x => x.ValidityPeriodInDays!.Value)
                    .GreaterThan(0).WithMessage("Thời hạn gói phải > 0 ngày")
                    .LessThanOrEqualTo(3650).WithMessage("Thời hạn gói không được vượt quá 3650 ngày (10 năm)");
            });

            // ========== VALIDITY MILEAGE ==========
            When(x => x.ValidityMileage.HasValue, () =>
            {
                RuleFor(x => x.ValidityMileage!.Value)
                    .GreaterThan(0).WithMessage("Số km hiệu lực phải > 0")
                    .LessThanOrEqualTo(1000000).WithMessage("Số km hiệu lực không được vượt quá 1,000,000 km");
            });

            // ========== AT LEAST ONE VALIDITY CONDITION ==========
            RuleFor(x => x)
                .Must(x => x.ValidityPeriodInDays.HasValue || x.ValidityMileage.HasValue)
                .WithMessage("Gói phải có ít nhất 1 điều kiện hết hạn (thời gian hoặc số km)");

            // ========== PRICING ==========
            RuleFor(x => x.TotalPriceAfterDiscount)
                .GreaterThanOrEqualTo(0).WithMessage("Giá gói không thể âm")
                .LessThanOrEqualTo(1000000000).WithMessage("Giá gói không được vượt quá 1 tỷ VNĐ");

            When(x => x.DiscountPercent.HasValue, () =>
            {
                RuleFor(x => x.DiscountPercent!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Phần trăm giảm giá phải >= 0%")
                    .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá không được > 100%");
            });

            // ========== IMAGE URL ==========
            When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), () =>
            {
                RuleFor(x => x.ImageUrl)
                    .MaximumLength(500).WithMessage("URL hình ảnh không được vượt quá 500 ký tự")
                    .Must(BeAValidUrl).WithMessage("URL hình ảnh không hợp lệ");
            });

            // ========== STATUS ==========
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ")
                .NotEqual(PackageStatusEnum.Deleted).WithMessage("Không thể update gói thành trạng thái Deleted");

            // ========== INCLUDED SERVICES ==========
            RuleFor(x => x.IncludedServices)
                .NotEmpty().WithMessage("Gói phải chứa ít nhất 1 dịch vụ")
                .Must(services => services.Count <= 50).WithMessage("Gói không được chứa quá 50 dịch vụ");

            RuleForEach(x => x.IncludedServices)
                .SetValidator(new PackageServiceItemRequestValidator());

            // ========== NO DUPLICATE SERVICES ==========
            RuleFor(x => x.IncludedServices)
                .Must(services =>
                {
                    if (services == null) return true;
                    var serviceIds = services.Select(s => s.ServiceId).ToList();
                    return serviceIds.Count == serviceIds.Distinct().Count();
                })
                .WithMessage("Không được có dịch vụ trùng lặp trong gói");
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
