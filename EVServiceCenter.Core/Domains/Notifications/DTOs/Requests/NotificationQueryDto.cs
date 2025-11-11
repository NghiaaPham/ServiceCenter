namespace EVServiceCenter.Core.Domains.Notifications.DTOs.Requests;

/// <summary>
/// Query parameters for notification list
/// </summary>
public class NotificationQueryDto
{
    /// <summary>
    /// Filter by read status (true = read, false = unread, null = all)
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Filter by status (Pending, Sent, Delivered, Failed)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by channel (Email, SMS, InApp, Push)
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Filter by priority (High, Medium, Low)
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Filter by related type (Appointment, WorkOrder, Invoice, Payment)
    /// </summary>
    public string? RelatedType { get; set; }

    /// <summary>
    /// Start date for filtering
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by (CreatedDate, SendDate, ReadDate)
    /// </summary>
    public string SortBy { get; set; } = "CreatedDate";

    /// <summary>
    /// Sort direction (Asc, Desc)
    /// </summary>
    public string SortDirection { get; set; } = "Desc";
}
