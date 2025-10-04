using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class CancelAppointmentValidator : AbstractValidator<CancelAppointmentRequestDto>
    {
        public CancelAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

            RuleFor(x => x.CancellationReason)
                .NotEmpty().WithMessage("Lý do hủy lịch không được để trống")
                .MaximumLength(500).WithMessage("Lý do hủy lịch không được vượt quá 500 ký tự");
        }
    }
}
