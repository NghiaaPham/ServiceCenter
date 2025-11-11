using EVServiceCenter.Core.Domains.Payments.Constants;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services;

/// <summary>
/// MoMo payment gateway service implementation
/// Handles API calls, signature generation, and callback verification
/// </summary>
public class MoMoService : IMoMoService
{
    private readonly string _endpoint;
    private readonly string _partnerCode;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MoMoService> _logger;
    private readonly HashSet<string> _allowedReturnUrls;

    public MoMoService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<MoMoService> logger)
    {
        _endpoint = configuration["MoMo:Endpoint"] ?? throw new ArgumentNullException("MoMo:Endpoint not configured");
        _partnerCode = configuration["MoMo:PartnerCode"] ?? throw new ArgumentNullException("MoMo:PartnerCode not configured");
        _accessKey = configuration["MoMo:AccessKey"] ?? throw new ArgumentNullException("MoMo:AccessKey not configured");
        _secretKey = configuration["MoMo:SecretKey"] ?? throw new ArgumentNullException("MoMo:SecretKey not configured");
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // ✅ GAP 3 FIX: Load whitelisted return URLs for security
        var allowedUrls = configuration.GetSection("MoMo:AllowedReturnUrls").Get<string[]>() ?? Array.Empty<string>();
        _allowedReturnUrls = new HashSet<string>(allowedUrls, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create MoMo payment request via API
    /// </summary>
    public async Task<PaymentGatewayResponseDto> CreatePaymentAsync(
        MoMoPaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ GAP 3 FIX: Validate return URL against whitelist for security
            ValidateReturnUrl(request.ReturnUrl);

            // Generate request ID
            var requestId = Guid.NewGuid().ToString();

            // Build raw signature data
            var rawSignature = BuildPaymentSignatureData(
                requestId,
                request.PaymentCode,
                request.Amount,
                request.OrderInfo,
                request.ReturnUrl,
                request.IpnUrl,
                request.ExtraData ?? string.Empty);

            // Compute signature
            var signature = ComputeHmacSha256(rawSignature, _secretKey);

            // Build request body
            var requestBody = new
            {
                partnerCode = _partnerCode,
                partnerName = "EV Service Center",
                storeId = _partnerCode,
                requestId,
                amount = request.Amount,
                orderId = request.PaymentCode,
                orderInfo = request.OrderInfo,
                redirectUrl = request.ReturnUrl,
                ipnUrl = request.IpnUrl,
                requestType = request.RequestType,
                extraData = request.ExtraData ?? string.Empty,
                lang = request.Lang,
                autoCapture = request.AutoCapture,
                signature
            };

            // Send HTTP POST request to MoMo
            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(_endpoint, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("MoMo API Response: {Response}", responseBody);

            // Parse response
            var momoResponse = JsonSerializer.Deserialize<MoMoApiResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (momoResponse == null)
                throw new Exception("Failed to parse MoMo response");

            // Check result code
            if (momoResponse.ResultCode != 0)
            {
                _logger.LogWarning("MoMo payment creation failed. Code: {Code}, Message: {Message}",
                    momoResponse.ResultCode, momoResponse.Message);

                throw new InvalidOperationException(
                    $"MoMo payment failed: {GetResponseMessage(momoResponse.ResultCode)}");
            }

            // Return payment URL and QR code
            return new PaymentGatewayResponseDto
            {
                PaymentId = request.PaymentId,
                PaymentCode = request.PaymentCode,
                Gateway = "MoMo",
                PaymentUrl = momoResponse.PayUrl ?? string.Empty,
                QrCodeUrl = momoResponse.QrCodeUrl,
                DeepLink = momoResponse.DeepLink,
                ExpiryTime = DateTime.UtcNow.AddMinutes(15) // MoMo default: 15 minutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment for {PaymentCode}", request.PaymentCode);
            throw;
        }
    }

    /// <summary>
    /// Verify MoMo callback signature
    /// </summary>
    public bool VerifyCallback(MoMoCallbackDto callback)
    {
        var receivedSignature = callback.signature;

        // Build raw signature data
        var rawSignature = BuildCallbackSignatureData(
            callback.requestId,
            callback.orderId,
            callback.amount,
            callback.orderInfo,
            callback.orderType,
            callback.transId,
            callback.resultCode,
            callback.message,
            callback.payType,
            callback.responseTime,
            callback.extraData ?? string.Empty);

        // Compute signature
        var computedSignature = ComputeHmacSha256(rawSignature, _secretKey);

        // Compare signatures
        return string.Equals(receivedSignature, computedSignature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    public bool VerifyCallbackAndExtractPaymentCode(MoMoCallbackDto callback, out string paymentCode)
    {
        paymentCode = callback.orderId;
        return VerifyCallback(callback);
    }

    /// <summary>
    /// Query payment status from MoMo API
    /// </summary>
    public async Task<MoMoCallbackDto> QueryPaymentStatusAsync(
        string orderId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build raw signature for query
            var rawSignature = $"accessKey={_accessKey}&orderId={orderId}&partnerCode={_partnerCode}&requestId={requestId}";
            var signature = ComputeHmacSha256(rawSignature, _secretKey);

            // Build request body
            var requestBody = new
            {
                partnerCode = _partnerCode,
                requestId,
                orderId,
                lang = "vi",
                signature
            };

            // Send HTTP POST request
            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var queryEndpoint = _endpoint.Replace("/create", "/query");
            var response = await httpClient.PostAsync(queryEndpoint, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse response
            var momoResponse = JsonSerializer.Deserialize<MoMoCallbackDto>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return momoResponse ?? throw new Exception("Failed to parse MoMo query response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying MoMo payment status for {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Get human-readable error message from MoMo result code
    /// </summary>
    public string GetResponseMessage(int resultCode)
    {
        return resultCode switch
        {
            0 => "Giao dịch thành công",
            9 => "Giao dịch thất bại",
            10 => "Giao dịch được khởi tạo, chờ người dùng xác nhận thanh toán",
            11 => "Truy cập bị từ chối",
            12 => "Phiên bản API không được hỗ trợ",
            13 => "Xác thực dữ liệu thất bại",
            20 => "Giao dịch không tồn tại",
            21 => "Giao dịch đã hết hạn",
            1001 => "Tài khoản không đủ số dư",
            1002 => "Giao dịch hết thời gian chờ",
            1003 => "Tài khoản không hợp lệ",
            1004 => "Người dùng hủy giao dịch",
            1005 => "Giao dịch thất bại",
            9000 => "Lỗi hệ thống",
            _ => $"Lỗi không xác định (Code: {resultCode})"
        };
    }

    #region Private Helper Methods

    /// <summary>
    /// Build signature data for payment creation request
    /// Format: accessKey=X&amount=X&extraData=X&ipnUrl=X&orderId=X&orderInfo=X&partnerCode=X&redirectUrl=X&requestId=X&requestType=X
    /// </summary>
    private string BuildPaymentSignatureData(
        string requestId,
        string orderId,
        long amount,
        string orderInfo,
        string redirectUrl,
        string ipnUrl,
        string extraData)
    {
        return $"accessKey={_accessKey}" +
               $"&amount={amount}" +
               $"&extraData={extraData}" +
               $"&ipnUrl={ipnUrl}" +
               $"&orderId={orderId}" +
               $"&orderInfo={orderInfo}" +
               $"&partnerCode={_partnerCode}" +
               $"&redirectUrl={redirectUrl}" +
               $"&requestId={requestId}" +
               $"&requestType=captureWallet";
    }

    /// <summary>
    /// Build signature data for callback verification
    /// </summary>
    private string BuildCallbackSignatureData(
        string requestId,
        string orderId,
        long amount,
        string orderInfo,
        string orderType,
        string transId,
        int resultCode,
        string message,
        string payType,
        long responseTime,
        string extraData)
    {
        return $"accessKey={_accessKey}" +
               $"&amount={amount}" +
               $"&extraData={extraData}" +
               $"&message={message}" +
               $"&orderId={orderId}" +
               $"&orderInfo={orderInfo}" +
               $"&orderType={orderType}" +
               $"&partnerCode={_partnerCode}" +
               $"&payType={payType}" +
               $"&requestId={requestId}" +
               $"&responseTime={responseTime}" +
               $"&resultCode={resultCode}" +
               $"&transId={transId}";
    }

    /// <summary>
    /// Compute HMAC SHA-256 signature
    /// </summary>
    private static string ComputeHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        // Convert to hex string (lowercase)
        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Validate return URL against whitelist to prevent phishing attacks
    /// ✅ GAP 3 FIX: Security measure to ensure only trusted URLs can be used
    /// </summary>
    private void ValidateReturnUrl(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
            throw new ArgumentException("Return URL cannot be null or empty", nameof(returnUrl));

        // Parse URL to get base URL without query string
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid return URL format: {returnUrl}", nameof(returnUrl));

        // Check if the base URL is in the whitelist
        var baseUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}".TrimEnd('/');

        if (!_allowedReturnUrls.Contains(baseUrl))
        {
            throw new SecurityException(
                $"Return URL '{baseUrl}' is not in the allowed whitelist. " +
                $"Please configure allowed return URLs in appsettings.json under MoMo:AllowedReturnUrls");
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// MoMo API response model for payment creation
    /// </summary>
    private class MoMoApiResponse
    {
        public string? PartnerCode { get; set; }
        public string? RequestId { get; set; }
        public string? OrderId { get; set; }
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string? Message { get; set; }
        public int ResultCode { get; set; }
        public string? PayUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? DeepLink { get; set; }
    }

    #endregion
}
