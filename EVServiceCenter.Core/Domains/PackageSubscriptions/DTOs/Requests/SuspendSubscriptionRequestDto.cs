using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests
{
    /// <summary>
    /// DTO request ?? t?m d?ng subscription
    /// Customer c� th? t?m d?ng khi:
    /// - Xe ?ang s?a ch?a l?n
    /// - ?i c�ng t�c d�i h?n
    /// - T?m th?i kh�ng s? d?ng
    /// Staff c� th? t?m d?ng khi:
    /// - Ph�t hi?n gian l?n
    /// - Vi ph?m ch�nh s�ch
    /// </summary>
    public class SuspendSubscriptionRequestDto
    {
        /// <summary>
        /// L� do t?m d?ng subscription (b?t bu?c)
        /// VD: "Xe ?ang b?o h�nh l?n, t?m d?ng 2 th�ng"
        /// </summary>
        [Required(ErrorMessage = "L� do t?m d?ng kh�ng ???c ?? tr?ng")]
        [MinLength(10, ErrorMessage = "L� do ph?i c� �t nh?t 10 k� t? ?? ch�ng t�i hi?u r� t�nh hu?ng")]
        [MaxLength(500, ErrorMessage = "L� do kh�ng ???c v??t qu� 500 k� t?")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Ghi ch� b? sung (optional)
        /// VD: "D? ki?n quay l?i s? d?ng v�o th�ng 12/2025"
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Ghi ch� kh�ng ???c v??t qu� 1000 k� t?")]
        public string? Notes { get; set; }
    }
}
