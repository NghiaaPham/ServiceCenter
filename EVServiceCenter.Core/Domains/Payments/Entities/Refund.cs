using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.Payments.Entities
{
    /// <summary>
    /// Entity tracking refund requests and processing
    /// Xử lý hoàn tiền khi cancel appointment hoặc điều chỉnh giá
    /// </summary>
    [Table("Refunds")]
    public class Refund
    {
        [Key]
        [Column("RefundID")]
        public int RefundId { get; set; }

        /// <summary>
        /// PaymentIntent gốc cần hoàn tiền
        /// </summary>
        [Column("PaymentIntentID")]
        public int PaymentIntentId { get; set; }

        /// <summary>
        /// Appointment liên quan
        /// </summary>
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        /// <summary>
        /// Customer nhận refund
        /// </summary>
        [Column("CustomerID")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Số tiền hoàn lại
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Lý do hoàn tiền
        /// </summary>
        [Required]
        [StringLength(500)]
        public string RefundReason { get; set; } = null!;

        /// <summary>
        /// Phương thức hoàn tiền
        /// Original - hoàn về phương thức gốc
        /// BankTransfer - chuyển khoản
        /// Cash - tiền mặt
        /// </summary>
        [StringLength(50)]
        public string RefundMethod { get; set; } = "Original";

        /// <summary>
        /// Trạng thái refund
        /// Pending - chờ xử lý
        /// Processing - đang xử lý
        /// Completed - đã hoàn
        /// Failed - thất bại
        /// Cancelled - hủy refund
        /// </summary>
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Ngày tạo refund request
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Người tạo refund request
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Ngày xử lý refund thành công
        /// </summary>
        public DateTime? ProcessedDate { get; set; }

        /// <summary>
        /// Người xử lý refund
        /// </summary>
        public int? ProcessedBy { get; set; }

        /// <summary>
        /// Gateway Refund ID (từ payment gateway)
        /// </summary>
        [StringLength(200)]
        public string? GatewayRefundId { get; set; }

        /// <summary>
        /// Response từ payment gateway
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Error message nếu refund failed
        /// </summary>
        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Navigation properties
        [ForeignKey("PaymentIntentId")]
        public virtual PaymentIntent PaymentIntent { get; set; } = null!;

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; } = null!;
    }
}
