using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests
{
    /// <summary>
    /// Request DTO cho staff xác nh?n thanh toán Cash/BankTransfer
    /// </summary>
    public class ConfirmPaymentRequestDto
    {
        /// <summary>
        /// ID subscription c?n xác nh?n thanh toán
        /// </summary>
        [Required]
        public int SubscriptionId { get; set; }

        /// <summary>
        /// Ph??ng th?c thanh toán: Cash ho?c BankTransfer
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = null!;

        /// <summary>
        /// S? ti?n ?ã thanh toán
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "S? ti?n ph?i l?n h?n 0")]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Ghi chú (ví d?: "Thanh toán t?i qu?y lúc 14:50")
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Mã giao d?ch ngân hàng (b?t bu?c n?u PaymentMethod = BankTransfer)
        /// </summary>
        [StringLength(100)]
        public string? BankTransactionId { get; set; }

        /// <summary>
        /// Ngày chuy?n kho?n (b?t bu?c n?u PaymentMethod = BankTransfer)
        /// </summary>
        public DateTime? TransferDate { get; set; }
    }
}
