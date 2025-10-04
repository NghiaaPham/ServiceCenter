using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
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

        [Column("SlotID")]
        public int? SlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public int? EstimatedDuration { get; set; }

        [Column(TypeName = "decimal(15, 2)")]
        public decimal? EstimatedCost { get; set; }

        [StringLength(1000)]
        public string? ServiceDescription { get; set; }

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }

        [Column("PreferredTechnicianID")]
        public int? PreferredTechnicianId { get; set; }

        [Column("StatusID")]
        public int StatusId { get; set; }

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

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column("RescheduledFromID")]
        public int? RescheduledFromId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedBy { get; set; }

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

        [ForeignKey("VehicleId")]
        [InverseProperty("Appointments")]
        public virtual CustomerVehicle Vehicle { get; set; } = null!;

        [InverseProperty("Appointment")]
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

        [InverseProperty("Appointment")]
        public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    }
}