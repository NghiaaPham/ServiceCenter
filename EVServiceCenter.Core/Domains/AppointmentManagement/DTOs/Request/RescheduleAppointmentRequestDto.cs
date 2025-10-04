namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class RescheduleAppointmentRequestDto
    {
        public int AppointmentId { get; set; }
        public int NewSlotId { get; set; }
        public string? Reason { get; set; }
    }
}
