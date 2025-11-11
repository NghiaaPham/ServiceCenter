using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for quality check by Staff/Admin
/// </summary>
public class QualityCheckRequestDto
{
    /// <summary>
    /// Quality rating from staff (1-5)
    /// </summary>
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    /// <summary>
    /// Quality check notes/comments
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
