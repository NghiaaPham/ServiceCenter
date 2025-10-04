namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class CancelAppointmentRequestDto
    {
        public int AppointmentId { get; set; }
        public string CancellationReason { get; set; } = null!;
    }
}
