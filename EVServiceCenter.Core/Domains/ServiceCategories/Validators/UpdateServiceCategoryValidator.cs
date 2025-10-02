using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ServiceCategories.Validators
{
    public class UpdateServiceCategoryValidator : AbstractValidator<UpdateServiceCategoryRequestDto>
    {
        public UpdateServiceCategoryValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("ID loại dịch vụ không hợp lệ");

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Tên loại dịch vụ không được để trống")
                .MaximumLength(100).WithMessage("Tên loại dịch vụ không được vượt quá 100 ký tự");

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.IconUrl), () =>
            {
                RuleFor(x => x.IconUrl)
                    .MaximumLength(500).WithMessage("URL icon không được vượt quá 500 ký tự")
                    .Must(BeAValidUrl).WithMessage("URL icon không hợp lệ");
            });

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải >= 0");
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}