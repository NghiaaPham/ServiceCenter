using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using System;
using System.Linq;

namespace EVServiceCenter.Core.Extensions
{
    /// <summary>
    /// Appointment domain helper / extension methods.
    /// ✅ Tập trung xử lý các rule tính toán phụ, giúp service code gọn hơn.
    /// </summary>
    public static class AppointmentExtensions
    {
        /// <summary>
        /// Tổng chi phí “phải trả” cho lịch hẹn.
        /// Ưu tiên:
        /// 1. FinalCost (nếu đã chốt giá),
        /// 2. EstimatedCost (nếu chưa chốt),
        /// 3. Fallback: sum từ AppointmentServices (bỏ qua dịch vụ từ Subscription).
        /// </summary>
        public static decimal GetTotalCost(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            // 1. Nếu đã có FinalCost → dùng luôn
            if (appointment.FinalCost.HasValue)
                return appointment.FinalCost.Value;

            // 2. Nếu chưa có FinalCost nhưng có EstimatedCost → dùng Estimated
            if (appointment.EstimatedCost.HasValue)
                return appointment.EstimatedCost.Value;

            // 3. Fallback: tính từ chi tiết dịch vụ (bỏ Subscription, vì Subscription = miễn phí)
            if (appointment.AppointmentServices != null && appointment.AppointmentServices.Any())
            {
                return appointment.AppointmentServices
                    .Where(s => !string.Equals(
                        s.ServiceSource,
                        "Subscription",
                        StringComparison.OrdinalIgnoreCase))
                    .Sum(s => s.Price);
            }

            return 0m;
        }

        /// <summary>
        /// Số tiền đã thanh toán cho lịch hẹn.
        /// </summary>
        public static decimal GetPaidAmount(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.PaidAmount ?? 0m;
        }

        /// <summary>
        /// Số tiền còn nợ (outstanding) = TotalCost - PaidAmount, không âm.
        /// </summary>
        public static decimal GetOutstandingAmount(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            var total = appointment.GetTotalCost();
            var paid = appointment.GetPaidAmount();

            var outstanding = total - paid;
            return outstanding > 0 ? outstanding : 0m;
        }

        /// <summary>
        /// Lịch hẹn có đang còn nợ hay không.
        /// </summary>
        public static bool HasOutstandingBalance(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.GetOutstandingAmount() > 0;
        }

        /// <summary>
        /// Có bất kỳ dịch vụ nào từ gói Subscription hay không.
        /// </summary>
        public static bool HasSubscriptionServices(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.AppointmentServices != null &&
                   appointment.AppointmentServices.Any(s =>
                       string.Equals(s.ServiceSource, "Subscription", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Có dịch vụ Extra (phát sinh ngoài gói) hay không.
        /// </summary>
        public static bool HasExtraServices(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.AppointmentServices != null &&
                   appointment.AppointmentServices.Any(s =>
                       string.Equals(s.ServiceSource, "Extra", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Có dịch vụ Regular (không thuộc gói, không phát sinh) hay không.
        /// </summary>
        public static bool HasRegularServices(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.AppointmentServices != null &&
                   appointment.AppointmentServices.Any(s =>
                       string.Equals(s.ServiceSource, "Regular", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Appointment đã ở quá khứ chưa (so với thời điểm nowUtc).
        /// </summary>
        public static bool IsPast(this Appointment appointment, DateTime? nowUtc = null)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            var now = nowUtc ?? DateTime.UtcNow;
            return appointment.AppointmentDate < now;
        }

        /// <summary>
        /// Có thể check-in hay không theo Option B:
        /// - Chỉ yêu cầu trạng thái Confirmed.
        /// - Không ép phải thanh toán đủ tại thời điểm check-in.
        /// </summary>
        public static bool CanCheckIn(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return appointment.StatusId == (int)AppointmentStatusEnum.Confirmed;
        }

        /// <summary>
        /// Có thể chuyển sang InProgress/hoàn tất quy trình tại tầng Appointment hay không.
        /// (Dùng cho Validate trước khi gọi CheckIn/CompleteAppointment).
        /// </summary>
        public static bool CanBeCompletedFromAppointmentPerspective(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            // Ở layer Appointment: chỉ cần đang InProgress.
            // Việc bắt buộc thanh toán full nên đặt ở WorkOrder/Delivery flow.
            return appointment.StatusId == (int)AppointmentStatusEnum.InProgress;
        }

        /// <summary>
        /// Appointment có đang ở trạng thái “active booking” không
        /// (phục vụ conflict check cho xe, kỹ thuật viên, service center).
        /// </summary>
        public static bool IsActiveBooking(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return AppointmentStatusHelper.IsActiveBooking(appointment.StatusId);
        }

        /// <summary>
        /// Lấy khoảng thời gian thực hiện (Start - End) dựa trên AppointmentDate + EstimatedDuration.
        /// Hữu ích cho UI hiển thị / conflict check / báo cáo.
        /// </summary>
        public static (DateTime StartUtc, DateTime EndUtc) GetTimeWindow(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            var start = appointment.AppointmentDate;
            var duration = appointment.EstimatedDuration ?? 60;
            var end = start.AddMinutes(duration);

            return (start, end);
        }

        /// <summary>
        /// Friendly status cho UI / API response nếu muốn show text tiếng Việt.
        /// (Không bắt buộc sử dụng, nhưng tiện cho mapping ở tầng ViewModel).
        /// </summary>
        public static string GetFriendlyStatus(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            var status = (AppointmentStatusEnum)appointment.StatusId;

            return status switch
            {
                AppointmentStatusEnum.Pending => "Chờ xác nhận",
                AppointmentStatusEnum.Confirmed => "Đã xác nhận",
                AppointmentStatusEnum.InProgress => "Đang thực hiện",
                AppointmentStatusEnum.Completed => "Hoàn tất",
                AppointmentStatusEnum.CompletedWithUnpaidBalance => "Hoàn tất (còn công nợ)",
                AppointmentStatusEnum.Cancelled => "Đã hủy",
                AppointmentStatusEnum.Rescheduled => "Đã dời lịch",
                AppointmentStatusEnum.NoShow => "Khách không đến",
                _ => $"Trạng thái không xác định ({appointment.StatusId})"
            };
        }

        /// <summary>
        /// PaymentStatus có đang ở trạng thái “đã thanh toán đủ” hay không.
        /// Giúp UI/BE đọc nhanh mà không cần dựa vào PaidAmount.
        /// </summary>
        public static bool IsPaymentCompleted(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return string.Equals(
                appointment.PaymentStatus,
                PaymentStatusEnum.Completed.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// PaymentStatus có đang ở trạng thái “đang chờ thanh toán” hay không.
        /// </summary>
        public static bool IsPaymentPending(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            return string.Equals(
                appointment.PaymentStatus,
                PaymentStatusEnum.Pending.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        public static decimal GetPaymentCompletionRate(this Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            var totalCost = appointment.GetTotalCost();
            if (totalCost <= 0)
                return 100m;

            var outstanding = appointment.GetOutstandingAmount();
            var paid = totalCost - outstanding;

            var rawRate = (paid / totalCost) * 100m;

            if (rawRate < 0m) rawRate = 0m;
            if (rawRate > 100m) rawRate = 100m;

            return Math.Round(rawRate, 2);
        }

    }
}
