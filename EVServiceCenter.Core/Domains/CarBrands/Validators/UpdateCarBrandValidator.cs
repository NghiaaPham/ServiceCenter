using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.CarBrands.Validators
{
    public class UpdateCarBrandValidator : AbstractValidator<UpdateCarBrandRequestDto>
    {
        public UpdateCarBrandValidator()
        {
            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("ID thương hiệu không hợp lệ");

            RuleFor(x => x.BrandName)
                .NotEmpty().WithMessage("Tên thương hiệu không được để trống")
                .MaximumLength(100).WithMessage("Tên thương hiệu không được vượt quá 100 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-.,()&]+$").WithMessage("Tên thương hiệu chứa ký tự không hợp lệ");

            When(x => !string.IsNullOrEmpty(x.Country), () =>
            {
                RuleFor(x => x.Country)
                    .MaximumLength(100).WithMessage("Tên quốc gia không được vượt quá 100 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.LogoUrl), () =>
            {
                RuleFor(x => x.LogoUrl)
                    .Must(BeAValidUrl).WithMessage("URL logo không hợp lệ")
                    .MaximumLength(500).WithMessage("URL logo quá dài");
            });

            When(x => !string.IsNullOrEmpty(x.Website), () =>
            {
                RuleFor(x => x.Website)
                    .Must(BeAValidUrl).WithMessage("Website không hợp lệ")
                    .MaximumLength(200).WithMessage("Website URL quá dài");
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