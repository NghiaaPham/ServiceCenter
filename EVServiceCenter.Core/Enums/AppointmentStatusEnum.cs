namespace EVServiceCenter.Core.Enums
{
    /// <summary>
    /// Trạng thái của Appointment trong hệ thống
    /// </summary>
    public enum AppointmentStatusEnum
    {
        /// <summary>
        /// Chờ xác nhận từ trung tâm
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Đã được xác nhận
        /// </summary>
        Confirmed = 2,

        /// <summary>
        /// Khách đã check-in tại trung tâm
        /// </summary>
        CheckedIn = 3,

        /// <summary>
        /// Đang thực hiện dịch vụ
        /// </summary>
        InProgress = 4,

        /// <summary>
        /// Hoàn thành và đã thanh toán đầy đủ
        /// </summary>
        Completed = 5,

        /// <summary>
        /// Hoàn thành nhưng còn khoản thanh toán chưa hoàn tất
        /// (Dùng khi có service bị degrade và thanh toán bổ sung thất bại)
        /// </summary>
        CompletedWithUnpaidBalance = 6,

        /// <summary>
        /// Đã hủy bỏ
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// Đã được dời lịch sang appointment khác
        /// </summary>
        Rescheduled = 8,

        /// <summary>
        /// Khách không đến (No-show)
        /// </summary>
        NoShow = 9
    }
}