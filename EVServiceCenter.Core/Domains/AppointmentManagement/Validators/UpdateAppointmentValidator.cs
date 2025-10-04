using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentRequestDto>
    {
        public UpdateAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("ID lịch hẹn không hợp lệ");

            When(x => x.VehicleId.HasValue, () =>
            {
                RuleFor(x => x.VehicleId)
                    .GreaterThan(0).WithMessage("Xe không hợp lệ");
            });

            When(x => x.SlotId.HasValue, () =>
            {
                RuleFor(x => x.SlotId)
                    .GreaterThan(0).WithMessage("Slot thời gian không hợp lệ");
            });

            When(x => x.ServiceIds != null && x.ServiceIds.Any(), () =>
            {
                RuleFor(x => x.ServiceIds)
                    .Must(ids => ids.All(id => id > 0)).WithMessage("ID dịch vụ không hợp lệ");
            });

            When(x => !string.IsNullOrEmpty(x.CustomerNotes), () =>
            {
                RuleFor(x => x.CustomerNotes)
                    .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự");
            });

            When(x => !string.IsNullOrEmpty(x.ServiceDescription), () =>
            {
                RuleFor(x => x.ServiceDescription)
                    .MaximumLength(1000).WithMessage("Mô tả dịch vụ không được vượt quá 1000 ký tự");
            });

            When(x => x.PreferredTechnicianId.HasValue, () =>
            {
                RuleFor(x => x.PreferredTechnicianId)
                    .GreaterThan(0).WithMessage("Kỹ thuật viên không hợp lệ");
            });

            When(x => !string.IsNullOrEmpty(x.Priority), () =>
            {
                RuleFor(x => x.Priority)
                    .Must(p => new[] { "Normal", "High", "Urgent" }.Contains(p))
                    .WithMessage("Độ ưu tiên phải là Normal, High hoặc Urgent");
            });
        }
    }
}
