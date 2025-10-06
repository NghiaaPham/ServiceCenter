using EVServiceCenter.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests
{
    /// <summary>
    /// DTO để cập nhật gói bảo dưỡng đã có
    /// Staff/Admin sử dụng để chỉnh sửa package
    /// </summary>
    public class UpdateMaintenancePackageRequestDto
    {
        /// <summary>
        /// ID của package cần update
        /// </summary>
        [Required(ErrorMessage = "PackageId không được trống")]
        [Range(1, int.MaxValue, ErrorMessage = "PackageId phải lớn hơn 0")]
        public int PackageId { get; set; }

        /// <summary>
        /// Mã gói (unique) - VD: "PKG-BASIC-2025"
        /// </summary>
        [Required(ErrorMessage = "Mã gói không được trống")]
        [StringLength(20, ErrorMessage = "Mã gói tối đa 20 ký tự")]
        public string PackageCode { get; set; } = null!;

        /// <summary>
        /// Tên gói - VD: "Gói bảo dưỡng cơ bản EV"
        /// </summary>
        [Required(ErrorMessage = "Tên gói không được trống")]
        [StringLength(100, ErrorMessage = "Tên gói tối đa 100 ký tự")]
        public string PackageName { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết gói
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Thời hạn sử dụng (số ngày)
        /// </summary>
        [Range(1, 3650, ErrorMessage = "Thời hạn phải từ 1-3650 ngày")]
        public int? ValidityPeriodInDays { get; set; }

        /// <summary>
        /// Số km cho phép
        /// </summary>
        [Range(0, 500000, ErrorMessage = "Số km phải từ 0-500,000")]
        public int? ValidityMileage { get; set; }

        /// <summary>
        /// Giá gói SAU KHI GIẢM (VND)
        /// </summary>
        [Required(ErrorMessage = "Giá gói không được trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá gói phải >= 0")]
        public decimal TotalPriceAfterDiscount { get; set; }

        /// <summary>
        /// Phần trăm giảm giá (0-100%)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Discount phải từ 0-100%")]
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// URL hình ảnh gói
        /// </summary>
        [StringLength(500, ErrorMessage = "URL tối đa 500 ký tự")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Có phải gói phổ biến không
        /// </summary>
        public bool IsPopularPackage { get; set; } = false;

        /// <summary>
        /// Trạng thái gói (Active/Inactive/Deleted)
        /// </summary>
        public PackageStatusEnum Status { get; set; } = PackageStatusEnum.Active;

        /// <summary>
        /// Danh sách các dịch vụ trong gói (update toàn bộ)
        /// Phải có ít nhất 1 dịch vụ
        /// </summary>
        [Required(ErrorMessage = "Gói phải có ít nhất 1 dịch vụ")]
        [MinLength(1, ErrorMessage = "Gói phải có ít nhất 1 dịch vụ")]
        public List<PackageServiceItemRequestDto> IncludedServices { get; set; } = new();
    }
}
