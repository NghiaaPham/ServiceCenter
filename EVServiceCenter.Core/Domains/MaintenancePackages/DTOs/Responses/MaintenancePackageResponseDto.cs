using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses
{
    /// <summary>
    /// DTO trả về chi tiết đầy đủ của package
    /// Sử dụng khi GET /api/maintenance-packages/{id}
    /// Hiển thị toàn bộ thông tin để khách hàng xem trước khi mua
    /// </summary>
    public class MaintenancePackageResponseDto
    {
        /// <summary>
        /// ID của package
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Mã gói - VD: "PKG-BASIC-2025"
        /// </summary>
        public string PackageCode { get; set; } = null!;

        /// <summary>
        /// Tên gói - VD: "Gói bảo dưỡng cơ bản EV"
        /// </summary>
        public string PackageName { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết gói
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời hạn sử dụng (số ngày)
        /// VD: 365 ngày = 1 năm
        /// </summary>
        public int? ValidityPeriodInDays { get; set; }

        /// <summary>
        /// Số km cho phép trong subscription
        /// VD: 10,000 km
        /// </summary>
        public int? ValidityMileage { get; set; }

        /// <summary>
        /// Giá gói sau discount (VND) - Giá khách phải trả
        /// VD: 2,000,000 VND
        /// </summary>
        public decimal TotalPriceAfterDiscount { get; set; }

        /// <summary>
        /// Giá gốc (tổng giá các services nếu mua lẻ) (VND)
        /// VD: 2,500,000 VND
        /// Để khách thấy được tiết kiệm bao nhiêu
        /// </summary>
        public decimal OriginalPriceBeforeDiscount { get; set; }

        /// <summary>
        /// Số tiền tiết kiệm được (VND)
        /// = OriginalPriceBeforeDiscount - TotalPriceAfterDiscount
        /// VD: 500,000 VND
        /// </summary>
        public decimal SavingsAmount { get; set; }

        /// <summary>
        /// Phần trăm discount (%)
        /// VD: 20% = Tiết kiệm 20%
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// URL hình ảnh gói
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Có phải gói phổ biến không
        /// TRUE: Hiển thị badge "Best Seller"
        /// </summary>
        public bool IsPopularPackage { get; set; }

        /// <summary>
        /// Trạng thái gói (enum)
        /// Active, Inactive, Deleted
        /// </summary>
        public PackageStatusEnum Status { get; set; }

        /// <summary>
        /// Tên hiển thị của status (tiếng Việt)
        /// VD: "Đang hoạt động", "Tạm ngưng", "Đã xóa"
        /// </summary>
        public string StatusDisplayName { get; set; } = null!;

        /// <summary>
        /// Ngày tạo package
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Chi tiết các dịch vụ trong gói (list đầy đủ)
        /// Khách xem được gói gồm những service gì
        /// </summary>
        public List<PackageServiceDetailResponseDto> IncludedServices { get; set; } = new();

        /// <summary>
        /// Tổng thời gian ước tính để làm hết các services (phút)
        /// VD: 180 phút = 3 giờ
        /// </summary>
        public int TotalEstimatedTimeInMinutes { get; set; }

        /// <summary>
        /// Số lượng dịch vụ trong gói
        /// VD: 8 services
        /// </summary>
        public int TotalServicesCount { get; set; }

        /// <summary>
        /// Tổng số lần thực hiện (sum of quantities)
        /// VD: 8 services, mỗi service x1 = 8 lần
        /// Hoặc: 5 services, có service x2 lần = 7 lần total
        /// </summary>
        public int TotalServiceQuantity { get; set; }
    }
}
