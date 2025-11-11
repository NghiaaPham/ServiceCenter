using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests
{
    /// <summary>
    /// DTO cho purchase subscription v?i payment method
    /// H? tr? c? online payment (VNPay/MoMo) và cash/bank transfer
    /// </summary>
    public class PurchaseWithPaymentRequestDto
    {
        /// <summary>
        /// ID c?a package c?n mua
        /// </summary>
        [Required(ErrorMessage = "PackageId là b?t bu?c")]
        [Range(1, int.MaxValue, ErrorMessage = "PackageId ph?i l?n h?n 0")]
        public int PackageId { get; set; }

        /// <summary>
        /// ID c?a vehicle áp d?ng subscription
        /// </summary>
        [Required(ErrorMessage = "VehicleId là b?t bu?c")]
        [Range(1, int.MaxValue, ErrorMessage = "VehicleId ph?i l?n h?n 0")]
        public int VehicleId { get; set; }

        /// <summary>
        /// Ph??ng th?c thanh toán
        /// Values: "VNPay", "MoMo", "Cash", "BankTransfer"
        /// </summary>
        [Required(ErrorMessage = "PaymentMethod là b?t bu?c")]
        public string PaymentMethod { get; set; } = null!;

        /// <summary>
        /// URL ?? redirect sau khi thanh toán online (ch? dùng cho VNPay/MoMo)
        /// </summary>
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Ghi chú c?a khách hàng
        /// </summary>
        [MaxLength(1000, ErrorMessage = "CustomerNotes không ???c v??t quá 1000 ký t?")]
        public string? CustomerNotes { get; set; }
    }
}
