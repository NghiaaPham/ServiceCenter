using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.WorkOrders.Validators;

/// <summary>
/// Validator for creating work order requests
/// Ensures business rules and data integrity
/// </summary>
public class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequestDto>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Valid customer ID is required");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0)
            .WithMessage("Valid vehicle ID is required");

        RuleFor(x => x.ServiceCenterId)
            .GreaterThan(0)
            .WithMessage("Valid service center ID is required");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .WithMessage("Priority must be one of: Low, Normal, High, Urgent");

        RuleFor(x => x.EstimatedCompletionDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.EstimatedCompletionDate.HasValue)
            .WithMessage("Estimated completion date cannot be in the past");

        RuleFor(x => x.CustomerNotes)
            .MaximumLength(1000)
            .WithMessage("Customer notes cannot exceed 1000 characters");

        RuleFor(x => x.InternalNotes)
            .MaximumLength(1000)
            .WithMessage("Internal notes cannot exceed 1000 characters");

        RuleFor(x => x.ServiceIds)
            .NotNull()
            .WithMessage("Service IDs list is required (can be empty)");
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return true; // Will use default "Normal"

        var validPriorities = new[] { "Low", "Normal", "High", "Urgent" };
        return validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase);
    }
}
