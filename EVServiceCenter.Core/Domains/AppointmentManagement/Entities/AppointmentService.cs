using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Entities
{
    [Table("AppointmentServices")]
    public partial class AppointmentService
    {
        [Key]
        [Column("AppointmentServiceID")]
        public int AppointmentServiceId { get; set; }

        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        [Column("ServiceID")]
        public int ServiceId { get; set; }

        /// <summary>
        /// ID của subscription được sử dụng cho service này (nếu ServiceSource = "Subscription")
        /// NULL cho services với ServiceSource = "Regular" hoặc "Extra"
        /// </summary>
        [Column("SubscriptionID")]
        public int? SubscriptionId { get; set; }

        [StringLength(20)]
        public string ServiceSource { get; set; } = "Extra";

        /// <summary>
        /// Original price before any discount
        /// Used for tracking discount amount
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// Final price after discount (what customer pays)
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Discount amount applied to this service
        /// = OriginalPrice - Price
        /// </summary>
        [Column(TypeName = "decimal(15, 2)")]
        public decimal? DiscountAmount { get; set; }

        public int EstimatedTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("AppointmentId")]
        [InverseProperty("AppointmentServices")]
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey("ServiceId")]
        [InverseProperty("AppointmentServices")]
        public virtual MaintenanceService Service { get; set; } = null!;

        /// <summary>
        /// Danh sách audit logs cho appointment service này
        /// Track mọi thay đổi về ServiceSource và Price (từ Subscription → Extra, Manual Adjust, Refund)
        /// </summary>
        [InverseProperty("AppointmentService")]
        public virtual ICollection<ServiceSourceAuditLog> ServiceSourceAuditLogs { get; set; }
            = new List<ServiceSourceAuditLog>();
    }
}