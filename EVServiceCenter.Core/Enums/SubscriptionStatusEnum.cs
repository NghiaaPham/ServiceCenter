namespace EVServiceCenter.Core.Enums
{
    /// <summary>
    /// Trạng thái của Customer Package Subscription
    /// Xác định subscription còn sử dụng được hay không
    /// </summary>
    public enum SubscriptionStatusEnum
    {
        /// <summary>
        /// Chờ thanh toán - subscription vừa tạo
        /// Chưa thanh toán hoặc chưa xác nhận thanh toán
        /// Chưa thể sử dụng services
        /// </summary>
        PendingPayment = 0,

        /// <summary>
        /// Đang hoạt động - khách có thể sử dụng
        /// Subscription còn hạn, còn số lần sử dụng
        /// </summary>
        Active = 1,

        /// <summary>
        /// Đã sử dụng hết - hết số lần
        /// Tất cả services trong gói đã được dùng hết
        /// Nhưng chưa hết hạn về thời gian
        /// </summary>
        FullyUsed = 2,

        /// <summary>
        /// Hết hạn - quá thời gian hoặc quá số km
        /// Đã quá ValidityPeriod hoặc quá ValidityMileage
        /// Không thể sử dụng nữa dù còn lần
        /// </summary>
        Expired = 3,

        /// <summary>
        /// Đã hủy - khách hàng hoặc admin hủy
        /// Có thể hoàn tiền một phần (tùy policy)
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Tạm ngưng - có vấn đề cần giải quyết
        /// VD: Thanh toán chưa xác nhận, tranh chấp...
        /// </summary>
        Suspended = 5
    }
}
