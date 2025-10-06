namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests
{
    /// <summary>
    /// DTO khi khách hàng mua/subscribe vào 1 gói dịch vụ
    /// Flow: Customer chọn package → điền thông tin → tạo subscription
    /// </summary>
    public class PurchasePackageRequestDto
    {
        /// <summary>
        /// ID của package muốn mua
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// ID của xe áp dụng subscription này
        /// Subscription được gắn với 1 xe cụ thể
        /// </summary>
        public int VehicleId { get; set; }

        /// <summary>
        /// Ghi chú của khách hàng khi mua package
        /// VD: "Mua cho xe mới", "Gói bảo dưỡng định kỳ"
        /// </summary>
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Phương thức thanh toán
        /// VD: "Cash", "BankTransfer", "CreditCard", "MoMo", "ZaloPay"
        /// </summary>
        public string PaymentMethod { get; set; } = null!;

        /// <summary>
        /// ID transaction thanh toán (nếu thanh toán online)
        /// VD: MoMo transaction ID, ZaloPay order ID
        /// </summary>
        public string? PaymentTransactionId { get; set; }

        /// <summary>
        /// Số tiền thực tế khách hàng đã trả
        /// Thường bằng TotalPriceAfterDiscount của package
        /// Nhưng có thể khác nếu có promotion thêm
        /// </summary>
        public decimal AmountPaid { get; set; }
    }
}
