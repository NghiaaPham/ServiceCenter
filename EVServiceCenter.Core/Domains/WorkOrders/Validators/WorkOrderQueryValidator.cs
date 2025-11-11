using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.WorkOrders.Validators;

/// <summary>
/// Validator for work order query parameters
/// Ensures efficient database queries
/// </summary>
public class WorkOrderQueryValidator : AbstractValidator<WorkOrderQueryDto>
{
    public WorkOrderQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Invalid sort field. Valid values: CreatedDate, StartDate, EstimatedCompletionDate, Priority, Status");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection))
            .WithMessage("Sort direction must be 'asc' or 'desc'");

        RuleFor(x => x.StartDateFrom)
            .LessThanOrEqualTo(x => x.StartDateTo)
            .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue)
            .WithMessage("Start date 'from' must be before 'to'");

        RuleFor(x => x.CompletedDateFrom)
            .LessThanOrEqualTo(x => x.CompletedDateTo)
            .When(x => x.CompletedDateFrom.HasValue && x.CompletedDateTo.HasValue)
            .WithMessage("Completed date 'from' must be before 'to'");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
            .WithMessage("Priority must be one of: Low, Normal, High, Urgent");
    }

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return true;

        var validFields = new[]
        {
            "CreatedDate", "StartDate", "EstimatedCompletionDate",
            "Priority", "Status", "WorkOrderCode"
        };

        return validFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidSortDirection(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
            return true;

        return sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
               sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return true;

        var validPriorities = new[] { "Low", "Normal", "High", "Urgent" };
        return validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase);
    }
}
