using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("WorkOrderTimeline")]
public partial class WorkOrderTimeline
{
    [Key]
    [Column("TimelineID")]
    public int TimelineId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [StringLength(50)]
    public string EventType { get; set; } = null!;

    [StringLength(500)]
    public string EventDescription { get; set; } = null!;

    public string? EventData { get; set; }

    public DateTime? EventDate { get; set; }

    public int? PerformedBy { get; set; }

    public bool? IsVisible { get; set; }

    [ForeignKey("PerformedBy")]
    [InverseProperty("WorkOrderTimelines")]
    public virtual User? PerformedByNavigation { get; set; }

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderTimelines")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
