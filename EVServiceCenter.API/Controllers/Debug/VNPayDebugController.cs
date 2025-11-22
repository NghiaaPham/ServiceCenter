using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace EVServiceCenter.API.Controllers.Debug;

/// <summary>
/// VNPay Debug Controller - FOR TESTING ONLY
/// Helps verify VNPay signature generation
/// </summary>
[ApiController]
[Route("api/debug/vnpay")]
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger in production
public class VNPayDebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VNPayDebugController> _logger;

    public VNPayDebugController(IConfiguration configuration, ILogger<VNPayDebugController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Test signature generation with sample data
    /// </summary>
    [HttpGet("test-signature")]
    public IActionResult TestSignature([FromQuery] decimal amount = 183600)
    {
        try
        {
            var tmnCode = _configuration["VNPay:TmnCode"];
            var hashSecret = _configuration["VNPay:HashSecret"];
            
            // Vietnam Time
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vnTimeZone);
            var createDate = vnTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var expireDate = vnTime.AddMinutes(30).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            // Sample parameters (sorted alphabetically)
            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Amount", ((long)(amount * 100)).ToString(CultureInfo.InvariantCulture) },
                { "vnp_Command", "pay" },
                { "vnp_CreateDate", createDate },
                { "vnp_CurrCode", "VND" },
                { "vnp_ExpireDate", expireDate },
                { "vnp_IpAddr", "118.69.182.149" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", "Test payment" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", _configuration["VNPay:ReturnUrl"]! },
                { "vnp_TmnCode", tmnCode! },
                { "vnp_TxnRef", "TEST123456" },
                { "vnp_Version", "2.1.0" }
            };

            // Build signature data
            var signData = string.Join("&", parameters.Select(kv => $"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}"));

            // Compute HMAC-SHA512
            var signature = ComputeHmacSha512(signData, hashSecret!);

            _logger.LogInformation(
                "VNPay Signature Test:\n" +
                "CreateDate: {CreateDate} (Length: {Length})\n" +
                "SignData: {SignData}\n" +
                "Signature: {Signature}",
                createDate,
                createDate.Length,
                signData,
                signature);

            return Ok(new
            {
                success = true,
                data = new
                {
                    tmnCode,
                    hashSecret = MaskSecret(hashSecret!),
                    vnTime = vnTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    createDate,
                    createDateLength = createDate.Length,
                    expireDate,
                    parameters,
                    signData,
                    signature,
                    url = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?{signData}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={signature}"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing VNPay signature");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private static string ComputeHmacSha512(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToUpperInvariant();
    }

    private static string MaskSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret) || secret.Length <= 8)
            return "****";
        return secret[..4] + "..." + secret[^4..];
    }
}
