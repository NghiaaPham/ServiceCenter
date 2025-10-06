using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities
{
    /// <summary>
    /// Entity để track việc sử dụng services trong subscription
    /// VD: Gói có "Thay dầu x2 lần" → Track đã dùng bao nhiêu, còn bao nhiêu
    /// </summary>
    public partial class PackageServiceUsage
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        [Column("UsageID")]
        public int UsageId { get; set; }

        /// <summary>
        /// FK to CustomerPackageSubscription
        /// Subscription nào đang track
        /// </summary>
        [Column("SubscriptionID")]
        public int SubscriptionId { get; set; }

        /// <summary>
        /// FK to MaintenanceService
        /// Service nào trong gói
        /// </summary>
        [Column("ServiceID")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Tổng số lần được phép dùng (theo package)
        /// VD: Gói có "Thay dầu x2 lần" → TotalAllowedQuantity = 2
        /// </summary>
        public int TotalAllowedQuantity { get; set; }

        /// <summary>
        /// Số lần đã sử dụng
        /// VD: Đã thay dầu 1 lần → UsedQuantity = 1
        /// </summary>
        public int UsedQuantity { get; set; } = 0;

        /// <summary>
        /// Số lần còn lại (computed)
        /// RemainingQuantity = TotalAllowedQuantity - UsedQuantity
        /// VD: Còn 1 lần thay dầu
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// Lần sử dụng gần nhất
        /// </summary>
        public DateTime? LastUsedDate { get; set; }

        /// <summary>
        /// AppointmentID của lần sử dụng gần nhất
        /// </summary>
        [Column("LastUsedAppointmentID")]
        public int? LastUsedAppointmentId { get; set; }

        /// <summary>
        /// Notes (optional)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Navigation: Subscription
        /// </summary>
        [ForeignKey("SubscriptionId")]
        [InverseProperty("PackageServiceUsages")]
        public virtual CustomerPackageSubscription Subscription { get; set; } = null!;

        /// <summary>
        /// Navigation: Service
        /// </summary>
        [ForeignKey("ServiceId")]
        [InverseProperty("PackageServiceUsages")]
        public virtual MaintenanceService Service { get; set; } = null!;
    }
}
