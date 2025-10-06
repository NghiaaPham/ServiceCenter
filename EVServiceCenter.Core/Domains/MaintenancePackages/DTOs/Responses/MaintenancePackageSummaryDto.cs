using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses
{
    /// <summary>
    /// DTO tóm tắt package (dùng cho danh sách)
    /// Sử dụng khi GET /api/maintenance-packages (list view)
    /// Chỉ hiển thị thông tin quan trọng, không load hết details
    /// </summary>
    public class MaintenancePackageSummaryDto
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
        /// Mô tả ngắn gọn (rút gọn từ Description)
        /// VD: "Gói bảo dưỡng toàn diện cho xe EV..."
        /// Max 200 ký tự
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Giá gói sau discount (VND) - Giá khách phải trả
        /// VD: 2,000,000 VND
        /// </summary>
        public decimal TotalPriceAfterDiscount { get; set; }

        /// <summary>
        /// Giá gốc (tổng giá các services nếu mua lẻ) (VND)
        /// VD: 2,500,000 VND
        /// </summary>
        public decimal OriginalPriceBeforeDiscount { get; set; }

        /// <summary>
        /// Số tiền tiết kiệm (VND)
        /// </summary>
        public decimal SavingsAmount { get; set; }

        /// <summary>
        /// Phần trăm discount (%)
        /// VD: 20%
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// URL hình ảnh gói
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Có phải gói phổ biến không (hiển thị badge)
        /// </summary>
        public bool IsPopularPackage { get; set; }

        /// <summary>
        /// Trạng thái gói (Active/Inactive/Deleted)
        /// </summary>
        public PackageStatusEnum Status { get; set; }

        /// <summary>
        /// Tên hiển thị status (tiếng Việt)
        /// VD: "Đang hoạt động"
        /// </summary>
        public string StatusDisplayName { get; set; } = null!;

        /// <summary>
        /// Thời hạn sử dụng (số ngày)
        /// VD: 365 ngày
        /// </summary>
        public int? ValidityPeriodInDays { get; set; }

        /// <summary>
        /// Số km cho phép
        /// VD: 10,000 km
        /// </summary>
        public int? ValidityMileage { get; set; }

        /// <summary>
        /// Số dịch vụ trong gói
        /// VD: 8 services
        /// </summary>
        public int TotalServicesCount { get; set; }

        /// <summary>
        /// Tổng số lần thực hiện
        /// VD: 10 lần (vì có service x2 lần)
        /// </summary>
        public int TotalServiceQuantity { get; set; }

        /// <summary>
        /// Tổng thời gian ước tính (phút)
        /// VD: 180 phút
        /// </summary>
        public int TotalEstimatedTimeInMinutes { get; set; }

        /// <summary>
        /// Preview tên các service (comma-separated, tối đa 3 cái)
        /// VD: "Thay dầu, Kiểm tra phanh, Kiểm tra điều hòa..."
        /// Để khách xem nhanh gói có gì
        /// </summary>
        public string ServiceNamesPreview { get; set; } = null!;

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
