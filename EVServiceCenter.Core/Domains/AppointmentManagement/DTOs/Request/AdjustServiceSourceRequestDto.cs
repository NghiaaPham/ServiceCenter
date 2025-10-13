using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    /// <summary>
    /// DTO cho Admin điều chỉnh ServiceSource của AppointmentService
    /// Dùng để sửa lỗi hoặc hoàn tiền cho khách hàng
    /// </summary>
    public class AdjustServiceSourceRequestDto
    {
        /// <summary>
        /// ServiceSource mới
        /// Các giá trị hợp lệ: "Subscription", "Extra", "Regular"
        /// </summary>
        [Required(ErrorMessage = "NewServiceSource là bắt buộc")]
        [StringLength(20, ErrorMessage = "NewServiceSource không được vượt quá 20 ký tự")]
        public string NewServiceSource { get; set; } = null!;

        /// <summary>
        /// Giá mới (VNĐ)
        /// Phải >= 0
        /// </summary>
        [Required(ErrorMessage = "NewPrice là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "NewPrice phải >= 0")]
        public decimal NewPrice { get; set; }

        /// <summary>
        /// Lý do điều chỉnh (bắt buộc cho audit trail)
        /// Ví dụ: "Lỗi hệ thống, customer đã có gói", "Dịch vụ không đạt yêu cầu, hoàn tiền"
        /// </summary>
        [Required(ErrorMessage = "Reason là bắt buộc")]
        [StringLength(500, MinimumLength = 10,
            ErrorMessage = "Reason phải từ 10-500 ký tự để đảm bảo audit trail đầy đủ")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Có hoàn tiền cho customer không?
        /// TRUE: Tạo PaymentTransaction với status Refunded (nếu giá mới < giá cũ)
        /// FALSE: Chỉ điều chỉnh giá, không hoàn tiền
        /// </summary>
        public bool IssueRefund { get; set; } = false;
    }
}
