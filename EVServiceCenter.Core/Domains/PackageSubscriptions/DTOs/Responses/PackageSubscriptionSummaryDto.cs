using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses
{
    /// <summary>
    /// DTO summary cho list view subscriptions
    /// Lightweight - không load hết service usages
    /// </summary>
    public class PackageSubscriptionSummaryDto
    {
        public int SubscriptionId { get; set; }
        public string PackageCode { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public string? PackageImageUrl { get; set; }

        public string VehiclePlateNumber { get; set; } = null!;
        public string VehicleModelName { get; set; } = null!;

        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public decimal PricePaid { get; set; }

        public SubscriptionStatusEnum Status { get; set; }
        public string StatusDisplayName { get; set; } = null!;

        /// <summary>
        /// Số services trong gói
        /// </summary>
        public int TotalServicesCount { get; set; }

        /// <summary>
        /// Đã dùng / Tổng lượt
        /// VD: "3/10" = đã dùng 3 trong 10 lượt
        /// </summary>
        public string UsageStatus { get; set; } = null!;

        /// <summary>
        /// Phần trăm đã sử dụng
        /// </summary>
        public decimal UsagePercentage { get; set; }

        /// <summary>
        /// Còn bao nhiêu ngày
        /// NULL nếu không giới hạn thời gian
        /// </summary>
        public int? DaysUntilExpiry { get; set; }

        /// <summary>
        /// Có thể dùng không
        /// Active và chưa hết hạn và còn lượt
        /// </summary>
        public bool CanUse { get; set; }

        /// <summary>
        /// Warning message nếu sắp hết hạn hoặc sắp hết lượt
        /// VD: "Sắp hết hạn (còn 5 ngày)", "Chỉ còn 1 lượt"
        /// </summary>
        public string? WarningMessage { get; set; }
    }
}
