using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces;

/// <summary>
/// VNPay payment gateway service interface
/// </summary>
public interface IVNPayService
{
    /// <summary>
    /// Create VNPay payment URL for redirecting customer
    /// </summary>
    /// <param name="request">VNPay payment request parameters</param>
    /// <returns>Payment URL with signed parameters</returns>
    string CreatePaymentUrl(VNPayPaymentRequestDto request);

    /// <summary>
    /// Verify VNPay callback signature (IPN or Return URL)
    /// </summary>
    /// <param name="callback">Callback parameters from VNPay</param>
    /// <returns>True if signature is valid</returns>
    bool VerifyCallback(VNPayCallbackDto callback);

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    /// <param name="callback">Callback parameters</param>
    /// <param name="paymentCode">Extracted payment code (vnp_TxnRef)</param>
    /// <returns>True if valid</returns>
    bool VerifyCallbackAndExtractPaymentCode(VNPayCallbackDto callback, out string paymentCode);

    /// <summary>
    /// Get human-readable error message from response code
    /// </summary>
    /// <param name="responseCode">VNPay response code</param>
    /// <returns>Error message in Vietnamese</returns>
    string GetResponseMessage(string responseCode);
}
