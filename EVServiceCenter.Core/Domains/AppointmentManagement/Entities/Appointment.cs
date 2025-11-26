using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Entities
{

    public partial class Appointment
    {
        [Key]
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        [StringLength(20)]
        public string AppointmentCode { get; set; } = null!;

        [Column("CustomerID")]
        public int CustomerId { get; set; }

        [Column("VehicleID")]
        public int VehicleId { get; set; }

        [Column("ServiceCenterID")]
        public int ServiceCenterId { get; set; }

        [Column("ServiceID")]
        public int? ServiceId { get; set; }

        [Column("PackageID")]
        public int? PackageId { get; set; }

        /// <summary>
        /// Subscription ID - Liên kết với CustomerPackageSubscription
        /// Nếu customer dùng subscription để book appointment
        /// </summary>
        [Column("SubscriptionID")]
        public int? SubscriptionId { get; set; }

        [Column("SlotID")]
        public int? SlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public int? EstimatedDuration { get; set; }

        [Column(TypeName = "decimal(15, 2)")]
        public decimal? EstimatedCost { get; set; }

        /// <summary>
        /// Chi phí cuối cùng sau khi hoàn tất dịch vụ
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? FinalCost { get; set; }

        /// <summary>
        /// Discount amount applied to this appointment
        /// = OriginalTotal - FinalTotal (EstimatedCost)
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Type of discount applied: "None", "CustomerType", "Promotion"
        /// </summary>
        [StringLength(20)]
        public string? DiscountType { get; set; }

        /// <summary>
        /// Promotion ID if promotion was applied
        /// </summary>
        [Column("PromotionID")]
        public int? PromotionId { get; set; }

        [StringLength(1000)]
        public string? ServiceDescription { get; set; }

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }

    [Column("PreferredTechnicianID")]
    public int? PreferredTechnicianId { get; set; }

    [Column("StatusID")]
    public int StatusId { get; set; }

    /// <summary>
    /// Km khach khai bao khi dat lich (tuy chon)
    /// </summary>
    public int? CustomerReportedMileage { get; set; }

        /// <summary>
        /// Trạng thái thanh toán hiện tại của appointment
        /// </summary>
        [StringLength(20)]
        public string PaymentStatus { get; set; } = PaymentStatusEnum.Pending.ToString();

        /// <summary>
        /// Tổng số tiền khách đã thanh toán
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? PaidAmount { get; set; }

        /// <summary>
        /// Số lần tạo PaymentIntent cho appointment này
        /// </summary>
        public int PaymentIntentCount { get; set; }

        /// <summary>
        /// FK tới intent gần nhất
        /// </summary>
        [Column("LatestPaymentIntentID")]
        public int? LatestPaymentIntentId { get; set; }

        [StringLength(20)]
        public string? Priority { get; set; }

        [StringLength(50)]
        public string? Source { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        [StringLength(20)]
        public string? ConfirmationMethod { get; set; }

        [StringLength(50)]
        public string? ConfirmationStatus { get; set; }

        public bool? ReminderSent { get; set; }

        public DateTime? ReminderSentDate { get; set; }

        public bool? NoShowFlag { get; set; }

        /// <summary>
        /// Ngày appointment bị hủy (nếu có)
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column("RescheduledFromID")]
        public int? RescheduledFromId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Ngày giờ hoàn thành appointment
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// User ID người đánh dấu completed (Staff/Technician)
        /// </summary>
        public int? CompletedBy { get; set; }

        /// <summary>
        /// RowVersion for optimistic concurrency control
        /// Prevents double-complete and race conditions
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        [InverseProperty("RelatedAppointment")]
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

        [ForeignKey("CreatedBy")]
        [InverseProperty("AppointmentCreatedByNavigations")]
        public virtual User? CreatedByNavigation { get; set; }

        [ForeignKey("CustomerId")]
        [InverseProperty("Appointments")]
        public virtual Customer Customer { get; set; } = null!;

        [InverseProperty("RescheduledFrom")]
        public virtual ICollection<Appointment> InverseRescheduledFrom { get; set; } = new List<Appointment>();

        [ForeignKey("PackageId")]
        [InverseProperty("Appointments")]
        public virtual MaintenancePackage? Package { get; set; }

        [ForeignKey("SubscriptionId")]
        [InverseProperty("Appointments")]
        public virtual CustomerPackageSubscription? Subscription { get; set; }

        [ForeignKey("PromotionId")]
        [InverseProperty("Appointments")]
        public virtual Promotion? Promotion { get; set; }

        [ForeignKey("PreferredTechnicianId")]
        [InverseProperty("AppointmentPreferredTechnicians")]
        public virtual User? PreferredTechnician { get; set; }

        [ForeignKey("RescheduledFromId")]
        [InverseProperty("InverseRescheduledFrom")]
        public virtual Appointment? RescheduledFrom { get; set; }

        [ForeignKey("ServiceId")]
        [InverseProperty("Appointments")]
        public virtual MaintenanceService? Service { get; set; }

        [ForeignKey("ServiceCenterId")]
        [InverseProperty("Appointments")]
        public virtual ServiceCenter ServiceCenter { get; set; } = null!;

        [ForeignKey("SlotId")]
        [InverseProperty("Appointments")]
        public virtual TimeSlot? Slot { get; set; }

        [ForeignKey("StatusId")]
        [InverseProperty("Appointments")]
        public virtual AppointmentStatus Status { get; set; } = null!;

        [ForeignKey("UpdatedBy")]
        [InverseProperty("AppointmentUpdatedByNavigations")]
        public virtual User? UpdatedByNavigation { get; set; }

        [ForeignKey("CompletedBy")]
        [InverseProperty("AppointmentCompletedByNavigations")]
        public virtual User? CompletedByNavigation { get; set; }

        [ForeignKey("VehicleId")]
        [InverseProperty("Appointments")]
        public virtual CustomerVehicle Vehicle { get; set; } = null!;

        [InverseProperty("Appointment")]
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

        [InverseProperty("Appointment")]
        public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

        /// <summary>
        /// Danh sách audit logs cho appointment này
        /// Track mọi thay đổi ServiceSource của tất cả services trong appointment
        /// </summary>
        [InverseProperty("Appointment")]
        public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
            = new List<ServiceSourceAuditLog>();

        /// <summary>
        /// Danh sách payment transactions cho appointment này
        /// Track tất cả các giao dịch thanh toán bổ sung và refunds
        /// </summary>
        [InverseProperty("Appointment")]
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
            = new List<PaymentTransaction>();

        [InverseProperty("Appointment")]
        public virtual ICollection<PaymentIntent> PaymentIntents { get; set; }
            = new List<PaymentIntent>();

        [ForeignKey("LatestPaymentIntentId")]
        public virtual PaymentIntent? LatestPaymentIntent { get; set; }
    }
}
