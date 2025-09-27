using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class CustomerVehicle
{
    [Key]
    [Column("VehicleID")]
    public int VehicleId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("ModelID")]
    public int ModelId { get; set; }

    [StringLength(20)]
    public string LicensePlate { get; set; } = null!;

    [Column("VIN")]
    [StringLength(50)]
    public string? Vin { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public int? Mileage { get; set; }

    public DateOnly? LastMaintenanceDate { get; set; }

    public DateOnly? NextMaintenanceDate { get; set; }

    public int? LastMaintenanceMileage { get; set; }

    public int? NextMaintenanceMileage { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? BatteryHealthPercent { get; set; }

    [StringLength(50)]
    public string? VehicleCondition { get; set; }

    [StringLength(50)]
    public string? InsuranceNumber { get; set; }

    public DateOnly? InsuranceExpiry { get; set; }

    public DateOnly? RegistrationExpiry { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    [InverseProperty("Vehicle")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerVehicles")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Vehicle")]
    public virtual ICollection<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; } = new List<CustomerPackageSubscription>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();

    [ForeignKey("ModelId")]
    [InverseProperty("CustomerVehicles")]
    public virtual CarModel Model { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("CustomerVehicles")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("Vehicle")]
    public virtual ICollection<VehicleCustomService> VehicleCustomServices { get; set; } = new List<VehicleCustomService>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<VehicleHealthMetric> VehicleHealthMetrics { get; set; } = new List<VehicleHealthMetric>();

    [InverseProperty("Vehicle")]
    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
