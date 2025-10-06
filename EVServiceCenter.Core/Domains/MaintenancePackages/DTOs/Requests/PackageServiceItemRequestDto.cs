using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests
{
    /// <summary>
    /// DTO cho từng dịch vụ trong package
    /// Sử dụng khi tạo hoặc update package
    /// </summary>
    public class PackageServiceItemRequestDto
    {
        /// <summary>
        /// ID của MaintenanceService
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ServiceId phải lớn hơn 0")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Số lần thực hiện service trong gói
        /// VD: "Thay dầu x2 lần" → QuantityInPackage = 2
        /// Khách mua gói sẽ được thay dầu 2 lần
        /// </summary>
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1-100")]
        public int QuantityInPackage { get; set; } = 1;

        /// <summary>
        /// Service này có tính trong giá gói không
        /// TRUE: Tính trong TotalPriceAfterDiscount
        /// FALSE: Service bổ sung (hiếm khi dùng)
        /// </summary>
        public bool IsIncludedInPackagePrice { get; set; } = true;

        /// <summary>
        /// Chi phí bổ sung nếu khách muốn thêm lần (optional)
        /// VD: Gói có 1 lần thay dầu, khách muốn thay lần 2 phải trả thêm
        /// NULL: Không cho phép mua thêm, phải mua package mới
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí phải >= 0")]
        public decimal? AdditionalCostPerExtraQuantity { get; set; }
    }
}
