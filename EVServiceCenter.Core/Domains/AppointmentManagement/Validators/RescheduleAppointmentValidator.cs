using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentRequestDto>
    {
        public RescheduleAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

            RuleFor(x => x.NewSlotId)
                .GreaterThan(0).WithMessage("Slot thời gian mới không hợp lệ");

            When(x => !string.IsNullOrEmpty(x.Reason), () =>
            {
                RuleFor(x => x.Reason)
                    .MaximumLength(500).WithMessage("Lý do dời lịch không được vượt quá 500 ký tự");
            });
        }
    }
}
