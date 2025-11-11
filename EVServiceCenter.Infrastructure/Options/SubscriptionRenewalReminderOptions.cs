using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Infrastructure.Options;

/// <summary>
/// Cấu hình nhắc gia hạn gói bảo dưỡng định kỳ cho khách hàng.
/// </summary>
public class SubscriptionRenewalReminderOptions
{
    /// <summary>
    /// Bật/tắt tính năng nhắc gia hạn.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Số ngày trước hạn thanh toán sẽ gửi nhắc lần 1 (ví dụ 7 ngày).
    /// </summary>
    [Range(0, 365)]
    public int FirstReminderDaysBefore { get; set; } = 7;

    /// <summary>
    /// Số ngày trước hạn thanh toán sẽ gửi nhắc lần cuối (ví dụ 1 ngày).
    /// </summary>
    [Range(0, 365)]
    public int FinalReminderDaysBefore { get; set; } = 1;

    /// <summary>
    /// Kênh gửi notification (InApp/Email/SMS). Mặc định InApp.
    /// </summary>
    [Required]
    public string Channel { get; set; } = "InApp";

    /// <summary>
    /// Tiêu đề nhắc lần đầu.
    /// </summary>
    [Required]
    public string FirstReminderSubject { get; set; } = "Nhắc gia hạn gói dịch vụ";

    /// <summary>
    /// Tiêu đề nhắc lần cuối (cận hạn).
    /// </summary>
    [Required]
    public string FinalReminderSubject { get; set; } = "Gói dịch vụ sắp hết hạn";
}
