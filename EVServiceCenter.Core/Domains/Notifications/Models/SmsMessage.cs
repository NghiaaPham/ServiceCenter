namespace EVServiceCenter.Core.Domains.Notifications.Models;

/// <summary>
/// SMS message model for sending notifications
/// Performance: SMS messages are limited to 160 characters for optimal delivery
/// Scalability: Supports batching for bulk SMS operations
/// </summary>
public class SmsMessage
{
    /// <summary>
    /// Recipient phone number (E.164 format recommended: +84xxxxxxxxx)
    /// </summary>
    public string To { get; set; } = null!;

    /// <summary>
    /// Message content (max 160 chars for single SMS, 1530 for concatenated)
    /// Performance: Keep under 160 chars to avoid multi-part SMS charges
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Optional sender ID (brand name or phone number)
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Message priority for queue processing
    /// </summary>
    public SmsPriority Priority { get; set; } = SmsPriority.Normal;

    /// <summary>
    /// Optional scheduled send time (for delayed delivery)
    /// </summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>
    /// Whether to request delivery receipt
    /// Performance: Delivery receipts may incur additional costs
    /// </summary>
    public bool RequestDeliveryReceipt { get; set; } = false;

    /// <summary>
    /// Message validity period in minutes (default: 1440 = 24 hours)
    /// </summary>
    public int ValidityPeriodMinutes { get; set; } = 1440;

    /// <summary>
    /// Custom metadata for tracking and analytics
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// SMS priority levels for queue processing
/// </summary>
public enum SmsPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// SMS delivery status tracking
/// </summary>
public class SmsDeliveryStatus
{
    public string MessageId { get; set; } = null!;
    public string Status { get; set; } = null!; // Sent, Delivered, Failed, Pending
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? Cost { get; set; }
}
