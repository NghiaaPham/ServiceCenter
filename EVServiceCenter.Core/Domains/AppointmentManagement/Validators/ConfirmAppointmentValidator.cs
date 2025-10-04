using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class ConfirmAppointmentValidator : AbstractValidator<ConfirmAppointmentRequestDto>
    {
        public ConfirmAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

            RuleFor(x => x.ConfirmationMethod)
                .NotEmpty().WithMessage("Phương thức xác nhận không được để trống")
                .Must(m => new[] { "Phone", "Email", "SMS", "In-Person" }.Contains(m))
                .WithMessage("Phương thức xác nhận phải là Phone, Email, SMS hoặc In-Person");
        }
    }
}
