using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests
{
    /// <summary>
    /// Request DTO ?? technician complete m?t checklist item
    /// </summary>
    public class CompleteChecklistItemRequestDto
    {
        /// <summary>
        /// ID c?a checklist item c?n complete
        /// </summary>
        [Required]
        public int ItemId { get; set; }

        /// <summary>
        /// WorkOrder ID (?? validate item thu?c v? work order này)
        /// </summary>
        [Required]
        public int WorkOrderId { get; set; }

        /// <summary>
        /// Ghi chú c?a technician khi complete item (optional)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// URL ?nh minh ch?ng (n?u có)
        /// </summary>
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
