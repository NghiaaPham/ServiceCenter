namespace EVServiceCenter.Core.Enums
{
    /// <summary>
    /// Nguồn gốc tạo WorkOrder
    /// - Scheduled: Từ Appointment (customer đặt lịch trước)
    /// - WalkIn: Khách hàng walk-in trực tiếp (không có appointment)
    /// </summary>
    public enum WorkOrderSourceType
    {
        /// <summary>
        /// Từ appointment đã đặt lịch trước
        /// Created via: POST /api/appointments/{id}/check-in
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Khách hàng walk-in trực tiếp (không đặt lịch)
        /// Created via: POST /api/work-orders
        /// </summary>
        WalkIn = 2
    }
}
