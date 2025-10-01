using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.CarModels.Validators
{
    public class CreateCarModelValidator : AbstractValidator<CreateCarModelRequestDto>
    {
        public CreateCarModelValidator()
        {
            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("Thương hiệu không hợp lệ");

            RuleFor(x => x.ModelName)
                .NotEmpty().WithMessage("Tên dòng xe không được để trống")
                .MaximumLength(100).WithMessage("Tên dòng xe không được vượt quá 100 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-.,()]+$").WithMessage("Tên dòng xe chứa ký tự không hợp lệ");

            When(x => x.Year.HasValue, () =>
            {
                RuleFor(x => x.Year)
                    .InclusiveBetween(2000, 2050).WithMessage("Năm sản xuất phải từ 2000 đến 2050");
            });

            When(x => x.BatteryCapacity.HasValue, () =>
            {
                RuleFor(x => x.BatteryCapacity)
                    .GreaterThan(0).WithMessage("Dung lượng pin phải lớn hơn 0")
                    .LessThanOrEqualTo(500).WithMessage("Dung lượng pin không hợp lý (>500 kWh)");
            });

            When(x => x.MaxRange.HasValue, () =>
            {
                RuleFor(x => x.MaxRange)
                    .GreaterThan(0).WithMessage("Quãng đường phải lớn hơn 0")
                    .LessThanOrEqualTo(2000).WithMessage("Quãng đường không hợp lý (>2000 km)");
            });

            When(x => x.MotorPower.HasValue, () =>
            {
                RuleFor(x => x.MotorPower)
                    .GreaterThan(0).WithMessage("Công suất động cơ phải lớn hơn 0")
                    .LessThanOrEqualTo(2000).WithMessage("Công suất không hợp lý (>2000 kW)");
            });

            When(x => x.AccelerationTime.HasValue, () =>
            {
                RuleFor(x => x.AccelerationTime)
                    .GreaterThan(0).WithMessage("Thời gian tăng tốc phải lớn hơn 0")
                    .LessThanOrEqualTo(30).WithMessage("Thời gian tăng tốc không hợp lý (>30s)");
            });

            When(x => x.TopSpeed.HasValue, () =>
            {
                RuleFor(x => x.TopSpeed)
                    .GreaterThan(0).WithMessage("Tốc độ tối đa phải lớn hơn 0")
                    .LessThanOrEqualTo(500).WithMessage("Tốc độ không hợp lý (>500 km/h)");
            });

            When(x => x.ServiceInterval.HasValue, () =>
            {
                RuleFor(x => x.ServiceInterval)
                    .GreaterThan(0).WithMessage("Chu kỳ bảo dưỡng phải lớn hơn 0");
            });

            When(x => x.ServiceIntervalMonths.HasValue, () =>
            {
                RuleFor(x => x.ServiceIntervalMonths)
                    .GreaterThan(0).WithMessage("Chu kỳ bảo dưỡng (tháng) phải lớn hơn 0")
                    .LessThanOrEqualTo(24).WithMessage("Chu kỳ bảo dưỡng không hợp lý (>24 tháng)");
            });

            When(x => x.WarrantyPeriod.HasValue, () =>
            {
                RuleFor(x => x.WarrantyPeriod)
                    .GreaterThan(0).WithMessage("Thời hạn bảo hành phải lớn hơn 0")
                    .LessThanOrEqualTo(120).WithMessage("Thời hạn bảo hành không hợp lý (>120 tháng)");
            });

            When(x => !string.IsNullOrEmpty(x.ImageUrl), () =>
            {
                RuleFor(x => x.ImageUrl)
                    .Must(BeAValidUrl).WithMessage("URL hình ảnh không hợp lệ")
                    .MaximumLength(500).WithMessage("URL hình ảnh quá dài");
            });

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự");
            });
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}