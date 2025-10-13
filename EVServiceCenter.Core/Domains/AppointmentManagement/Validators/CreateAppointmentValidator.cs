using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using FluentValidation;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Validators
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequestDto>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Khách hàng không hợp lệ");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Xe không hợp lệ");

            RuleFor(x => x.ServiceCenterId)
                .GreaterThan(0).WithMessage("Trung tâm dịch vụ không hợp lệ");

            RuleFor(x => x.SlotId)
                .GreaterThan(0).WithMessage("Slot thời gian không hợp lệ");

            // ✅ SMART DEDUPLICATION: ServiceIds có thể empty nếu SubscriptionId được cung cấp
            // Customer có thể:
            // 1. Đặt lịch chỉ với Subscription (ServiceIds empty) → Dùng tất cả services từ gói
            // 2. Đặt lịch với Subscription + ServiceIds → Dùng gói trước, services ngoài gói tính thêm
            // 3. Đặt lịch với ServiceIds only (no Subscription) → Thanh toán đầy đủ
            RuleFor(x => x)
                .Must(x => x.SubscriptionId.HasValue ||
                           (x.ServiceIds != null && x.ServiceIds.Any()))
                .WithMessage("Phải chọn ít nhất một gói dịch vụ (Subscription) hoặc dịch vụ đơn lẻ");

            // Nếu có ServiceIds, validate từng ID phải > 0
            When(x => x.ServiceIds != null && x.ServiceIds.Any(), () =>
            {
                RuleFor(x => x.ServiceIds)
                    .Must(ids => ids.All(id => id > 0))
                    .WithMessage("ID dịch vụ không hợp lệ");
            });

            // Nếu có SubscriptionId, validate phải > 0
            When(x => x.SubscriptionId.HasValue, () =>
            {
                RuleFor(x => x.SubscriptionId)
                    .GreaterThan(0)
                    .WithMessage("Subscription ID không hợp lệ");
            });

            When(x => x.PackageId.HasValue, () =>
            {
                RuleFor(x => x.PackageId)
                    .GreaterThan(0).WithMessage("Gói dịch vụ không hợp lệ");
            });

            When(x => !string.IsNullOrEmpty(x.CustomerNotes), () =>
            {
                RuleFor(x => x.CustomerNotes)
                    .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự");
            });

            When(x => x.PreferredTechnicianId.HasValue, () =>
            {
                RuleFor(x => x.PreferredTechnicianId)
                    .GreaterThan(0).WithMessage("Kỹ thuật viên không hợp lệ");
            });

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Độ ưu tiên không được để trống")
                .Must(p => new[] { "Normal", "High", "Urgent" }.Contains(p))
                .WithMessage("Độ ưu tiên phải là Normal, High hoặc Urgent");

            RuleFor(x => x.Source)
                .NotEmpty().WithMessage("Nguồn đặt lịch không được để trống")
                .Must(s => new[] { "Online", "Walk-in", "Phone" }.Contains(s))
                .WithMessage("Nguồn phải là Online, Walk-in hoặc Phone");
        }
    }
}
