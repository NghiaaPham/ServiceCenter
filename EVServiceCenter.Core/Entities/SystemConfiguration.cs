using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class SystemConfiguration
{
    [Key]
    [Column("ConfigID")]
    public int ConfigId { get; set; }

    [StringLength(100)]
    public string ConfigKey { get; set; } = null!;

    public string? ConfigValue { get; set; }

    [StringLength(20)]
    public string? DataType { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsEditable { get; set; }

    public bool? RequiresRestart { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("SystemConfigurations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
