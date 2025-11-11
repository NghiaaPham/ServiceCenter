using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services;

/// <summary>
/// Mock MoMo service for development/demo without real credentials
/// Simulates payment gateway behavior for testing and demonstration
/// </summary>
public class MockMoMoService : IMoMoService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MockMoMoService> _logger;
    private readonly string _frontendUrl;

    public MockMoMoService(
        IConfiguration configuration,
        ILogger<MockMoMoService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _frontendUrl = configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";
    }

    /// <summary>
    /// Create mock MoMo payment request
    /// Returns mock payment URL that redirects to frontend mock payment page
    /// </summary>
    public async Task<PaymentGatewayResponseDto> CreatePaymentAsync(
        MoMoPaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🎭 MOCK MODE: Creating MoMo payment for {PaymentCode}", request.PaymentCode);

        // Simulate async API call
        await Task.Delay(100, cancellationToken);

        // Build mock payment page URL
        var mockPaymentUrl = $"{_frontendUrl}/demo/momo-payment?" +
                            $"paymentCode={request.PaymentCode}&" +
                            $"amount={request.Amount}&" +
                            $"orderInfo={Uri.EscapeDataString(request.OrderInfo)}&" +
                            $"returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";

        _logger.LogInformation("🎭 MOCK MODE: MoMo payment URL created");

        return new PaymentGatewayResponseDto
        {
            PaymentId = request.PaymentId,
            PaymentCode = request.PaymentCode,
            Gateway = "MoMo",
            PaymentUrl = mockPaymentUrl,
            QrCodeUrl = $"{_frontendUrl}/demo/momo-qr?code={request.PaymentCode}", // Mock QR
            DeepLink = $"momo://app?action=payWithApp&code={request.PaymentCode}", // Mock deep link
            ExpiryTime = DateTime.UtcNow.AddMinutes(15)
        };
    }

    /// <summary>
    /// Verify mock MoMo callback - always returns true in demo mode
    /// </summary>
    public bool VerifyCallback(MoMoCallbackDto callback)
    {
        _logger.LogInformation("🎭 MOCK MODE: Verifying MoMo callback for {OrderId}", callback.orderId);

        // In mock mode, accept all callbacks as valid
        var isSuccess = callback.resultCode == 0;

        _logger.LogInformation("🎭 MOCK MODE: Callback verification result: {Result}",
            isSuccess ? "SUCCESS" : "FAILURE");

        return true; // Always return true - signature is valid in mock mode
    }

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    public bool VerifyCallbackAndExtractPaymentCode(MoMoCallbackDto callback, out string paymentCode)
    {
        paymentCode = callback.orderId;

        _logger.LogInformation("🎭 MOCK MODE: Extracted payment code {PaymentCode}", paymentCode);

        return VerifyCallback(callback);
    }

    /// <summary>
    /// Mock query payment status
    /// </summary>
    public async Task<MoMoCallbackDto> QueryPaymentStatusAsync(
        string orderId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🎭 MOCK MODE: Querying MoMo payment status for {OrderId}", orderId);

        // Simulate async API call
        await Task.Delay(100, cancellationToken);

        // Return mock pending status
        return new MoMoCallbackDto
        {
            orderId = orderId,
            requestId = requestId,
            resultCode = 10, // Transaction initiated, waiting for user confirmation
            message = "🎭 MOCK: Giao dịch đang chờ xác nhận",
            orderInfo = "Mock Order Info",
            amount = 0,
            responseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }

    /// <summary>
    /// Get response message from MoMo result code
    /// </summary>
    public string GetResponseMessage(int resultCode)
    {
        return resultCode switch
        {
            0 => "🎭 MOCK: Giao dịch thành công",
            9 => "🎭 MOCK: Giao dịch thất bại",
            10 => "🎭 MOCK: Giao dịch được khởi tạo, chờ người dùng xác nhận thanh toán",
            11 => "🎭 MOCK: Truy cập bị từ chối",
            12 => "🎭 MOCK: Phiên bản API không được hỗ trợ",
            13 => "🎭 MOCK: Xác thực dữ liệu thất bại",
            20 => "🎭 MOCK: Giao dịch không tồn tại",
            21 => "🎭 MOCK: Giao dịch đã hết hạn",
            1001 => "🎭 MOCK: Tài khoản không đủ số dư",
            1002 => "🎭 MOCK: Giao dịch hết thời gian chờ",
            1003 => "🎭 MOCK: Tài khoản không hợp lệ",
            1004 => "🎭 MOCK: Người dùng hủy giao dịch",
            1005 => "🎭 MOCK: Giao dịch thất bại",
            9000 => "🎭 MOCK: Lỗi hệ thống",
            _ => $"🎭 MOCK: Lỗi không xác định (Code: {resultCode})"
        };
    }
}
