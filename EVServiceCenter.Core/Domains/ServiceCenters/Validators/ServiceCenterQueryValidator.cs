using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Validators
{
    public class ServiceCenterQueryValidator : AbstractValidator<ServiceCenterQueryDto>
    {
        private readonly string[] _allowedSortFields = new[]
        {
            "CenterName", "CenterCode", "Province", "District",
            "Capacity", "CreatedDate", "IsActive"
        };

        public ServiceCenterQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(200).WithMessage("Từ khóa tìm kiếm không được vượt quá 200 ký tự")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.SortBy)
                .Must(value => _allowedSortFields.Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Trường sắp xếp phải là một trong: {string.Join(", ", _allowedSortFields)}");

            RuleFor(x => x.SortOrder)
                .Must(value => value.ToLower() == "asc" || value.ToLower() == "desc")
                .WithMessage("Thứ tự sắp xếp phải là 'asc' hoặc 'desc'");
        }
    }
}