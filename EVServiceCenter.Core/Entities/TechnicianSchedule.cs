using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class TechnicianSchedule
{
    [Key]
    [Column("ScheduleID")]
    public int ScheduleId { get; set; }

    [Column("TechnicianID")]
    public int TechnicianId { get; set; }

    [Column("CenterID")]
    public int CenterId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public TimeOnly? BreakStartTime { get; set; }

    public TimeOnly? BreakEndTime { get; set; }

    public int? MaxCapacityMinutes { get; set; }

    public int? BookedMinutes { get; set; }

    public int? AvailableMinutes { get; set; }

    public bool? IsAvailable { get; set; }

    [StringLength(20)]
    public string? ShiftType { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("TechnicianSchedules")]
    public virtual ServiceCenter Center { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    [InverseProperty("TechnicianSchedules")]
    public virtual User Technician { get; set; } = null!;
}
