namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    /// <summary>
    /// Response DTO sau khi Admin điều chỉnh ServiceSource thành công
    /// Chứa thông tin chi tiết về adjustment để Admin xác nhận
    /// </summary>
    public class AdjustServiceSourceResponseDto
    {
        /// <summary>
        /// ID của AppointmentService đã được điều chỉnh
        /// </summary>
        public int AppointmentServiceId { get; set; }

        /// <summary>
        /// ServiceSource cũ (trước khi điều chỉnh)
        /// </summary>
        public string OldServiceSource { get; set; } = null!;

        /// <summary>
        /// ServiceSource mới (sau khi điều chỉnh)
        /// </summary>
        public string NewServiceSource { get; set; } = null!;

        /// <summary>
        /// Giá cũ (VNĐ)
        /// </summary>
        public decimal OldPrice { get; set; }

        /// <summary>
        /// Giá mới (VNĐ)
        /// </summary>
        public decimal NewPrice { get; set; }

        /// <summary>
        /// Chênh lệch giá (NewPrice - OldPrice)
        /// Âm = Giảm giá/Hoàn tiền
        /// Dương = Tăng giá/Thu thêm
        /// </summary>
        public decimal PriceDifference { get; set; }

        /// <summary>
        /// Đã hoàn tiền cho customer chưa?
        /// TRUE nếu PriceDifference < 0 và IssueRefund = true
        /// </summary>
        public bool RefundIssued { get; set; }

        /// <summary>
        /// Đã trừ lượt subscription chưa?
        /// TRUE nếu chuyển từ Extra → Subscription (cần trừ lượt)
        /// FALSE nếu chuyển từ Subscription → Extra (có thể hoàn lại lượt)
        /// </summary>
        public bool UsageDeducted { get; set; }

        /// <summary>
        /// User ID của Admin thực hiện adjustment
        /// </summary>
        public int UpdatedBy { get; set; }

        /// <summary>
        /// Thời gian thực hiện adjustment
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
