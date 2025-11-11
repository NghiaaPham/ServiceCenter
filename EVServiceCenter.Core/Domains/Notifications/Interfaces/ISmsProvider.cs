using EVServiceCenter.Core.Domains.Notifications.Models;

namespace EVServiceCenter.Core.Domains.Notifications.Interfaces;

/// <summary>
/// SMS provider abstraction for sending SMS notifications
/// Scalability: Supports multiple SMS gateways (Twilio, AWS SNS, local providers)
/// Performance: Async operations with rate limiting for API quotas
/// Cost-effectiveness: Tracks message counts and costs per provider
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Send a single SMS message
    /// Performance: Should complete within 3 seconds for most providers
    /// Cost: Typically 0.01-0.05 USD per message depending on provider and destination
    /// </summary>
    /// <param name="message">SMS message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Send result with message ID for tracking</returns>
    Task<SmsSendResult> SendSmsAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send multiple SMS messages in batch
    /// Performance: Batching reduces API overhead (up to 50% faster for 100+ messages)
    /// Scalability: Supports up to 500 messages per batch
    /// </summary>
    /// <param name="messages">Collection of SMS messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Results for each SMS with delivery status</returns>
    Task<List<SmsSendResult>> SendBatchSmsAsync(
        List<SmsMessage> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check delivery status of a sent message
    /// Maintainability: Enables delivery tracking and failed message retry logic
    /// </summary>
    /// <param name="messageId">Provider's message identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<SmsDeliveryStatus?> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify SMS provider connection and account balance
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current account balance (if supported by provider)
    /// Cost-effectiveness: Prevents sending failures due to insufficient credits
    /// </summary>
    Task<decimal?> GetAccountBalanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider name for logging and tracking
    /// </summary>
    string ProviderName { get; }
}

/// <summary>
/// SMS send operation result
/// </summary>
public class SmsSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime SentAt { get; set; }
    public string? RecipientPhone { get; set; }
    public int? SegmentCount { get; set; } // Number of SMS parts for long messages
    public decimal? Cost { get; set; } // Actual cost if provided by API
}
