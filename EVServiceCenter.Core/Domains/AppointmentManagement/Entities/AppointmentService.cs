using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;

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

        [StringLength(20)]
        public string ServiceSource { get; set; } = "Extra";

        [Column(TypeName = "decimal(15, 2)")]
        public decimal Price { get; set; }

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
    }
}