using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ModelServicePricings.Validators
{
    public class ModelServicePricingQueryValidator : AbstractValidator<ModelServicePricingQueryDto>
    {
        private readonly string[] _allowedSortFields = new[] { "PricingId", "ModelName", "ServiceName", "CustomPrice", "EffectiveDate" };

        public ModelServicePricingQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");

            When(x => x.ModelId.HasValue, () =>
            {
                RuleFor(x => x.ModelId!.Value)
                    .GreaterThan(0).WithMessage("Model ID không hợp lệ");
            });

            When(x => x.ServiceId.HasValue, () =>
            {
                RuleFor(x => x.ServiceId!.Value)
                    .GreaterThan(0).WithMessage("Service ID không hợp lệ");
            });

            RuleFor(x => x.SortBy)
                .Must(value => _allowedSortFields.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Trường sắp xếp phải là một trong: {string.Join(", ", _allowedSortFields)}");

            RuleFor(x => x.SortOrder)
                .Must(value => value.ToLower() == "asc" || value.ToLower() == "desc")
                .WithMessage("Thứ tự sắp xếp phải là 'asc' hoặc 'desc'");
        }
    }
}