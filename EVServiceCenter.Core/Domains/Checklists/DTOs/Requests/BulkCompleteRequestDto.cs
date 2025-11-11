using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Request DTO ?? bulk complete t?t c? checklist items
/// </summary>
public class BulkCompleteRequestDto
{
    /// <summary>
    /// Ghi chú chung cho t?t c? items
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes không ???c v??t quá 500 ký t?")]
    public string? Notes { get; set; }
}
