using EVServiceCenter.Core.Domains.Notifications.Models;

namespace EVServiceCenter.Core.Domains.Notifications.Interfaces;

/// <summary>
/// Email provider abstraction for sending email notifications
/// Scalability: Supports multiple email providers (SMTP, SendGrid, AWS SES, etc.)
/// Maintainability: Provider-agnostic interface for easy swapping
/// Performance: Async operations with cancellation support
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Send a single email message
    /// Performance: Should complete within 5 seconds for local SMTP, 2 seconds for cloud providers
    /// </summary>
    /// <param name="message">Email message to send</param>
    /// <param name="cancellationToken">Cancellation token for timeout handling</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<EmailSendResult> SendEmailAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send multiple email messages in batch
    /// Performance: Batching improves throughput for bulk operations (100+ emails)
    /// Scalability: Supports up to 1000 emails per batch
    /// </summary>
    /// <param name="messages">Collection of email messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Results for each email with success/failure status</returns>
    Task<List<EmailSendResult>> SendBatchEmailAsync(
        List<EmailMessage> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify email provider connection and credentials
    /// Maintainability: Health check for monitoring and diagnostics
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider name for logging and tracking
    /// </summary>
    string ProviderName { get; }
}

/// <summary>
/// Email send operation result
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime SentAt { get; set; }
    public string? RecipientEmail { get; set; }
}
