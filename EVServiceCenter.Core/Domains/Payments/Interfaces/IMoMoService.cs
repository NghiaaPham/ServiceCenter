using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces;

/// <summary>
/// MoMo payment gateway service interface
/// </summary>
public interface IMoMoService
{
    /// <summary>
    /// Create MoMo payment request and get payment URL/QR code
    /// </summary>
    /// <param name="request">MoMo payment request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment URL and QR code data</returns>
    Task<PaymentGatewayResponseDto> CreatePaymentAsync(MoMoPaymentRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify MoMo callback signature (IPN or Return URL)
    /// </summary>
    /// <param name="callback">Callback parameters from MoMo</param>
    /// <returns>True if signature is valid</returns>
    bool VerifyCallback(MoMoCallbackDto callback);

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    /// <param name="callback">Callback parameters</param>
    /// <param name="paymentCode">Extracted payment code (orderId)</param>
    /// <returns>True if valid</returns>
    bool VerifyCallbackAndExtractPaymentCode(MoMoCallbackDto callback, out string paymentCode);

    /// <summary>
    /// Query payment status from MoMo (for reconciliation)
    /// </summary>
    /// <param name="orderId">Order ID (payment code)</param>
    /// <param name="requestId">Original request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment status details</returns>
    Task<MoMoCallbackDto> QueryPaymentStatusAsync(string orderId, string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get human-readable error message from result code
    /// </summary>
    /// <param name="resultCode">MoMo result code</param>
    /// <returns>Error message in Vietnamese</returns>
    string GetResponseMessage(int resultCode);
}
