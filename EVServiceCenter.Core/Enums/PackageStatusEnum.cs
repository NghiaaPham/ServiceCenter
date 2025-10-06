namespace EVServiceCenter.Core.Enums
{
    /// <summary>
    /// Trạng thái của Maintenance Package
    /// Xác định package có đang bán hay không
    /// </summary>
    public enum PackageStatusEnum
    {
        /// <summary>
        /// Đang hoạt động - khách hàng có thể mua
        /// Package hiển thị trên hệ thống và cho phép mua subscription
        /// </summary>
        Active = 1,

        /// <summary>
        /// Tạm ngưng - không hiển thị cho khách hàng
        /// Package tạm thời không bán (có thể do update, thay đổi giá...)
        /// Các subscription đã mua vẫn có hiệu lực
        /// </summary>
        Inactive = 2,

        /// <summary>
        /// Đã xóa (soft delete)
        /// Package đã bị xóa, không hiển thị và không cho phép mua
        /// Giữ lại trong DB để tham chiếu với các subscription cũ
        /// </summary>
        Deleted = 3
    }
}
