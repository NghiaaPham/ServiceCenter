using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests
{
    /// <summary>
    /// Request DTO ?? skip m?t checklist item v?i lý do
    /// </summary>
    public class SkipChecklistItemRequestDto
    {
        /// <summary>
        /// ID c?a checklist item c?n skip
        /// </summary>
        [Required]
        public int ItemId { get; set; }

        /// <summary>
        /// WorkOrder ID (?? validate)
        /// </summary>
        [Required]
        public int WorkOrderId { get; set; }

        /// <summary>
        /// Lý do skip (b?t bu?c)
        /// VD: "Khách hàng t? ch?i", "Không áp d?ng cho xe này", "Thi?u ph? tùng"
        /// </summary>
        [Required(ErrorMessage = "Lý do skip là b?t bu?c")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do ph?i t? 10-500 ký t?")]
        public string SkipReason { get; set; } = null!;
    }
}
