namespace EVServiceCenter.Core.Domains.Payments.DTOs.Requests;

/// <summary>
/// VNPay payment gateway request parameters
/// Internal use - not exposed to API
/// </summary>
public class VNPayPaymentRequestDto
{
    /// <summary>
    /// Payment ID (maps to vnp_TxnRef)
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// Payment code (e.g., PAY-20251023-0001)
    /// </summary>
    public string PaymentCode { get; set; } = null!;

    /// <summary>
    /// Amount in VND (no decimal, will be multiplied by 100 for VNPay)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Order info/description
    /// </summary>
    public string OrderInfo { get; set; } = null!;

    /// <summary>
    /// Return URL after payment
    /// </summary>
    public string ReturnUrl { get; set; } = null!;

    /// <summary>
    /// IPN URL for server-to-server notifications
    /// </summary>
    public string? IpnUrl { get; set; }

    /// <summary>
    /// IP address of customer
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// Optional: Customer name
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Optional: Customer email
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Optional: Customer phone
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Optional: Bank code for direct payment (e.g., NCB, VIETCOMBANK)
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// Payment category code (default: other)
    /// </summary>
    public string CategoryCode { get; set; } = "other";

    /// <summary>
    /// VNPay transaction reference (must be 6-32 alphanumeric characters)
    /// Generated from internal payment code to satisfy VNPay constraints.
    /// </summary>
    public string GatewayReference { get; set; } = null!;
}
