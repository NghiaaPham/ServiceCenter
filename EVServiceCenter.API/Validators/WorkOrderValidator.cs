using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class WorkOrderValidator : AbstractValidator<WorkOrder>
{
    public WorkOrderValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.WorkOrderCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.ServiceCenterId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.StatusId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.WorkOrderCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.Priority).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.TechnicianId).GreaterThan(0).When(x => x.TechnicianId.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.AdvisorId).GreaterThan(0).When(x => x.AdvisorId.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.SupervisorId).GreaterThan(0).When(x => x.SupervisorId.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.EstimatedAmount).GreaterThanOrEqualTo(0).When(x => x.EstimatedAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0).When(x => x.TotalAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0).When(x => x.TaxAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.FinalAmount).GreaterThanOrEqualTo(0).When(x => x.FinalAmount.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0, 100).When(x => x.ProgressPercentage.HasValue).WithMessage(ValidationRules.Messages.InvalidRange);
        RuleFor(x => x.ChecklistCompleted).GreaterThanOrEqualTo(0).When(x => x.ChecklistCompleted.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ChecklistTotal).GreaterThanOrEqualTo(0).When(x => x.ChecklistTotal.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ApprovalNotes).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CustomerNotes).MaximumLength(1000).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.InternalNotes).MaximumLength(1000).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.TechnicianNotes).MaximumLength(1000).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EstimatedCompletionDate!.Value)
            .When(x => x.StartDate.HasValue && x.EstimatedCompletionDate.HasValue);
        RuleFor(x => x.EstimatedCompletionDate)
            .LessThanOrEqualTo(x => x.CompletedDate!.Value)
            .When(x => x.EstimatedCompletionDate.HasValue && x.CompletedDate.HasValue);
    }
}

