using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests
{
    /// <summary>
    /// DTO để filter/search packages với pagination
    /// Sử dụng cho API GET /api/maintenance-packages
    /// </summary>
    public class MaintenancePackageQueryDto
    {
        /// <summary>
        /// Trang hiện tại (bắt đầu từ 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số lượng items trên 1 trang
        /// </summary>
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// Tìm kiếm theo tên hoặc mã gói
        /// VD: "cơ bản" sẽ tìm "Gói bảo dưỡng cơ bản"
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter theo trạng thái
        /// NULL: Lấy tất cả
        /// Active: Chỉ lấy đang bán
        /// Inactive: Chỉ lấy tạm ngưng
        /// </summary>
        public PackageStatusEnum? Status { get; set; }

        /// <summary>
        /// Chỉ lấy gói popular (IsPopularPackage = true)
        /// TRUE: Chỉ lấy popular packages
        /// FALSE/NULL: Lấy tất cả
        /// </summary>
        public bool? IsPopularOnly { get; set; }

        /// <summary>
        /// Filter theo giá tối thiểu (VND)
        /// VD: MinPrice = 1000000 → chỉ lấy gói >= 1 triệu
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Filter theo giá tối đa (VND)
        /// VD: MaxPrice = 5000000 → chỉ lấy gói <= 5 triệu
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Filter theo discount tối thiểu (%)
        /// VD: MinDiscount = 15 → chỉ lấy gói giảm >= 15%
        /// </summary>
        public decimal? MinDiscountPercent { get; set; }

        /// <summary>
        /// Sắp xếp theo field nào
        /// Options: "Price", "Name", "Discount", "CreatedDate", "Popular"
        /// Default: "CreatedDate"
        /// </summary>
        public string SortBy { get; set; } = "CreatedDate";

        /// <summary>
        /// Sắp xếp giảm dần hay không
        /// TRUE: Giảm dần (Z-A, mới nhất trước)
        /// FALSE: Tăng dần (A-Z, cũ nhất trước)
        /// </summary>
        public bool SortDescending { get; set; } = true;
    }
}
