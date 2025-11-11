using EVServiceCenter.Core.Domains.Payments.Constants;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services;

/// <summary>
/// Mock VNPay service for development/demo without real credentials
/// Simulates payment gateway behavior for testing and demonstration
/// </summary>
public class MockVNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MockVNPayService> _logger;
    private readonly string _frontendUrl;

    public MockVNPayService(
        IConfiguration configuration,
        ILogger<MockVNPayService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _frontendUrl = configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";
    }

    /// <summary>
    /// Create mock payment URL that redirects to frontend mock payment page
    /// </summary>
    public string CreatePaymentUrl(VNPayPaymentRequestDto request)
    {
        _logger.LogInformation("🎭 MOCK MODE: Creating VNPay payment URL for {PaymentCode}", request.PaymentCode);

        // Build mock payment page URL with all necessary data
        var mockPaymentUrl = $"{_frontendUrl}/demo/vnpay-payment?" +
                            $"paymentCode={request.PaymentCode}&" +
                            $"amount={request.Amount}&" +
                            $"orderInfo={Uri.EscapeDataString(request.OrderInfo)}&" +
                            $"returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";

        _logger.LogInformation("🎭 MOCK MODE: Payment URL created - User will be redirected to mock page");

        return mockPaymentUrl;
    }

    /// <summary>
    /// Verify mock callback - always returns true in demo mode
    /// In real implementation, this would verify HMAC signature
    /// </summary>
    public bool VerifyCallback(VNPayCallbackDto callback)
    {
        _logger.LogInformation("🎭 MOCK MODE: Verifying VNPay callback for {TxnRef}", callback.vnp_TxnRef);

        // In mock mode, we accept all callbacks as valid
        // This simulates successful signature verification
        var isSuccess = callback.vnp_ResponseCode == "00" && callback.vnp_TransactionStatus == "00";

        _logger.LogInformation("🎭 MOCK MODE: Callback verification result: {Result}",
            isSuccess ? "SUCCESS" : "FAILURE");

        return true; // Always return true - signature is valid in mock mode
    }

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    public bool VerifyCallbackAndExtractPaymentCode(VNPayCallbackDto callback, out string paymentCode)
    {
        paymentCode = callback.vnp_TxnRef;

        _logger.LogInformation("🎭 MOCK MODE: Extracted payment code {PaymentCode}", paymentCode);

        return VerifyCallback(callback);
    }

    /// <summary>
    /// Get response message from VNPay response code
    /// </summary>
    public string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            PaymentResponseCode.Success => "🎭 MOCK: Giao dịch thành công",
            PaymentResponseCode.VNPay_Suspicious => "🎭 MOCK: Giao dịch nghi ngờ gian lận",
            PaymentResponseCode.VNPay_NotRegistered => "🎭 MOCK: Thẻ chưa đăng ký Internet Banking",
            PaymentResponseCode.VNPay_AuthFailed => "🎭 MOCK: Xác thực thất bại",
            PaymentResponseCode.VNPay_Timeout => "🎭 MOCK: Giao dịch hết hạn",
            PaymentResponseCode.VNPay_InvalidCard => "🎭 MOCK: Thẻ không hợp lệ",
            PaymentResponseCode.VNPay_InvalidAmount => "🎭 MOCK: Số tiền không hợp lệ",
            PaymentResponseCode.VNPay_InsufficientFunds => "🎭 MOCK: Tài khoản không đủ số dư",
            PaymentResponseCode.VNPay_ExceededLimit => "🎭 MOCK: Vượt quá hạn mức giao dịch",
            PaymentResponseCode.VNPay_Maintenance => "🎭 MOCK: Ngân hàng đang bảo trì",
            PaymentResponseCode.VNPay_InvalidPassword => "🎭 MOCK: Sai mật khẩu OTP",
            _ => $"🎭 MOCK: Lỗi không xác định (Code: {responseCode})"
        };
    }
}
