namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    /// <summary>
    /// Request DTO for creating pre-payment for an appointment
    /// </summary>
    public class PayAppointmentRequestDto
    {
        /// <summary>
        /// URL to redirect customer after payment completion
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Payment method: "VNPay" or "MoMo"
        /// Default: VNPay
        /// </summary>
        public string PaymentMethod { get; set; } = "VNPay";
    }
}
