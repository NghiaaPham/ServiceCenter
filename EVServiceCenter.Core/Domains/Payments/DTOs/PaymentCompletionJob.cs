using System;

namespace EVServiceCenter.Core.Domains.Payments.DTOs
{
    public class PaymentCompletionJob
    {
        public int AppointmentId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentIntentId { get; set; }
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
        public int ProcessedBy { get; set; } = 0; // system user id by default
        public int RetryCount { get; set; } = 0;
    }
}
