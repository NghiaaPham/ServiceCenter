using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.API.Validators
{
    public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequestDto>
    {
        public GoogleLoginRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty()
                .WithMessage(ErrorMessages.VALIDATION_ERROR + ": Google id_token is required.");
        }
    }

    public class FacebookLoginRequestValidator : AbstractValidator<FacebookLoginRequestDto>
    {
        public FacebookLoginRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage(ErrorMessages.VALIDATION_ERROR + ": Facebook access_token is required.");
        }
    }
}
