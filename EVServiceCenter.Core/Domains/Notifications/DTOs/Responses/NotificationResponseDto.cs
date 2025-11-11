namespace EVServiceCenter.Core.Domains.Notifications.DTOs.Responses;

/// <summary>
/// Notification response DTO
/// </summary>
public class NotificationResponseDto
{
    public int NotificationId { get; set; }
    public string NotificationCode { get; set; } = null!;
    public string Channel { get; set; } = null!;
    public string? Priority { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = null!;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string? Status { get; set; }
    public string? RelatedType { get; set; }
    public int? RelatedId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsRead => ReadDate.HasValue;
    public bool IsDelivered => DeliveredDate.HasValue;
}

/// <summary>
/// Paginated notification list response
/// </summary>
public class NotificationListResponseDto
{
    public List<NotificationResponseDto> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
