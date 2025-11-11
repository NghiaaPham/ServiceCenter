namespace EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

/// <summary>
/// MoMo IPN/Return callback parameters
/// </summary>
public class MoMoCallbackDto
{
    public string partnerCode { get; set; } = null!;
    public string orderId { get; set; } = null!;
    public string requestId { get; set; } = null!;
    public long amount { get; set; }
    public string orderInfo { get; set; } = null!;
    public string orderType { get; set; } = null!;
    public string transId { get; set; } = null!;
    public int resultCode { get; set; }
    public string message { get; set; } = null!;
    public string payType { get; set; } = null!;
    public long responseTime { get; set; }
    public string? extraData { get; set; }
    public string signature { get; set; } = null!;

    /// <summary>
    /// Check if payment was successful (resultCode = 0)
    /// </summary>
    public bool IsSuccess => resultCode == 0;

    /// <summary>
    /// Get response code as string (for consistency with VNPay)
    /// </summary>
    public string ResponseCode => resultCode.ToString();

    /// <summary>
    /// Get payment date from response time (Unix timestamp in milliseconds)
    /// </summary>
    public DateTime GetPaymentDate()
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(responseTime).DateTime;
    }
}
