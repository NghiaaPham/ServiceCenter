using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ServiceCategories.Validators
{
    public class ServiceCategoryQueryValidator : AbstractValidator<ServiceCategoryQueryDto>
    {
        private readonly string[] _allowedSortFields = new[] { "CategoryName", "DisplayOrder", "CreatedDate" };

        public ServiceCategoryQueryValidator()
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

            RuleFor(x => x.SortBy)
                .Must(value => _allowedSortFields.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Trường sắp xếp phải là một trong: {string.Join(", ", _allowedSortFields)}");

            RuleFor(x => x.SortOrder)
                .Must(value => value.ToLower() == "asc" || value.ToLower() == "desc")
                .WithMessage("Thứ tự sắp xếp phải là 'asc' hoặc 'desc'");
        }
    }
}