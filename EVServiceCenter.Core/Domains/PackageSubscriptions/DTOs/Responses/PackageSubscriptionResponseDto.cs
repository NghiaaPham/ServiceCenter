using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses
{
    /// <summary>
    /// DTO response đầy đủ thông tin subscription
    /// Dùng khi customer xem chi tiết subscription của mình
    /// </summary>
    public class PackageSubscriptionResponseDto
    {
        // ========== SUBSCRIPTION INFO ==========
        public int SubscriptionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = null!;
        public string VehicleModelName { get; set; } = null!;

        // ========== PACKAGE INFO ==========
        public int PackageId { get; set; }
        public string PackageCode { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public string? PackageDescription { get; set; }
        public string? PackageImageUrl { get; set; }

        // ========== SUBSCRIPTION DATES ==========
        public DateTime PurchaseDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Số ngày hiệu lực từ lúc mua
        /// VD: 365 ngày = 1 năm
        /// NULL nếu không giới hạn thời gian
        /// </summary>
        public int? ValidityPeriodInDays { get; set; }

        /// <summary>
        /// Số km hiệu lực
        /// VD: 10,000 km
        /// NULL nếu không giới hạn số km
        /// </summary>
        public int? ValidityMileage { get; set; }

        /// <summary>
        /// Số km hiện tại của xe khi mua gói
        /// Dùng để tính còn bao nhiêu km trong gói
        /// </summary>
        public int? InitialVehicleMileage { get; set; }

        // ========== PRICING ==========
        public decimal PricePaid { get; set; }

        // ========== STATUS ==========
        public SubscriptionStatusEnum Status { get; set; }
        public string StatusDisplayName { get; set; } = null!;

        /// <summary>
        /// Lý do cancel/suspend (nếu có)
        /// VD: "Khách yêu cầu", "Xe bán đi", "Gian lận"
        /// </summary>
        public string? CancellationReason { get; set; }
        public DateTime? CancelledDate { get; set; }

        // ========== SERVICE USAGE TRACKING ==========

        /// <summary>
        /// Danh sách services trong subscription và usage
        /// </summary>
        public List<PackageServiceUsageDto> ServiceUsages { get; set; } = new();

        /// <summary>
        /// Tổng số services trong gói
        /// </summary>
        public int TotalServicesCount => ServiceUsages.Count;

        /// <summary>
        /// Tổng số lượt dịch vụ (sum all quantities)
        /// VD: 3 dịch vụ x 2 lần = 6 lượt
        /// </summary>
        public int TotalServiceQuantity => ServiceUsages.Sum(s => s.TotalAllowedQuantity);

        /// <summary>
        /// Đã dùng bao nhiêu lượt
        /// </summary>
        public int TotalUsedQuantity => ServiceUsages.Sum(s => s.UsedQuantity);

        /// <summary>
        /// Còn lại bao nhiêu lượt
        /// </summary>
        public int TotalRemainingQuantity => ServiceUsages.Sum(s => s.RemainingQuantity);

        /// <summary>
        /// Phần trăm đã sử dụng gói
        /// </summary>
        public decimal UsagePercentage => TotalServiceQuantity > 0
            ? (decimal)TotalUsedQuantity / TotalServiceQuantity * 100
            : 0;

        // ========== EXPIRY CHECKS ==========

        /// <summary>
        /// Đã hết hạn chưa (theo thời gian)
        /// </summary>
        public bool IsExpiredByDate => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

        /// <summary>
        /// Còn bao nhiêu ngày hết hạn
        /// NULL nếu không giới hạn thời gian hoặc đã hết hạn
        /// </summary>
        public int? DaysUntilExpiry
        {
            get
            {
                if (!ExpiryDate.HasValue) return null;
                var days = (ExpiryDate.Value - DateTime.UtcNow).Days;
                return days > 0 ? days : 0;
            }
        }

        /// <summary>
        /// Ghi chú của customer khi mua
        /// </summary>
        public string? CustomerNotes { get; set; }
    }
}
