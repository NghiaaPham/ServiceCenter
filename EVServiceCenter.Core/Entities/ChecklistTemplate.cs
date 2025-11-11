using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ChecklistTemplate
{
    [Key]
    [Column("TemplateID")]
    public int TemplateId { get; set; }

    [StringLength(100)]
    public string TemplateName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column("ServiceID")]
    public int? ServiceId { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    public string Items { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("ChecklistTemplates")]
    public virtual ServiceCategory? Category { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("ChecklistTemplates")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("ChecklistTemplates")]
    public virtual MaintenanceService? Service { get; set; }
}
