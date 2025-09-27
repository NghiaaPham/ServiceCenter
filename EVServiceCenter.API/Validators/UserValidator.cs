using System.Text.RegularExpressions;
using EVServiceCenter.Core.Domains.Identity.Entities;

namespace EVServiceCenter.API.Validators;

public sealed class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleSet(ValidationRules.RuleSetCreate, () =>
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength).Must(x => x.Trim() == x);
            RuleFor(x => x.FullName).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
            RuleFor(x => x.RoleId).GreaterThan(0).WithMessage(ValidationRules.Messages.MustBePositive);
        });

        RuleSet(ValidationRules.RuleSetUpdate, () =>
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage(ValidationRules.Messages.Required).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        });

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage(ValidationRules.Messages.InvalidEmail)
            .MaximumLength(100).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.EmployeeCode).MaximumLength(20).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Department).MaximumLength(50).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.FailedLoginAttempts).GreaterThanOrEqualTo(0).When(x => x.FailedLoginAttempts.HasValue).WithMessage(ValidationRules.Messages.MustBeNonNegative);
        RuleFor(x => x.ProfilePicture).MaximumLength(500).WithMessage(ValidationRules.Messages.MaxLength);
        RuleFor(x => x.CreatedBy).GreaterThan(0).When(x => x.CreatedBy.HasValue).WithMessage(ValidationRules.Messages.MustBePositive);
        RuleFor(x => x.CreatedDate).LessThanOrEqualTo(DateTime.UtcNow).When(x => x.CreatedDate.HasValue).WithMessage(ValidationRules.Messages.NotInFuture);
    }
}

