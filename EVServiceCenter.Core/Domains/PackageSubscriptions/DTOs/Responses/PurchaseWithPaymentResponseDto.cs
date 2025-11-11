using System;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses
{
    /// <summary>
    /// Response DTO sau khi purchase subscription v?i payment
    /// </summary>
    public class PurchaseWithPaymentResponseDto
    {
        /// <summary>
        /// Subscription ?ã t?o
        /// </summary>
        public PackageSubscriptionResponseDto Subscription { get; set; } = null!;

        /// <summary>
        /// Ph??ng th?c thanh toán ?ã ch?n
        /// </summary>
        public string PaymentMethod { get; set; } = null!;

        /// <summary>
        /// Tr?ng thái thanh toán
        /// Values: "Pending", "Completed", "Cancelled"
        /// </summary>
        public string PaymentStatus { get; set; } = null!;

        /// <summary>
        /// Payment URL (ch? có khi thanh toán online)
        /// Customer s? redirect ??n URL này ?? thanh toán
        /// </summary>
        public string? PaymentUrl { get; set; }

        /// <summary>
        /// Payment code/transaction ID
        /// </summary>
        public string? PaymentCode { get; set; }

        /// <summary>
        /// QR Code URL (cho VNPay/MoMo)
        /// </summary>
        public string? QrCodeUrl { get; set; }

        /// <summary>
        /// Deep link (cho MoMo app)
        /// </summary>
        public string? DeepLink { get; set; }

        /// <summary>
        /// Invoice ID (n?u ?ã t?o invoice)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Invoice code
        /// </summary>
        public string? InvoiceCode { get; set; }

        /// <summary>
        /// Payment expiry time (cho online payment)
        /// </summary>
        public DateTime? PaymentExpiresAt { get; set; }

        /// <summary>
        /// Thông báo cho user
        /// </summary>
        public string Message { get; set; } = null!;
    }
}
