using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Validators
{
    public class CreateCustomerVehicleValidator : AbstractValidator<CreateCustomerVehicleRequestDto>
    {
        public CreateCustomerVehicleValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Khách hàng không hợp lệ");

            RuleFor(x => x.ModelId)
                .GreaterThan(0).WithMessage("Dòng xe không hợp lệ");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("Biển số xe không được để trống")
                .MaximumLength(20).WithMessage("Biển số xe không được vượt quá 20 ký tự")
                .Matches(@"^[A-Z0-9\-\.]+$").WithMessage("Biển số xe chỉ chứa chữ in hoa, số, dấu gạch ngang và dấu chấm");

            When(x => !string.IsNullOrEmpty(x.Vin), () =>
            {
                RuleFor(x => x.Vin)
                    .Length(17).WithMessage("VIN phải có đúng 17 ký tự")
                    .Matches(@"^[A-HJ-NPR-Z0-9]{17}$").WithMessage("VIN không hợp lệ (không chứa I, O, Q)");
            });

            When(x => !string.IsNullOrEmpty(x.Color), () =>
            {
                RuleFor(x => x.Color)
                    .MaximumLength(50).WithMessage("Màu xe không được vượt quá 50 ký tự");
            });

            When(x => x.PurchaseDate.HasValue, () =>
            {
                RuleFor(x => x.PurchaseDate)
                    .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Ngày mua không được là ngày trong tương lai");
            });

            When(x => x.Mileage.HasValue, () =>
            {
                RuleFor(x => x.Mileage)
                    .GreaterThanOrEqualTo(0).WithMessage("Số km phải >= 0")
                    .LessThanOrEqualTo(1000000).WithMessage("Số km không hợp lý (>1,000,000 km)");
            });

            When(x => x.BatteryHealthPercent.HasValue, () =>
            {
                RuleFor(x => x.BatteryHealthPercent)
                    .InclusiveBetween(0, 100).WithMessage("Sức khỏe pin phải từ 0 đến 100%");
            });

            When(x => x.NextMaintenanceDate.HasValue && x.LastMaintenanceDate.HasValue, () =>
            {
                RuleFor(x => x.NextMaintenanceDate)
                    .GreaterThan(x => x.LastMaintenanceDate!.Value)
                    .WithMessage("Ngày bảo dưỡng tiếp theo phải sau ngày bảo dưỡng cuối");
            });

            When(x => x.NextMaintenanceMileage.HasValue && x.LastMaintenanceMileage.HasValue, () =>
            {
                RuleFor(x => x.NextMaintenanceMileage)
                    .GreaterThan(x => x.LastMaintenanceMileage!.Value)
                    .WithMessage("Số km bảo dưỡng tiếp theo phải lớn hơn số km bảo dưỡng cuối");
            });

            When(x => x.InsuranceExpiry.HasValue, () =>
            {
                RuleFor(x => x.InsuranceExpiry)
                    .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Bảo hiểm đã hết hạn");
            });

            When(x => x.RegistrationExpiry.HasValue, () =>
            {
                RuleFor(x => x.RegistrationExpiry)
                    .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Đăng kiểm đã hết hạn");
            });

            When(x => !string.IsNullOrEmpty(x.Notes), () =>
            {
                RuleFor(x => x.Notes)
                    .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự");
            });
        }
    }
}