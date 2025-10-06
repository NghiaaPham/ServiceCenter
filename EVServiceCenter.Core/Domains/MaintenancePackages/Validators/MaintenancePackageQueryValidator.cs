using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Enums;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Validators
{
    public class MaintenancePackageQueryValidator : AbstractValidator<MaintenancePackageQueryDto>
    {
        private readonly string[] _validSortFields = new[]
        {
            "Price", "Name", "Discount", "Popular", "CreatedDate"
        };

        public MaintenancePackageQueryValidator()
        {
            // ========== PAGINATION ==========
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page phải > 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize phải > 0")
                .LessThanOrEqualTo(100).WithMessage("PageSize không được vượt quá 100");

            // ========== SEARCH TERM ==========
            When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(200).WithMessage("Từ khóa tìm kiếm không được vượt quá 200 ký tự");
            });

            // ========== STATUS FILTER ==========
            When(x => x.Status.HasValue, () =>
            {
                RuleFor(x => x.Status!.Value)
                    .IsInEnum().WithMessage("Trạng thái không hợp lệ");
            });

            // ========== PRICE RANGE ==========
            When(x => x.MinPrice.HasValue, () =>
            {
                RuleFor(x => x.MinPrice!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("MinPrice phải >= 0");
            });

            When(x => x.MaxPrice.HasValue, () =>
            {
                RuleFor(x => x.MaxPrice!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("MaxPrice phải >= 0");
            });

            When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.MinPrice <= x.MaxPrice)
                    .WithMessage("MinPrice không được lớn hơn MaxPrice");
            });

            // ========== DISCOUNT FILTER ==========
            When(x => x.MinDiscountPercent.HasValue, () =>
            {
                RuleFor(x => x.MinDiscountPercent!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("MinDiscountPercent phải >= 0%")
                    .LessThanOrEqualTo(100).WithMessage("MinDiscountPercent không được > 100%");
            });

            // ========== SORTING ==========
            When(x => !string.IsNullOrWhiteSpace(x.SortBy), () =>
            {
                RuleFor(x => x.SortBy)
                    .Must(sortBy => _validSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                    .WithMessage($"SortBy phải là một trong: {string.Join(", ", _validSortFields)}");
            });
        }
    }
}
