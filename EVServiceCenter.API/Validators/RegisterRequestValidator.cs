using EVServiceCenter.Core.Enums;
using FluentValidation;
using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;

namespace EVServiceCenter.API.Validators
{
  public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
  {
    public RegisterRequestValidator()
    {
      RuleFor(r => r.Username)
          .NotEmpty().WithMessage(ErrorMessages.VALIDATION_ERROR + ": " + "Username is required.")
          .Length(SystemConstants.USERNAME_MIN_LENGTH, SystemConstants.USERNAME_MAX_LENGTH)
          .WithMessage(string.Format(ErrorMessages.VALIDATION_ERROR + ": Username must be between {0} and {1} characters.", SystemConstants.USERNAME_MIN_LENGTH, SystemConstants.USERNAME_MAX_LENGTH));

      RuleFor(r => r.Password)
          .NotEmpty().WithMessage(ErrorMessages.VALIDATION_ERROR + ": " + "Password is required.")
          .Length(SystemConstants.PASSWORD_MIN_LENGTH, SystemConstants.PASSWORD_MAX_LENGTH)
          .WithMessage(string.Format(ErrorMessages.VALIDATION_ERROR + ": Password must be between {0} and {1} characters.", SystemConstants.PASSWORD_MIN_LENGTH, SystemConstants.PASSWORD_MAX_LENGTH));

      RuleFor(r => r.FullName)
          .NotEmpty().WithMessage(ErrorMessages.VALIDATION_ERROR + ": " + "Full name is required.")
          .Length(1, SystemConstants.USERNAME_MAX_LENGTH)
          .WithMessage(string.Format(ErrorMessages.VALIDATION_ERROR + ": Full name must be up to {0} characters.", SystemConstants.USERNAME_MAX_LENGTH));

      RuleFor(r => r.Email)
          .EmailAddress().When(r => !string.IsNullOrEmpty(r.Email))
          .WithMessage(ErrorMessages.INVALID_EMAIL_FORMAT)
          .Length(0, SystemConstants.USERNAME_MAX_LENGTH)
          .WithMessage(string.Format(ErrorMessages.VALIDATION_ERROR + ": Email must be up to {0} characters.", SystemConstants.USERNAME_MAX_LENGTH));

      RuleFor(r => r.PhoneNumber)
          .Matches(ValidationConstants.VIETNAM_PHONE_PATTERN).When(r => !string.IsNullOrEmpty(r.PhoneNumber))
          .WithMessage(ErrorMessages.INVALID_PHONE_FORMAT)
          .Length(0, SystemConstants.USERNAME_MAX_LENGTH)
          .WithMessage(string.Format(ErrorMessages.VALIDATION_ERROR + ": Phone number must be up to {0} characters.", SystemConstants.USERNAME_MAX_LENGTH));

      RuleFor(r => r.RoleId)
          .Must(roleId => Enum.IsDefined(typeof(UserRoles), roleId))
          .WithMessage(ErrorMessages.VALIDATION_ERROR + ": " + "RoleId must be a valid UserRoles enum value.");
    }
  }
}