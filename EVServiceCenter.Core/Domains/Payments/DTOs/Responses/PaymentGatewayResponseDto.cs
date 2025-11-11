namespace EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

/// <summary>
/// Generic payment gateway response (for redirecting user to payment page)
/// </summary>
public class PaymentGatewayResponseDto
{
    /// <summary>
    /// Payment ID in our system
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Payment code (e.g., PAY-20251023-0001)
    /// </summary>
    public string PaymentCode { get; set; } = null!;

    /// <summary>
    /// Gateway name (VNPay, MoMo)
    /// </summary>
    public string Gateway { get; set; } = null!;

    /// <summary>
    /// Payment URL to redirect user to
    /// </summary>
    public string PaymentUrl { get; set; } = null!;

    /// <summary>
    /// Optional: QR code data (for MoMo)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Optional: Deep link for mobile app
    /// </summary>
    public string? DeepLink { get; set; }

    /// <summary>
    /// Expiry time (if applicable)
    /// </summary>
    public DateTime? ExpiryTime { get; set; }
}
