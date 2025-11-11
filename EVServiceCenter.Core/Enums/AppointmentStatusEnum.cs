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
        /// Đã hủy bỏ
        /// </summary>
        Cancelled = 6,

        /// <summary>
        /// Đã được dời lịch sang appointment khác
        /// </summary>
        Rescheduled = 7,

        /// <summary>
        /// Khách không đến (No-show)
        /// </summary>
        NoShow = 8,

        /// <summary>
        /// Hoàn thành nhưng còn công nợ chưa thanh toán
        /// (Xảy ra khi subscription services bị degrade thành Extra do hết lượt)
        /// </summary>
        CompletedWithUnpaidBalance = 9
    }

}