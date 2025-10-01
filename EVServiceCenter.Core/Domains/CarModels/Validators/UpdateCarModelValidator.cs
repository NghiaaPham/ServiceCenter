using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.CarModels.Validators
{
    public class UpdateCarModelValidator : AbstractValidator<UpdateCarModelRequestDto>
    {
        public UpdateCarModelValidator()
        {
            RuleFor(x => x.ModelId)
                .GreaterThan(0).WithMessage("ID dòng xe không hợp lệ");

            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("Thương hiệu không hợp lệ");

            RuleFor(x => x.ModelName)
                .NotEmpty().WithMessage("Tên dòng xe không được để trống")
                .MaximumLength(100).WithMessage("Tên dòng xe không được vượt quá 100 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-.,()]+$").WithMessage("Tên dòng xe chứa ký tự không hợp lệ");

            // Same validations as Create
            When(x => x.Year.HasValue, () =>
            {
                RuleFor(x => x.Year)
                    .InclusiveBetween(2000, 2050).WithMessage("Năm sản xuất phải từ 2000 đến 2050");
            });

            When(x => x.BatteryCapacity.HasValue, () =>
            {
                RuleFor(x => x.BatteryCapacity)
                    .GreaterThan(0).WithMessage("Dung lượng pin phải lớn hơn 0")
                    .LessThanOrEqualTo(500).WithMessage("Dung lượng pin không hợp lý");
            });

            When(x => x.ServiceInterval.HasValue, () =>
            {
                RuleFor(x => x.ServiceInterval)
                    .GreaterThan(0).WithMessage("Chu kỳ bảo dưỡng phải lớn hơn 0");
            });

            When(x => !string.IsNullOrEmpty(x.ImageUrl), () =>
            {
                RuleFor(x => x.ImageUrl)
                    .Must(BeAValidUrl).WithMessage("URL hình ảnh không hợp lệ");
            });
        }

        private bool BeAValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}