namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses
{
    /// <summary>
    /// DTO hiển thị thông tin usage của 1 service trong subscription
    /// Cho customer biết còn bao nhiêu lần sử dụng
    /// </summary>
    public class PackageServiceUsageDto
    {
        public int UsageId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? ServiceDescription { get; set; }

        /// <summary>
        /// Tổng số lần được dùng
        /// VD: 3 lần thay dầu
        /// </summary>
        public int TotalAllowedQuantity { get; set; }

        /// <summary>
        /// Đã dùng bao nhiêu lần
        /// VD: Đã thay dầu 1 lần
        /// </summary>
        public int UsedQuantity { get; set; }

        /// <summary>
        /// Còn lại bao nhiêu lần
        /// VD: Còn 2 lần thay dầu
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// Lần cuối dùng dịch vụ này
        /// NULL nếu chưa dùng lần nào
        /// </summary>
        public DateTime? LastUsedDate { get; set; }

        /// <summary>
        /// ID của appointment lần cuối dùng
        /// Để customer có thể xem lại lịch sử
        /// </summary>
        public int? LastUsedAppointmentId { get; set; }

        /// <summary>
        /// Đã sử dụng hết chưa
        /// TRUE nếu RemainingQuantity = 0
        /// </summary>
        public bool IsFullyUsed => RemainingQuantity == 0;

        /// <summary>
        /// Phần trăm đã sử dụng
        /// VD: 33.33% nếu dùng 1/3 lần
        /// </summary>
        public decimal UsagePercentage => TotalAllowedQuantity > 0
            ? (decimal)UsedQuantity / TotalAllowedQuantity * 100
            : 0;
    }
}
