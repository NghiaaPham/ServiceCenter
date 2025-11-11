namespace EVServiceCenter.Core.Domains.Payments.DTOs.Requests;

/// <summary>
/// MoMo payment gateway request parameters
/// Internal use - not exposed to API
/// </summary>
public class MoMoPaymentRequestDto
{
    /// <summary>
    /// Payment ID (maps to orderId)
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Payment code (e.g., PAY-20251023-0001)
    /// </summary>
    public string PaymentCode { get; set; } = null!;

    /// <summary>
    /// Amount in VND (integer only)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Order info/description
    /// </summary>
    public string OrderInfo { get; set; } = null!;

    /// <summary>
    /// Return URL after payment
    /// </summary>
    public string ReturnUrl { get; set; } = null!;

    /// <summary>
    /// IPN (Instant Payment Notification) URL for webhook
    /// </summary>
    public string IpnUrl { get; set; } = null!;

    /// <summary>
    /// Request type: captureWallet or payWithATM
    /// </summary>
    public string RequestType { get; set; } = "captureWallet";

    /// <summary>
    /// Extra data (JSON string)
    /// </summary>
    public string? ExtraData { get; set; }

    /// <summary>
    /// Auto capture (true/false)
    /// </summary>
    public bool AutoCapture { get; set; } = true;

    /// <summary>
    /// Language (vi or en)
    /// </summary>
    public string Lang { get; set; } = "vi";
}
