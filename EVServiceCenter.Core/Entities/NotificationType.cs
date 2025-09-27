using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class NotificationType
{
    [Key]
    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(50)]
    public string TypeName { get; set; } = null!;

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public bool? DefaultEnabled { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Type")]
    public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
}
