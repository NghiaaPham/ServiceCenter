namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// Response DTO for pre-payment creation
    /// Contains payment URL and related information
    /// </summary>
    public class PrePaymentResponseDto
    {
        /// <summary>
        /// ID of the PaymentIntent created
        /// </summary>
        public int PaymentIntentId { get; set; }

        /// <summary>
        /// ID of the Invoice generated
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Invoice code (e.g., INV-20251025-0301)
        /// </summary>
        public string InvoiceCode { get; set; } = string.Empty;

        /// <summary>
        /// Total amount to be paid
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment gateway URL to redirect customer to
        /// </summary>
        public string PaymentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Internal payment code (e.g. PAY-20251025-0001)
        /// </summary>
        public string PaymentCode { get; set; } = string.Empty;

        /// <summary>
        /// Gateway identifier (VNPay, MoMo, Cash...)
        /// </summary>
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// Optional QR code link (MoMo)
        /// </summary>
        public string? QrCodeUrl { get; set; }

        /// <summary>
        /// Optional deep link for mobile app payments
        /// </summary>
        public string? DeepLink { get; set; }

        /// <summary>
        /// Payment intent expiration time (typically 24 hours)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
