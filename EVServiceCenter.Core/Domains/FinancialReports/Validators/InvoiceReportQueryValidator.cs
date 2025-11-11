using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.FinancialReports.Validators;

/// <summary>
/// Validator for invoice report query parameters
/// </summary>
public class InvoiceReportQueryValidator : AbstractValidator<InvoiceReportQueryDto>
{
    public InvoiceReportQueryValidator()
    {
        // ✅ Enhancement: Start Date Validation
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .Must(BeAValidDate)
            .WithMessage("Start date must be a valid date")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Start date cannot be in the future")
            .GreaterThanOrEqualTo(new DateTime(2020, 1, 1))
            .WithMessage("Start date cannot be before 2020-01-01");

        // ✅ Enhancement: End Date Validation
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .Must(BeAValidDate)
            .WithMessage("End date must be a valid date")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("End date cannot be in the future");

        // ✅ Enhancement: Date Range Validation
        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days for performance reasons");

        // ✅ Enhancement: Status Validation
        RuleFor(x => x.Status)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "Pending", "Paid", "Cancelled", "PartiallyPaid", "Overdue" }.Contains(x))
            .WithMessage("Status must be one of: Pending, Paid, Cancelled, PartiallyPaid, Overdue");

        // ✅ Enhancement: Center ID Validation
        RuleFor(x => x.CenterId)
            .GreaterThan(0)
            .When(x => x.CenterId.HasValue)
            .WithMessage("Center ID must be greater than 0")
            .LessThan(10000)
            .When(x => x.CenterId.HasValue)
            .WithMessage("Center ID must be less than 10000");
    }

    private bool BeAValidDate(DateTime date)
    {
        return date != DateTime.MinValue && date != DateTime.MaxValue;
    }
}
