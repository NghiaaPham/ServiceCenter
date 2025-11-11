using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.FinancialReports.Validators;

/// <summary>
/// Validator for revenue report query parameters
/// </summary>
public class RevenueReportQueryValidator : AbstractValidator<RevenueReportQueryDto>
{
    public RevenueReportQueryValidator()
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
            .WithMessage("Start date cannot be before 2020-01-01 (system inception)");

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

        // ✅ Enhancement: Date Range Validation (Performance & Security)
        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days (performance limit)")
            .Must(x => (x.EndDate - x.StartDate).TotalDays >= 0)
            .WithMessage("Date range must be positive");

        // ✅ Enhancement: GroupBy Validation (Performance-aware)
        RuleFor(x => x.GroupBy)
            .NotEmpty()
            .WithMessage("GroupBy is required")
            .Must(x => new[] { "Daily", "Weekly", "Monthly" }.Contains(x))
            .WithMessage("GroupBy must be one of: Daily, Weekly, Monthly");

        // ✅ Enhancement: GroupBy + Date Range combination validation
        RuleFor(x => x)
            .Must(x => !(x.GroupBy == "Daily" && (x.EndDate - x.StartDate).TotalDays > 90))
            .WithMessage("Daily grouping limited to 90 days for performance reasons. Use Weekly or Monthly for longer periods.")
            .Must(x => !(x.GroupBy == "Weekly" && (x.EndDate - x.StartDate).TotalDays > 180))
            .WithMessage("Weekly grouping limited to 180 days for performance reasons. Use Monthly for longer periods.");

        // ✅ Enhancement: Payment Method Validation
        RuleFor(x => x.PaymentMethod)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "Cash", "BankTransfer", "VNPay", "MoMo", "Card" }.Contains(x))
            .WithMessage("Payment method must be one of: Cash, BankTransfer, VNPay, MoMo, Card");

        // ✅ Enhancement: Center ID Validation
        RuleFor(x => x.CenterId)
            .GreaterThan(0)
            .When(x => x.CenterId.HasValue)
            .WithMessage("Center ID must be greater than 0")
            .LessThan(10000)
            .When(x => x.CenterId.HasValue)
            .WithMessage("Center ID must be less than 10000 (sanity check)");
    }

    /// <summary>
    /// Validates that a date is not DateTime.MinValue or DateTime.MaxValue
    /// </summary>
    private bool BeAValidDate(DateTime date)
    {
        return date != DateTime.MinValue && date != DateTime.MaxValue;
    }
}
