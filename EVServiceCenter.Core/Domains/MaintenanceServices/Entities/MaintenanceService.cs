using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.MaintenanceServices.Entities;

public partial class MaintenanceService
{
    [Key]
    [Column("ServiceID")]
    public int ServiceId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(20)]
    public string ServiceCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string ServiceName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    // ✅ Nên NOT NULL
    [Required]
    public int StandardTime { get; set; } // minutes

    // ✅ Nên NOT NULL
    [Required]
    [Column(TypeName = "decimal(15, 2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? LaborCost { get; set; }

    [StringLength(50)]
    public string? SkillLevel { get; set; } // Should validate: Entry/Intermediate/Expert

    [StringLength(200)]
    public string? RequiredCertification { get; set; }

    public bool? IsWarrantyService { get; set; }

    public int? WarrantyPeriod { get; set; } // months - only if IsWarrantyService = true

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    // ✅ THÊM
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties - GIỮ NGUYÊN
    [InverseProperty("Service")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("CategoryId")]
    [InverseProperty("MaintenanceServices")]
    public virtual ServiceCategory Category { get; set; } = null!;

    [InverseProperty("Service")]
    public virtual ICollection<ChecklistTemplate> ChecklistTemplates { get; set; } = new List<ChecklistTemplate>();

    [InverseProperty("Service")]
    public virtual ICollection<ModelServicePricing> ModelServicePricings { get; set; } = new List<ModelServicePricing>();

    [InverseProperty("Service")]
    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();

    [InverseProperty("Service")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();

    [InverseProperty("Service")]
    public virtual ICollection<WorkOrderService> WorkOrderServices { get; set; } = new List<WorkOrderService>();

    [InverseProperty("Service")]
    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    /// <summary>
    /// Navigation: Track usage trong subscriptions
    /// </summary>
    [InverseProperty("Service")]
    public virtual ICollection<PackageServiceUsage> PackageServiceUsages { get; set; } = new List<PackageServiceUsage>();
}