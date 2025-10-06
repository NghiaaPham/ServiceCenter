using EVServiceCenter.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests
{
    /// <summary>
    /// DTO để tạo gói bảo dưỡng mới
    /// Staff/Admin sử dụng để define package trong hệ thống
    /// </summary>
    public class CreateMaintenancePackageRequestDto
    {
        /// <summary>
        /// Mã gói (unique) - VD: "PKG-BASIC-2025"
        /// Để dễ quản lý và tra cứu
        /// </summary>
        [Required(ErrorMessage = "Mã gói không được trống")]
        [StringLength(20, ErrorMessage = "Mã gói tối đa 20 ký tự")]
        public string PackageCode { get; set; } = null!;

        /// <summary>
        /// Tên gói - VD: "Gói bảo dưỡng cơ bản EV"
        /// Hiển thị cho khách hàng
        /// </summary>
        [Required(ErrorMessage = "Tên gói không được trống")]
        [StringLength(100, ErrorMessage = "Tên gói tối đa 100 ký tự")]
        public string PackageName { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết gói
        /// VD: "Gói bảo dưỡng toàn diện cho xe EV, bao gồm 8 dịch vụ cơ bản..."
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Thời hạn sử dụng (số ngày) - VD: 365 ngày = 1 năm
        /// Sau thời hạn này, subscription hết hiệu lực
        /// </summary>
        [Range(1, 3650, ErrorMessage = "Thời hạn phải từ 1-3650 ngày (10 năm)")]
        public int? ValidityPeriodInDays { get; set; }

        /// <summary>
        /// Số km cho phép trong thời hạn - VD: 10,000 km
        /// Subscription hết hiệu lực khi xe chạy quá số km này
        /// </summary>
        [Range(0, 500000, ErrorMessage = "Số km phải từ 0-500,000")]
        public int? ValidityMileage { get; set; }

        /// <summary>
        /// Giá gói SAU KHI GIẢM (VND) - Giá khách phải trả
        /// VD: Tổng giá 8 services = 2.500.000, giảm 20% → 2.000.000
        /// </summary>
        [Required(ErrorMessage = "Giá gói không được trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá gói phải >= 0")]
        public decimal TotalPriceAfterDiscount { get; set; }

        /// <summary>
        /// Phần trăm giảm giá (0-100%) - VD: 20 = giảm 20%
        /// Để marketing: "Tiết kiệm 20% khi mua gói"
        /// </summary>
        [Range(0, 100, ErrorMessage = "Discount phải từ 0-100%")]
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// URL hình ảnh gói (để hiển thị trên app/web)
        /// </summary>
        [StringLength(500, ErrorMessage = "URL tối đa 500 ký tự")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Có phải gói phổ biến không
        /// TRUE: Hiển thị badge "Best Seller" hoặc "Phổ biến"
        /// </summary>
        public bool IsPopularPackage { get; set; } = false;

        /// <summary>
        /// Trạng thái gói
        /// Active: Đang bán
        /// Inactive: Tạm ngưng
        /// Deleted: Đã xóa
        /// </summary>
        public PackageStatusEnum Status { get; set; } = PackageStatusEnum.Active;

        /// <summary>
        /// Danh sách các dịch vụ trong gói
        /// Phải có ít nhất 1 dịch vụ
        /// </summary>
        [Required(ErrorMessage = "Gói phải có ít nhất 1 dịch vụ")]
        [MinLength(1, ErrorMessage = "Gói phải có ít nhất 1 dịch vụ")]
        public List<PackageServiceItemRequestDto> IncludedServices { get; set; } = new();
    }
}
