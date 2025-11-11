using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.WorkOrders.Validators;

/// <summary>
/// Validator for recording vehicle health metrics
/// Ensures data quality for health tracking
/// </summary>
public class RecordVehicleHealthMetricRequestValidator : AbstractValidator<RecordVehicleHealthMetricRequestDto>
{
    public RecordVehicleHealthMetricRequestValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0)
            .WithMessage("Valid vehicle ID is required");

        RuleFor(x => x.BatteryHealth)
            .InclusiveBetween(0, 100)
            .When(x => x.BatteryHealth.HasValue)
            .WithMessage("Battery health must be between 0 and 100");

        RuleFor(x => x.MotorEfficiency)
            .InclusiveBetween(0, 100)
            .When(x => x.MotorEfficiency.HasValue)
            .WithMessage("Motor efficiency must be between 0 and 100");

        RuleFor(x => x.BrakeWear)
            .InclusiveBetween(0, 100)
            .When(x => x.BrakeWear.HasValue)
            .WithMessage("Brake wear must be between 0 and 100");

        RuleFor(x => x.TireWear)
            .InclusiveBetween(0, 100)
            .When(x => x.TireWear.HasValue)
            .WithMessage("Tire wear must be between 0 and 100");

        RuleFor(x => x.OverallCondition)
            .InclusiveBetween(0, 100)
            .When(x => x.OverallCondition.HasValue)
            .WithMessage("Overall condition must be between 0 and 100");

        RuleFor(x => x.DiagnosticCodes)
            .MaximumLength(500)
            .WithMessage("Diagnostic codes cannot exceed 500 characters");

        RuleFor(x => x.Recommendations)
            .MaximumLength(1000)
            .WithMessage("Recommendations cannot exceed 1000 characters");

        RuleFor(x => x.NextCheckDue)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.NextCheckDue.HasValue)
            .WithMessage("Next check due date cannot be in the past");

        // Business rule: At least one metric must be provided
        RuleFor(x => x)
            .Must(HaveAtLeastOneMetric)
            .WithMessage("At least one health metric must be provided");
    }

    private bool HaveAtLeastOneMetric(RecordVehicleHealthMetricRequestDto request)
    {
        return request.BatteryHealth.HasValue ||
               request.MotorEfficiency.HasValue ||
               request.BrakeWear.HasValue ||
               request.TireWear.HasValue ||
               request.OverallCondition.HasValue ||
               !string.IsNullOrWhiteSpace(request.DiagnosticCodes) ||
               !string.IsNullOrWhiteSpace(request.Recommendations);
    }
}
