using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities
{
    /// <summary>
    /// Payment transaction log - Track mọi giao dịch thanh toán
    /// Đặc biệt quan trọng cho degrade scenarios
    /// </summary>
    [Table("PaymentTransactions")]
    public partial class PaymentTransaction
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        [Column("TransactionID")]
        public int TransactionId { get; set; }

        /// <summary>
        /// FK to Appointment
        /// </summary>
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        /// <summary>
        /// FK to Customer
        /// </summary>
        [Column("CustomerID")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Số tiền giao dịch
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Loại tiền tệ (default: VND)
        /// </summary>
        [StringLength(3)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Phương thức thanh toán
        /// Values: Card, Cash, Transfer, Wallet, etc.
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Trạng thái giao dịch
        /// Values: Pending, Authorized, Captured, Failed, Refunded
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = null!;

        /// <summary>
        /// Transaction ID từ payment gateway
        /// </summary>
        [StringLength(100)]
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// Tên payment gateway
        /// Values: VNPay, Momo, Stripe, Paypal, etc.
        /// </summary>
        [StringLength(50)]
        public string? GatewayName { get; set; }

        /// <summary>
        /// Raw response từ payment gateway (JSON)
        /// Dùng cho debugging và dispute resolution
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// Thời gian tạo transaction
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian authorized (payment gateway chấp nhận)
        /// </summary>
        public DateTime? AuthorizedDate { get; set; }

        /// <summary>
        /// Thời gian captured (tiền đã được trừ)
        /// </summary>
        public DateTime? CapturedDate { get; set; }

        /// <summary>
        /// Thời gian failed (nếu thanh toán thất bại)
        /// </summary>
        public DateTime? FailedDate { get; set; }

        /// <summary>
        /// Thời gian refunded (nếu hoàn tiền)
        /// </summary>
        public DateTime? RefundedDate { get; set; }

        /// <summary>
        /// Error code từ payment gateway
        /// </summary>
        [StringLength(50)]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Ghi chú
        /// VD: "Additional charge due to service degradation"
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        // ========== NAVIGATION PROPERTIES ==========

        [ForeignKey("AppointmentId")]
        [InverseProperty("PaymentTransactions")]
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey("CustomerId")]
        [InverseProperty("PaymentTransactions")]
        public virtual Customer Customer { get; set; } = null!;
    }
}
