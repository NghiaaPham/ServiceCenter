namespace EVServiceCenter.Core.Enums
{
    /// <summary>
    /// Nguồn gốc của service trong appointment
    /// Xác định service đến từ đâu: Package, Extra, hay Regular
    /// </summary>
    public enum ServiceSourceEnum
    {
        /// <summary>
        /// Dịch vụ đơn lẻ thường (không thuộc package)
        /// Khách hàng chọn service riêng lẻ, không có package/subscription
        /// </summary>
        Regular = 1,

        /// <summary>
        /// Dịch vụ từ subscription package đã mua
        /// Service này nằm trong subscription mà khách hàng đã mua trước
        /// Không tính thêm tiền, chỉ trừ số lần sử dụng
        /// </summary>
        FromSubscription = 2,

        /// <summary>
        /// Dịch vụ extra (thêm ngoài subscription)
        /// Service này KHÔNG nằm trong subscription, khách thêm sau
        /// Tính giá lẻ riêng biệt, KHÔNG áp dụng discount của package
        /// </summary>
        ExtraService = 3
    }
}
