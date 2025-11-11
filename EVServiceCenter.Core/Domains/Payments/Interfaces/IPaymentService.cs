using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces;

/// <summary>
/// Payment orchestration service - handles payment lifecycle
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create payment and get gateway URL (for VNPay, MoMo) or record manual payment
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <param name="createdBy">User ID creating the payment</param>
    /// <param name="ipAddress">Customer IP address (for gateway)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment gateway response with URL or payment details for manual payment</returns>
    Task<PaymentGatewayResponseDto> CreatePaymentAsync(
        CreatePaymentRequestDto request,
        int createdBy,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process VNPay callback (IPN or Return URL)
    /// </summary>
    /// <param name="callback">VNPay callback parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if payment processed successfully</returns>
    Task<PaymentCallbackResult> ProcessVNPayCallbackAsync(VNPayCallbackDto callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process MoMo callback (IPN or Return URL)
    /// </summary>
    /// <param name="callback">MoMo callback parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if payment processed successfully</returns>
    Task<bool> ProcessMoMoCallbackAsync(MoMoCallbackDto callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record manual payment (Cash, BankTransfer)
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <param name="createdBy">User ID recording the payment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recorded payment details</returns>
    Task<PaymentResponseDto> RecordManualPaymentAsync(
        CreatePaymentRequestDto request,
        int createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment by ID
    /// </summary>
    Task<PaymentResponseDto?> GetPaymentByIdAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment by code
    /// </summary>
    Task<PaymentResponseDto?> GetPaymentByCodeAsync(string paymentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all payments for an invoice
    /// </summary>
    Task<List<PaymentResponseDto>> GetPaymentsByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
}

