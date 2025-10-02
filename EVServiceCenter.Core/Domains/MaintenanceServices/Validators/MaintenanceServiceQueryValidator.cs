using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.MaintenanceServices.Validators
{
    public class MaintenanceServiceQueryValidator : AbstractValidator<MaintenanceServiceQueryDto>
    {
        private readonly string[] _allowedSortFields = new[]
        {
            "ServiceName", "ServiceCode", "BasePrice", "StandardTime", "CategoryName", "CreatedDate"
        };

        public MaintenanceServiceQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(200).WithMessage("Từ khóa tìm kiếm không được vượt quá 200 ký tự");
            });

            When(x => x.CategoryId.HasValue, () =>
            {
                RuleFor(x => x.CategoryId!.Value)
                    .GreaterThan(0).WithMessage("ID loại dịch vụ không hợp lệ");
            });

            When(x => x.MinPrice.HasValue, () =>
            {
                RuleFor(x => x.MinPrice!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Giá tối thiểu phải >= 0");
            });

            When(x => x.MaxPrice.HasValue, () =>
            {
                RuleFor(x => x.MaxPrice!.Value)
                    .GreaterThan(0).WithMessage("Giá tối đa phải > 0");
            });

            When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue, () =>
            {
                RuleFor(x => x.MaxPrice)
                    .GreaterThanOrEqualTo(x => x.MinPrice)
                    .WithMessage("Giá tối đa phải >= giá tối thiểu");
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