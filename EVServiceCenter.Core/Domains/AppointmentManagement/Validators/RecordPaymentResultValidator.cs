using System;
using System.Linq;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Enums;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
  public class RecordPaymentResultValidator : AbstractValidator<RecordPaymentResultRequestDto>
  {
    private static readonly string[] AllowedStatuses = Enum.GetNames(typeof(PaymentIntentStatusEnum));

    public RecordPaymentResultValidator()
    {
      RuleFor(x => x.AppointmentId)
          .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

      RuleFor(x => x.PaymentIntentId)
          .GreaterThan(0).WithMessage("PaymentIntentId không hợp lệ");

      RuleFor(x => x.Amount)
          .GreaterThanOrEqualTo(0).WithMessage("Số tiền phải lớn hơn hoặc bằng 0");

      RuleFor(x => x.Status)
          .NotEmpty().WithMessage("Trạng thái thanh toán không được để trống")
          .Must(status => AllowedStatuses.Contains(status))
          .WithMessage($"Trạng thái thanh toán phải thuộc một trong các giá trị: {nameof(PaymentIntentStatusEnum.Pending)}, {nameof(PaymentIntentStatusEnum.Completed)}, {nameof(PaymentIntentStatusEnum.Cancelled)}, {nameof(PaymentIntentStatusEnum.Expired)}, {nameof(PaymentIntentStatusEnum.Failed)}");

      RuleFor(x => x.Currency)
          .NotEmpty().WithMessage("Đơn vị tiền tệ không được để trống")
          .Length(3).WithMessage("Đơn vị tiền tệ phải gồm 3 ký tự (ISO-4217)");
    }
  }
}
