using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.InventoryManagement.Validators;

public class PartInventoryQueryValidator : AbstractValidator<PartInventoryQueryDto>
{
    public PartInventoryQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Page cannot exceed 1000");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("SearchTerm cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.ServiceCenterId)
            .GreaterThan(0)
            .WithMessage("ServiceCenterId must be greater than 0")
            .When(x => x.ServiceCenterId.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .WithMessage("SupplierId must be greater than 0")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "partcode", "partname", "currentstock", "lastupdated" }
                .Contains(sortBy.ToLowerInvariant()))
            .WithMessage("SortBy must be one of: partCode, partName, currentStock, lastUpdated");

        RuleFor(x => x.SortDirection)
            .Must(dir => dir.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                        dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'");
    }
}
