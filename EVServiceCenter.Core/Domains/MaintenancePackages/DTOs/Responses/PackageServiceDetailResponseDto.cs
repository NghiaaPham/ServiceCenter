namespace EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses
{
    /// <summary>
    /// Chi tiết 1 service trong package
    /// Sử dụng khi hiển thị chi tiết package cho khách hàng
    /// </summary>
    public class PackageServiceDetailResponseDto
    {
        /// <summary>
        /// ID của PackageService (relation record)
        /// </summary>
        public int PackageServiceId { get; set; }

        /// <summary>
        /// ID của MaintenanceService
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Tên dịch vụ - VD: "Thay dầu động cơ điện"
        /// </summary>
        public string ServiceName { get; set; } = null!;

        /// <summary>
        /// Mô tả dịch vụ
        /// VD: "Kiểm tra và thay dầu làm mát động cơ điện..."
        /// </summary>
        public string? ServiceDescription { get; set; }

        /// <summary>
        /// Tên danh mục dịch vụ
        /// VD: "Bảo dưỡng định kỳ", "Kiểm tra điện"
        /// </summary>
        public string CategoryName { get; set; } = null!;

        /// <summary>
        /// Giá base của service nếu mua lẻ (VND)
        /// Để khách thấy được tiết kiệm bao nhiêu khi mua gói
        /// </summary>
        public decimal ServiceBasePrice { get; set; }

        /// <summary>
        /// Thời gian chuẩn để thực hiện service (phút)
        /// VD: 30 phút
        /// </summary>
        public int StandardTimeInMinutes { get; set; }

        /// <summary>
        /// Số lần thực hiện trong gói
        /// VD: "Thay dầu x2 lần" → QuantityInPackage = 2
        /// </summary>
        public int QuantityInPackage { get; set; }

        /// <summary>
        /// Service này có tính trong giá gói không
        /// TRUE: Đã tính trong TotalPriceAfterDiscount
        /// FALSE: Service bổ sung (hiếm)
        /// </summary>
        public bool IsIncludedInPackagePrice { get; set; }

        /// <summary>
        /// Chi phí nếu khách muốn thêm lần (VND)
        /// VD: Gói có 1 lần thay dầu, muốn thêm lần 2 → trả 200,000
        /// NULL: Không cho phép mua thêm
        /// </summary>
        public decimal? AdditionalCostPerExtraQuantity { get; set; }
    }
}
