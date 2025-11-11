namespace EVServiceCenter.Core.Domains.Notifications.Models;

/// <summary>
/// Email message model for sending notifications
/// Maintainability: Encapsulates all email-related data in a single model
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string To { get; set; } = null!;

    /// <summary>
    /// Optional recipient name for personalization
    /// </summary>
    public string? ToName { get; set; }

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Email body content (supports HTML)
    /// </summary>
    public string Body { get; set; } = null!;

    /// <summary>
    /// Whether body contains HTML markup
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Optional CC recipients
    /// </summary>
    public List<string>? CC { get; set; }

    /// <summary>
    /// Optional BCC recipients
    /// </summary>
    public List<string>? BCC { get; set; }

    /// <summary>
    /// Optional file attachments (file paths or URLs)
    /// Performance: Keep attachments small to avoid memory issues
    /// </summary>
    public List<EmailAttachment>? Attachments { get; set; }

    /// <summary>
    /// Priority level for email delivery
    /// </summary>
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;

    /// <summary>
    /// Custom headers for tracking and routing
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }
}

/// <summary>
/// Email attachment model
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = null!;
}

/// <summary>
/// Email priority levels
/// </summary>
public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
