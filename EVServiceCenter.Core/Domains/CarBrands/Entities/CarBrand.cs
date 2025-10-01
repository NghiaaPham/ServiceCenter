using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.CarBrands.Entities
{
    [Table("CarBrands")]
    public partial class CarBrand
    {
        [Key]
        [Column("BrandID")]
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]  
        public string BrandName { get; set; } = null!;

        [StringLength(100)]  
        public string? Country { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [InverseProperty("Brand")]
        public virtual ICollection<CarModel> CarModels { get; set; } = new List<CarModel>();

        [InverseProperty("Brand")]
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}