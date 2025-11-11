using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.Identity.Validators
{
  public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
  {
    public ForgotPasswordRequestDtoValidator()
    {
      RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Vui lòng nhập địa chỉ email.")
          .EmailAddress().WithMessage("Định dạng email không hợp lệ.");
    }
  }
}