using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Entities;

public partial class TimeSlot
{
    [Key]
    [Column("SlotID")]
    public int SlotId { get; set; }

    [Column("CenterID")]
    public int CenterId { get; set; }

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDuration { get; set; }

    public int? MaxBookings { get; set; }

    public int? CurrentBookings { get; set; }

    public bool? IsAvailable { get; set; }

    [StringLength(20)]
    public string? SlotType { get; set; }

    public DateTime? CreatedDate { get; set; }

    [InverseProperty("Slot")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("CenterId")]
    [InverseProperty("TimeSlots")]
    public virtual ServiceCenter Center { get; set; } = null!;
}
