namespace EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

/// <summary>
/// VNPay IPN/Return callback parameters
/// </summary>
public class VNPayCallbackDto
{
    public string vnp_TmnCode { get; set; } = null!;
    public string vnp_Amount { get; set; } = null!;
    public string vnp_BankCode { get; set; } = null!;
    public string? vnp_BankTranNo { get; set; }
    public string? vnp_CardType { get; set; }
    public string vnp_PayDate { get; set; } = null!;
    public string vnp_OrderInfo { get; set; } = null!;
    public string vnp_TransactionNo { get; set; } = null!;
    public string vnp_ResponseCode { get; set; } = null!;
    public string vnp_TransactionStatus { get; set; } = null!;
    public string vnp_TxnRef { get; set; } = null!;
    public string vnp_SecureHash { get; set; } = null!;

    /// <summary>
    /// Check if payment was successful
    /// </summary>
    public bool IsSuccess => vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";

    /// <summary>
    /// Get parsed amount (divide by 100 to get actual VND)
    /// </summary>
    public decimal GetAmount()
    {
        if (decimal.TryParse(vnp_Amount, out var amount))
            return amount / 100;
        return 0;
    }

    /// <summary>
    /// Get parsed transaction date
    /// </summary>
    public DateTime? GetPaymentDate()
    {
        if (DateTime.TryParseExact(vnp_PayDate, "yyyyMMddHHmmss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var date))
            return date;
        return null;
    }
}
