using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Entities
{
    [Table("ServiceCenters")]
    public partial class ServiceCenter
    {
        [Key]
        [Column("CenterID")]
        public int CenterId { get; set; }

        [Required]
        [StringLength(20)]
        public string CenterCode { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string CenterName { get; set; } = null!;

        [StringLength(200)]  
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Ward { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [Column("ManagerID")]
        public int? ManagerId { get; set; }

        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }

        public int? Capacity { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Facilities { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column("Latitude", TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column("Longitude", TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [InverseProperty("ServiceCenter")]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [InverseProperty("Center")]
        public virtual ICollection<DailyMetric> DailyMetrics { get; set; } = new List<DailyMetric>();

        [InverseProperty("Center")]
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

        [ForeignKey("ManagerId")]
        [InverseProperty("ServiceCenters")]
        public virtual User? Manager { get; set; }

        [InverseProperty("Center")]
        public virtual ICollection<PartInventory> PartInventories { get; set; } = new List<PartInventory>();

        [InverseProperty("Center")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

        [InverseProperty("Center")]
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        [InverseProperty("Center")]
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        [InverseProperty("Center")]
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

        [InverseProperty("Center")]
        public virtual ICollection<TechnicianSchedule> TechnicianSchedules { get; set; } = new List<TechnicianSchedule>();

        [InverseProperty("Center")]
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

        [InverseProperty("ServiceCenter")]
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}