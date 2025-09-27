using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using FluentValidation;
namespace EVServiceCenter.Core.Domains.CustomerTypes.Validators
{
    public class CustomerTypeQueryValidator : AbstractValidator<CustomerTypeQueryDto>
    {
        public CustomerTypeQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Số trang phải lớn hơn 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("Số trang không được vượt quá 1000");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Kích thước trang phải lớn hơn 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Kích thước trang không được vượt quá 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự")
                .Matches("^[a-zA-ZÀ-ỹ0-9\\s\\-_]*$")
                .WithMessage("Từ khóa tìm kiếm chỉ được chứa chữ cái, số, khoảng trắng và dấu gạch ngang")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.SortBy)
                .Must(sortBy => IsValidSortField(sortBy))
                .WithMessage("Trường sắp xếp không hợp lệ. Các trường được phép: TypeName, DiscountPercent, IsActive");

            RuleFor(x => x.SortDesc)
                .NotNull()
                .WithMessage("Hướng sắp xếp là bắt buộc");

            RuleFor(x => x)
                .Must(query => !query.IncludeStats || query.PageSize <= 50)
                .WithMessage("Khi bao gồm thống kê, kích thước trang không được vượt quá 50")
                .When(x => x.IncludeStats);
        }

        private static bool IsValidSortField(string sortBy)
        {
            var validSortFields = new[] { "TypeName", "DiscountPercent", "IsActive" };
            return validSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
        }
    }
}
