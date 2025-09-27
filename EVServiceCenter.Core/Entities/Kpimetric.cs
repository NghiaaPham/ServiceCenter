using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("KPIMetrics")]
public partial class Kpimetric
{
    [Key]
    [Column("MetricID")]
    public int MetricId { get; set; }

    [StringLength(100)]
    public string MetricName { get; set; } = null!;

    [StringLength(50)]
    public string MetricType { get; set; } = null!;

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(500)]
    public string? CalculationFormula { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TargetValue { get; set; }

    [StringLength(20)]
    public string? UnitOfMeasure { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
