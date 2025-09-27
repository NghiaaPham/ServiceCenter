using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class AppointmentValidator : AbstractValidator<Appointment>
{
    public AppointmentValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.AppointmentCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.ServiceCenterId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
            RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.MinValue);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.AppointmentCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.EstimatedDuration).GreaterThan(0).When(x => x.EstimatedDuration.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ServiceDescription).MaximumLength(1000).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CustomerNotes).MaximumLength(1000).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.Priority).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Source).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ConfirmationMethod).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ConfirmationStatus).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CancellationReason).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CreatedBy).GreaterThan(0).When(x => x.CreatedBy.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
    }
}

