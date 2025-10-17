using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    /// <summary>
    /// DTO để thêm dịch vụ phát sinh vào appointment đang InProgress
    /// </summary>
    public class AddServicesRequestDto
    {
        /// <summary>
        /// Danh sách Service IDs cần thêm
        /// </summary>
        [Required(ErrorMessage = "Danh sách dịch vụ không được rỗng")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 dịch vụ")]
        public List<int> ServiceIds { get; set; } = new();

        /// <summary>
        /// Ghi chú về lý do thêm dịch vụ (optional)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
