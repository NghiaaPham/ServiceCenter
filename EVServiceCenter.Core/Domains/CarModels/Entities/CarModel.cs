using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.CarModels.Entities
{
    [Table("CarModels")]
    public partial class CarModel
    {
        [Key]
        [Column("ModelID")]
        public int ModelId { get; set; }

        [Required]
        [Column("BrandID")]
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        public string ModelName { get; set; } = null!;

        public int? Year { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? BatteryCapacity { get; set; }

        public int? MaxRange { get; set; }

        [StringLength(100)]
        public string? ChargingType { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? MotorPower { get; set; }

        [Column(TypeName = "decimal(4, 2)")]
        public decimal? AccelerationTime { get; set; }

        public int? TopSpeed { get; set; }

        public int? ServiceInterval { get; set; }

        public int? ServiceIntervalMonths { get; set; }

        public int? WarrantyPeriod { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // THÊM MỚI
        [StringLength(1000)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        // THÊM MỚI - Audit fields
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("BrandId")]
        [InverseProperty("CarModels")]
        public virtual CarBrand Brand { get; set; } = null!;

        [InverseProperty("Model")]
        public virtual ICollection<CustomerVehicle> CustomerVehicles { get; set; } = new List<CustomerVehicle>();

        [InverseProperty("Model")]
        public virtual ICollection<ModelServicePricing> ModelServicePricings { get; set; } = new List<ModelServicePricing>();
    }
}