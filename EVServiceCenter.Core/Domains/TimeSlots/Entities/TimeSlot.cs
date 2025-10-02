using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.TimeSlots.Entities

{
    [Table("TimeSlots")]
    public partial class TimeSlot
    {
        [Key]
        [Column("SlotID")]
        public int SlotId { get; set; }

        [Column("CenterID")]
        public int CenterId { get; set; }

        [Required]
        public DateOnly SlotDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// Maximum concurrent bookings allowed for this slot
        /// </summary>
        public int MaxBookings { get; set; } = 1;

        /// <summary>
        /// Slot type for special handling (Regular, Express, Emergency, etc.)
        /// </summary>
        [StringLength(20)]
        public string? SlotType { get; set; }

        /// <summary>
        /// Manual override to disable slot (maintenance, holiday, etc.)
        /// </summary>
        public bool IsBlocked { get; set; } = false;

        [StringLength(200)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // ========== NAVIGATION PROPERTIES ==========

        [InverseProperty("Slot")]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [ForeignKey("CenterId")]
        [InverseProperty("TimeSlots")]
        public virtual ServiceCenter Center { get; set; } = null!;

        // ========== COMPUTED PROPERTIES (NOT MAPPED) ==========

        /// <summary>
        /// Slot duration in minutes (computed from EndTime - StartTime)
        /// </summary>
        [NotMapped]
        public int DurationMinutes => (int)(EndTime.ToTimeSpan() - StartTime.ToTimeSpan()).TotalMinutes;

        /// <summary>
        /// Current number of active bookings (computed from Appointments)
        /// Only counts statuses that occupy capacity
        /// </summary>
        [NotMapped]
        public int CurrentBookings => Appointments?.Count(a =>
            AppointmentStatusHelper.IsActiveBooking(a.StatusId)
        ) ?? 0;

        /// <summary>
        /// Check if slot is available for new bookings
        /// </summary>
        [NotMapped]
        public bool IsAvailable
        {
            get
            {
                // Blocked manually
                if (IsBlocked)
                    return false;

                // Past slot
                var slotDateTime = SlotDate.ToDateTime(StartTime);
                if (slotDateTime < DateTime.Now)
                    return false;

                // Full capacity
                if (CurrentBookings >= MaxBookings)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Remaining capacity
        /// </summary>
        [NotMapped]
        public int RemainingCapacity => Math.Max(0, MaxBookings - CurrentBookings);

    }
}