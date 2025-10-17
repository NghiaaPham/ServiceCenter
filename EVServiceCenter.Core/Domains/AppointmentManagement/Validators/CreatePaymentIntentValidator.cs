using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
  public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentRequestDto>
  {
    public CreatePaymentIntentValidator()
    {
      RuleFor(x => x.AppointmentId)
          .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

      RuleFor(x => x.Currency)
          .NotEmpty().WithMessage("Đơn vị tiền tệ không được để trống")
          .Length(3).WithMessage("Đơn vị tiền tệ phải gồm 3 ký tự");

      RuleFor(x => x.Amount)
          .GreaterThan(0).When(x => x.Amount.HasValue)
          .WithMessage("Số tiền phải lớn hơn 0");

      RuleFor(x => x.ExpiresInHours)
          .GreaterThan(0).When(x => x.ExpiresInHours.HasValue)
          .WithMessage("Thời gian hết hạn phải lớn hơn 0 giờ");
    }
  }
}
