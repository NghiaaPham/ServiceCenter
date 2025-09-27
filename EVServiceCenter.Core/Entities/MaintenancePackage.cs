
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class MaintenancePackage
{
    [Key]
    [Column("PackageID")]
    public int PackageId { get; set; }

    [StringLength(20)]
    public string PackageCode { get; set; } = null!;

    [StringLength(100)]
    public string PackageName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ValidityPeriod { get; set; }

    public int? ValidityMileage { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalPrice { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsPopular { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [InverseProperty("Package")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Package")]
    public virtual ICollection<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; } = new List<CustomerPackageSubscription>();

    [InverseProperty("Package")]
    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
}
