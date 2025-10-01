using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Shift
{
    [Key]
    [Column("ShiftID")]
    public int ShiftId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("CenterID")]
    public int CenterId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public TimeOnly? BreakStartTime { get; set; }

    public TimeOnly? BreakEndTime { get; set; }

    [StringLength(20)]
    public string? ShiftType { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public TimeOnly? ActualStartTime { get; set; }

    public TimeOnly? ActualEndTime { get; set; }

    [Column(TypeName = "numeric(17, 6)")]
    public decimal? WorkedHours { get; set; }

    public TimeOnly? ActualBreakStart { get; set; }

    public TimeOnly? ActualBreakEnd { get; set; }

    public int? BreakMinutes { get; set; }

    [Column(TypeName = "numeric(18, 6)")]
    public decimal? NetWorkingHours { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public bool? IsLate { get; set; }

    public bool? IsEarlyLeave { get; set; }

    [StringLength(500)]
    public string? AbsentReason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("ShiftApprovedByNavigations")]
    public virtual User? ApprovedByNavigation { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Shifts")]
    public virtual ServiceCenter Center { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("ShiftCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ShiftUsers")]
    public virtual User User { get; set; } = null!;
}
