using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities
{
    /// <summary>
    /// Audit log để tracking mọi thay đổi ServiceSource của AppointmentService
    /// Dùng cho transparency, refund, và dispute resolution
    /// </summary>
    [Table("ServiceSourceAuditLog")]
    public partial class ServiceSourceAuditLog
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        [Column("AuditID")]
        public int AuditId { get; set; }

        /// <summary>
        /// FK to AppointmentService
        /// </summary>
        [Column("AppointmentServiceID")]
        public int AppointmentServiceId { get; set; }

        /// <summary>
        /// FK to Appointment (for easier querying)
        /// </summary>
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        /// <summary>
        /// FK to MaintenanceService
        /// </summary>
        [Column("ServiceID")]
        public int ServiceId { get; set; }

        /// <summary>
        /// FK to Customer (for customer-specific audit queries)
        /// </summary>
        [Column("CustomerID")]
        public int CustomerId { get; set; }

        /// <summary>
        /// ServiceSource trước khi thay đổi
        /// </summary>
        [StringLength(20)]
        public string? OldServiceSource { get; set; }

        /// <summary>
        /// ServiceSource sau khi thay đổi
        /// </summary>
        [StringLength(20)]
        public string NewServiceSource { get; set; } = null!;

        /// <summary>
        /// Giá trước khi thay đổi
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal OldPrice { get; set; }

        /// <summary>
        /// Giá sau khi thay đổi
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal NewPrice { get; set; }

        /// <summary>
        /// Chênh lệch giá (computed column)
        /// NewPrice - OldPrice
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal PriceDifference { get; set; }

        /// <summary>
        /// Lý do thay đổi (bắt buộc)
        /// VD: "Race condition: Subscription ran out of usage"
        /// </summary>
        [StringLength(1000)]
        public string ChangeReason { get; set; } = null!;

        /// <summary>
        /// Loại thay đổi
        /// Values: AUTO_DEGRADE, MANUAL_ADJUST, REFUND
        /// </summary>
        [StringLength(50)]
        public string ChangeType { get; set; } = null!;

        /// <summary>
        /// User ID người thực hiện thay đổi
        /// Null nếu là system auto-degrade
        /// </summary>
        public int? ChangedBy { get; set; }

        /// <summary>
        /// Thời gian thay đổi
        /// </summary>
        public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP Address của user (security tracking)
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent (browser/device info)
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Số tiền refund nếu có (cho REFUND change type)
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? RefundAmount { get; set; }

        /// <summary>
        /// Có issue refund không?
        /// </summary>
        public bool RefundIssued { get; set; } = false;

        /// <summary>
        /// Có trừ lượt subscription không? (cho MANUAL_ADJUST từ Extra → Subscription)
        /// </summary>
        public bool UsageDeducted { get; set; } = false;

        // ========== NAVIGATION PROPERTIES ==========

        [ForeignKey("AppointmentServiceId")]
        [InverseProperty("ServiceSourceAuditLogs")]
        public virtual AppointmentService AppointmentService { get; set; } = null!;

        [ForeignKey("AppointmentId")]
        [InverseProperty("ServiceSourceAuditLogs")]
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey("ServiceId")]
        [InverseProperty("ServiceSourceAuditLogs")]
        public virtual MaintenanceService Service { get; set; } = null!;

        /// <summary>
        /// NOTE: Không cần Customer navigation vì đã có qua Appointment.Customer
        /// </summary>

        [ForeignKey("ChangedBy")]
        [InverseProperty("ServiceSourceAuditLogs")]
        public virtual User? ChangedByUser { get; set; }
    }
}
