using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ChecklistItem
{
    [Key]
    [Column("ItemID")]
    public int ItemId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("TemplateID")]
    public int? TemplateId { get; set; }

    public int? ItemOrder { get; set; }

    [StringLength(500)]
    public string ItemDescription { get; set; } = null!;

    public bool? IsRequired { get; set; }

    public bool? IsCompleted { get; set; }

    public int? CompletedBy { get; set; }

    public DateTime? CompletedDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [ForeignKey("CompletedBy")]
    [InverseProperty("ChecklistItems")]
    public virtual User? CompletedByNavigation { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("ChecklistItems")]
    public virtual ChecklistTemplate? Template { get; set; }

    [ForeignKey("WorkOrderId")]
    [InverseProperty("ChecklistItems")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
