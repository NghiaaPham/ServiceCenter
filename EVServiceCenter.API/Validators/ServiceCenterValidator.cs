using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class ServiceCenterValidator : AbstractValidator<ServiceCenter>
{
    public ServiceCenterValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.CenterCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.CenterName).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.CenterCode).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.Address).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.City).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.State).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.PostalCode).MaximumLength(10).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ContactPhone).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail)).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.ManagerId).GreaterThan(0).When(x => x.ManagerId.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.ImageUrl).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Gpslatitude).InclusiveBetween(-90, 90).When(x => x.Gpslatitude.HasValue).WithMessage(ValidationRules.Messages.InvalidRange);
        RuleFor(x => x.Gpslongitude).InclusiveBetween(-180, 180).When(x => x.Gpslongitude.HasValue).WithMessage(ValidationRules.Messages.InvalidRange);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
    }
}

