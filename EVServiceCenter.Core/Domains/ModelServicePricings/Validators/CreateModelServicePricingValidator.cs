using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ModelServicePricings.Validators
{
    public class CreateModelServicePricingValidator : AbstractValidator<CreateModelServicePricingRequestDto>
    {
        public CreateModelServicePricingValidator()
        {
            RuleFor(x => x.ModelId)
                .GreaterThan(0).WithMessage("Model ID không hợp lệ");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Service ID không hợp lệ");

            When(x => x.CustomPrice.HasValue, () =>
            {
                RuleFor(x => x.CustomPrice!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Giá tùy chỉnh phải >= 0");
            });

            When(x => x.CustomTime.HasValue, () =>
            {
                RuleFor(x => x.CustomTime!.Value)
                    .GreaterThan(0).WithMessage("Thời gian tùy chỉnh phải > 0 phút")
                    .LessThanOrEqualTo(480).WithMessage("Thời gian tùy chỉnh không được vượt quá 480 phút");
            });

            When(x => !string.IsNullOrEmpty(x.Notes), () =>
            {
                RuleFor(x => x.Notes)
                    .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự");
            });

            When(x => x.EffectiveDate.HasValue && x.ExpiryDate.HasValue, () =>
            {
                RuleFor(x => x.ExpiryDate)
                    .GreaterThan(x => x.EffectiveDate)
                    .WithMessage("Ngày hết hạn phải sau ngày hiệu lực");
            });

            // At least one of CustomPrice or CustomTime must be provided
            RuleFor(x => x)
                .Must(x => x.CustomPrice.HasValue || x.CustomTime.HasValue)
                .WithMessage("Phải có ít nhất giá tùy chỉnh hoặc thời gian tùy chỉnh");
        }
    }
}