using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests
{
    /// <summary>
    /// DTO request ?? t?m d?ng subscription
    /// Customer có th? t?m d?ng khi:
    /// - Xe ?ang s?a ch?a l?n
    /// - ?i công tác dài h?n
    /// - T?m th?i không s? d?ng
    /// Staff có th? t?m d?ng khi:
    /// - Phát hi?n gian l?n
    /// - Vi ph?m chính sách
    /// </summary>
    public class SuspendSubscriptionRequestDto
    {
        /// <summary>
        /// Lý do t?m d?ng subscription (b?t bu?c)
        /// VD: "Xe ?ang b?o hành l?n, t?m d?ng 2 tháng"
        /// </summary>
        [Required(ErrorMessage = "Lý do t?m d?ng không ???c ?? tr?ng")]
        [MinLength(10, ErrorMessage = "Lý do ph?i có ít nh?t 10 ký t? ?? chúng tôi hi?u rõ tình hu?ng")]
        [MaxLength(500, ErrorMessage = "Lý do không ???c v??t quá 500 ký t?")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Ghi chú b? sung (optional)
        /// VD: "D? ki?n quay l?i s? d?ng vào tháng 12/2025"
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Ghi chú không ???c v??t quá 1000 ký t?")]
        public string? Notes { get; set; }
    }
}
