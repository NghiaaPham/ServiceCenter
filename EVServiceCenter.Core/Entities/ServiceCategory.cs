using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ServiceCategory
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<ChecklistTemplate> ChecklistTemplates { get; set; } = new List<ChecklistTemplate>();

    [InverseProperty("Category")]
    public virtual ICollection<MaintenanceService> MaintenanceServices { get; set; } = new List<MaintenanceService>();
}
