using System.Collections.Generic;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;

/// <summary>
/// DTO mô tả subscription có thể áp dụng cho một danh sách dịch vụ.
/// Bao gồm subscription phù hợp và danh sách serviceId hợp lệ/không hợp lệ.
/// </summary>
public class ApplicableSubscriptionResultDto
{
    /// <summary>
    /// Subscription đáp ứng yêu cầu (có thể null nếu không tìm thấy).
    /// </summary>
    public PackageSubscriptionSummaryDto? Subscription { get; set; }

    /// <summary>
    /// Các serviceId thỏa điều kiện sử dụng subscription.
    /// </summary>
    public List<int> ApplicableServiceIds { get; set; } = new();

    /// <summary>
    /// Các serviceId không thể áp dụng subscription (không tìm thấy hoặc vượt số lượt).
    /// </summary>
    public List<int> InvalidServiceIds { get; set; } = new();
}
