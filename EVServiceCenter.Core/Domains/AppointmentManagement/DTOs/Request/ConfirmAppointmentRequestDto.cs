namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class ConfirmAppointmentRequestDto
    {
        /// <summary>
        /// ID appointment cần xác nhận
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// Phương thức xác nhận: Phone, Email, SMS, InPerson
        /// </summary>
        public string ConfirmationMethod { get; set; } = "Phone";

        /// <summary>
        /// Ghi chú khi xác nhận (optional)
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Có gửi email xác nhận cho khách không
        /// </summary>
        public bool SendConfirmationEmail { get; set; } = true;

        /// <summary>
        /// Có gửi SMS xác nhận cho khách không
        /// </summary>
        public bool SendConfirmationSMS { get; set; } = false;
    }
}
