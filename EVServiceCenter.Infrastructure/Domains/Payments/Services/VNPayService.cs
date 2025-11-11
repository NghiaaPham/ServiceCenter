using EVServiceCenter.Core.Domains.Payments.Constants;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services;

/// <summary>
/// VNPay payment gateway service implementation
/// Handles URL generation, signature creation, and callback verification
/// </summary>
public class VNPayService : IVNPayService
{
    private readonly ILogger<VNPayService> _logger;

    private readonly string _vnpUrl;
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _version;
    private readonly HashSet<string> _allowedReturnUrls;
    private readonly TimeZoneInfo _vnTimeZone;

    public VNPayService(IConfiguration configuration, ILogger<VNPayService> logger)
    {
        _logger = logger;

        _vnpUrl = configuration["VNPay:Url"] ?? throw new ArgumentNullException("VNPay:Url not configured");
        _tmnCode = configuration["VNPay:TmnCode"] ?? throw new ArgumentNullException("VNPay:TmnCode not configured");
        _hashSecret = configuration["VNPay:HashSecret"] ?? throw new ArgumentNullException("VNPay:HashSecret not configured");
        _version = configuration["VNPay:Version"] ?? "2.1.0";
        _vnTimeZone = ResolveVietnamTimeZone(configuration["VNPay:TimeZoneId"]);

        // ? GAP 3 FIX: Load whitelisted return URLs for security
        var allowedUrls = configuration.GetSection("VNPay:AllowedReturnUrls").Get<string[]>() ?? Array.Empty<string>();
        _allowedReturnUrls = new HashSet<string>(
            allowedUrls
                .Select(NormalizeBaseUrl)
                .Where(url => !string.IsNullOrEmpty(url)),
            StringComparer.OrdinalIgnoreCase);

        AddAllowedCallbackUrl(configuration["VNPay:ReturnUrl"]);
        AddAllowedCallbackUrl(configuration["VNPay:IpnUrl"]);
    }

    /// <summary>
    /// Create VNPay payment URL with all required parameters and signature
    /// </summary>
    public string CreatePaymentUrl(VNPayPaymentRequestDto request)
    {
        // ? GAP 3 FIX: Validate return URL against whitelist for security
        ValidateReturnUrl(request.ReturnUrl);
        if (!string.IsNullOrWhiteSpace(request.IpnUrl))
        {
            ValidateReturnUrl(request.IpnUrl);
        }

        // Build parameter dictionary
        var vnpParams = BuildPaymentParameters(request);

        // ?? DEBUG: Log parameters BEFORE signing
        _logger.LogInformation(
            "?? VNPay Parameters BEFORE signing:\n{Params}",
            string.Join("\n", vnpParams.OrderBy(kv => kv.Key).Select(kv => $"  {kv.Key} = {kv.Value}")));

        var queryString = BuildSignedQueryString(vnpParams, out var signData, out var signature);
        
        // ?? DEBUG: Log signature details
        _logger.LogInformation(
            "?? VNPay Signature Debug:\n" +
            "SignData (RAW): {SignData}\n" +
            "HashSecret: {Secret}\n" +
            "Generated Hash: {Hash}",
            signData,
            MaskSecret(_hashSecret),
            signature);

        var finalUrl = $"{_vnpUrl}?{queryString}";
        
        // ?? DEBUG: Log final URL (masked)
        _logger.LogInformation(
            "?? VNPay Final URL Generated:\n{Url}\n" +
            "?? Check vnp_ReturnUrl - must NOT contain query params!",
            MaskSensitiveUrl(finalUrl));

        return finalUrl;
    }

    /// <summary>
    /// Verify VNPay callback signature
    /// </summary>
    public bool VerifyCallback(VNPayCallbackDto callback)
    {
        var receivedSignature = callback.vnp_SecureHash;

        // Build parameter dictionary from callback (exclude signature)
        var vnpParams = BuildCallbackParameters(callback);

        // Generate signature from parameters
        var signData = BuildSignatureData(vnpParams);
        var computedSignature = ComputeHmacSha512(signData, _hashSecret);

        // Compare signatures (case-insensitive)
        return string.Equals(receivedSignature, computedSignature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify callback and extract payment code
    /// </summary>
    public bool VerifyCallbackAndExtractPaymentCode(VNPayCallbackDto callback, out string paymentCode)
    {
        paymentCode = callback.vnp_TxnRef;
        return VerifyCallback(callback);
    }

    /// <summary>
    /// Get human-readable error message from VNPay response code
    /// </summary>
    public string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            PaymentResponseCode.Success => "Giao d?ch thành công",
            PaymentResponseCode.VNPay_Suspicious => "Giao d?ch nghi ng? gian l?n",
            PaymentResponseCode.VNPay_NotRegistered => "Th? chua dang ký Internet Banking",
            PaymentResponseCode.VNPay_AuthFailed => "Xác th?c th?t b?i",
            PaymentResponseCode.VNPay_Timeout => "Giao d?ch h?t h?n",
            PaymentResponseCode.VNPay_InvalidCard => "Th? không h?p l?",
            PaymentResponseCode.VNPay_InvalidAmount => "S? ti?n không h?p l?",
            PaymentResponseCode.VNPay_InsufficientFunds => "Tài kho?n không d? s? du",
            PaymentResponseCode.VNPay_ExceededLimit => "Vu?t quá h?n m?c giao d?ch",
            PaymentResponseCode.VNPay_Maintenance => "Ngân hàng dang b?o trì",
            PaymentResponseCode.VNPay_InvalidPassword => "Sai m?t kh?u OTP",
            _ => $"L?i không xác d?nh (Code: {responseCode})"
        };
    }

    /// <summary>
    /// Validate return URL against whitelist to prevent phishing attacks
    /// ? GAP 3 FIX: Security measure to ensure only trusted URLs can be used
    /// </summary>
    private void ValidateReturnUrl(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
            throw new ArgumentException("Return URL cannot be null or empty", nameof(returnUrl));

        var baseUrl = NormalizeBaseUrl(returnUrl);
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException($"Invalid return URL format: {returnUrl}", nameof(returnUrl));

        // ? FIX: Allow localhost in development mode
        if (baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) || 
            baseUrl.Contains("127.0.0.1", StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "?? Allowing localhost URL in development: {Url}", baseUrl);
            return;
        }

        if (!_allowedReturnUrls.Contains(baseUrl))
        {
            throw new SecurityException(
                $"Return URL '{baseUrl}' is not in the allowed whitelist. " +
                $"Please configure allowed return URLs in appsettings.json under VNPay:AllowedReturnUrls");
        }
    }

    private void AddAllowedCallbackUrl(string? url)
    {
        var normalized = NormalizeBaseUrl(url);
        if (!string.IsNullOrEmpty(normalized))
        {
            _allowedReturnUrls.Add(normalized);
        }
    }

    private static string? NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}".TrimEnd('/');
    }

    private static string? NormalizeVietnamPhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length == 11)
        {
            digits = "0" + digits[2..];
        }

        return digits.Length == 10 ? digits : null;
    }

    private static string? ExtractAsciiFirstName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        var firstName = parts[^1];

        var normalized = firstName.Normalize(NormalizationForm.FormD);
        var withoutMarks = new string(normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var asciiBytes = Encoding.ASCII.GetBytes(withoutMarks);
        var ascii = Encoding.ASCII.GetString(asciiBytes).Trim();

        return string.IsNullOrWhiteSpace(ascii) ? null : ascii;
    }

    #region Private Helper Methods

    /// <summary>
    /// Build VNPay payment parameters dictionary
    /// </summary>
    private Dictionary<string, string> BuildPaymentParameters(VNPayPaymentRequestDto request)
    {
        // ?? FIX: Ensure Vietnam timezone for vnp_CreateDate
        var vnTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _vnTimeZone);
        
        // ? CRITICAL FIX: Use InvariantCulture to prevent Unicode issues
        var createDate = vnTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        
        // ? FIX: Add vnp_ExpireDate (required by VNPay)
        // Increase to 30 minutes for better UX
        var expireDate = vnTime.AddMinutes(30).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        
        // ? CRITICAL FIX: Clean URLs - Remove ALL query parameters from ReturnUrl and IpnUrl
        var cleanReturnUrl = CleanUrl(request.ReturnUrl);
        var cleanIpnUrl = string.IsNullOrWhiteSpace(request.IpnUrl) ? null : CleanUrl(request.IpnUrl);

        // ?? DEBUG: Log URL cleaning
        _logger.LogInformation(
            "?? VNPay URL Cleaning:\n" +
            "ReturnUrl (Original): {OriginalReturn}\n" +
            "ReturnUrl (Cleaned): {CleanReturn}\n" +
            "IpnUrl (Original): {OriginalIpn}\n" +
            "IpnUrl (Cleaned): {CleanIpn}",
            request.ReturnUrl,
            cleanReturnUrl,
            request.IpnUrl ?? "null",
            cleanIpnUrl ?? "null");
        
        // ?? DEBUG: Log time sync
        _logger.LogInformation(
            "? VNPay Time Sync:\n" +
            "UTC: {UtcNow}\n" +
            "VN Time: {VnTime}\n" +
            "TimeZone: {TimeZone}\n" +
            "CreateDate: '{CreateDate}' (Length: {CreateDateLength})\n" +
            "ExpireDate: '{ExpireDate}' (Length: {ExpireDateLength})",
            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            vnTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            _vnTimeZone.DisplayName,
            createDate,
            createDate.Length,
            expireDate,
            expireDate.Length);

        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_Version", _version },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _tmnCode },
            { "vnp_Amount", ((long)(request.Amount * 100)).ToString(CultureInfo.InvariantCulture) }, // VNPay uses amount * 100
            { "vnp_CreateDate", createDate }, // ? Use VN timezone with InvariantCulture
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", request.IpAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", request.OrderInfo },
            { "vnp_OrderType", request.CategoryCode },
            { "vnp_ReturnUrl", cleanReturnUrl }, // ? MUST NOT contain query params
            { "vnp_TxnRef", ResolveTxnRef(request) },
            { "vnp_ExpireDate", expireDate } // ? REQUIRED by VNPay
        };

        // ? CRITICAL FIX: Add IpnUrl if provided (VNPay requires this for server callback)
        // IMPORTANT: IpnUrl MUST NOT contain query parameters
        if (!string.IsNullOrEmpty(cleanIpnUrl))
            vnpParams.Add("vnp_IpnUrl", cleanIpnUrl);

        // Add optional parameters if provided
        if (!string.IsNullOrEmpty(request.BankCode))
            vnpParams.Add("vnp_BankCode", request.BankCode);

        var firstName = ExtractAsciiFirstName(request.CustomerName);
        if (!string.IsNullOrEmpty(firstName))
            vnpParams.Add("vnp_Bill_FirstName", firstName);

        if (!string.IsNullOrEmpty(request.CustomerEmail))
            vnpParams.Add("vnp_Bill_Email", request.CustomerEmail);

        var phone = NormalizeVietnamPhone(request.CustomerPhone);
        if (!string.IsNullOrEmpty(phone))
            vnpParams.Add("vnp_Bill_Mobile", phone);

        // ?? DEBUG: Log final parameters
        _logger.LogInformation(
            "?? VNPay Parameters Summary:\n" +
            "Amount: {Amount} VND ({AmountCents} cents)\n" +
            "TxnRef: {TxnRef}\n" +
            "ReturnUrl: {ReturnUrl}\n" +
            "IpnUrl: {IpnUrl}\n" +
            "Customer: {Customer} ({Email})",
            request.Amount,
            vnpParams["vnp_Amount"],
            vnpParams["vnp_TxnRef"],
            vnpParams["vnp_ReturnUrl"],
            cleanIpnUrl ?? "Not configured",
            firstName ?? "N/A",
            request.CustomerEmail ?? "N/A");

        return vnpParams;
    }

    /// <summary>
    /// Normalize callback URL (strip query/fragment, append ngrok skip flag if needed)
    /// </summary>
    private static string CleanUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return url.TrimEnd('/');
        }

        var builder = new UriBuilder(parsed.Scheme, parsed.Host, parsed.IsDefaultPort ? -1 : parsed.Port,
            parsed.AbsolutePath.TrimEnd('/'));

        if (parsed.Host.EndsWith(".ngrok-free.app", StringComparison.OrdinalIgnoreCase))
        {
            builder.Query = "_ngrok-skip-browser-warning=1";
        }

        return builder.Uri.ToString();
    }

    private static string ResolveTxnRef(VNPayPaymentRequestDto request)
    {
        // ? SIMPLIFICATION: Always use PaymentId directly (already numeric)
        // No need for GatewayReference complexity
        return request.PaymentId.ToString();
    }

    /// <summary>
    /// Build parameter dictionary from callback (excluding signature)
    /// Only include keys that are present (non-null/whitespace) to match VNPay behavior.
    /// </summary>
    private static Dictionary<string, string> BuildCallbackParameters(VNPayCallbackDto callback)
    {
        var vnpParams = new Dictionary<string, string>();

        void AddIfPresent(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                vnpParams.Add(key, value!);
        }

        AddIfPresent("vnp_TmnCode", callback.vnp_TmnCode);
        AddIfPresent("vnp_Amount", callback.vnp_Amount);
        AddIfPresent("vnp_BankCode", callback.vnp_BankCode);
        AddIfPresent("vnp_PayDate", callback.vnp_PayDate);
        AddIfPresent("vnp_OrderInfo", callback.vnp_OrderInfo);
        AddIfPresent("vnp_TransactionNo", callback.vnp_TransactionNo);
        AddIfPresent("vnp_ResponseCode", callback.vnp_ResponseCode);
        AddIfPresent("vnp_TransactionStatus", callback.vnp_TransactionStatus);
        AddIfPresent("vnp_TxnRef", callback.vnp_TxnRef);

        // Optional parameters
        AddIfPresent("vnp_BankTranNo", callback.vnp_BankTranNo);
        AddIfPresent("vnp_CardType", callback.vnp_CardType);

        return vnpParams;
    }

    private string BuildSignedQueryString(
        Dictionary<string, string> parameters,
        out string signData,
        out string secureHash)
    {
        // ? CRITICAL: Sort by KEY (case-sensitive, ordinal comparison)
        var sortedParams = parameters
            .Where(kv => !string.IsNullOrEmpty(kv.Value) &&
                         !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                         !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal) // ? MUST use StringComparer.Ordinal
            .ToList();

        // ?? DEBUG: Verify sorting
        _logger.LogDebug(
            "?? Parameter Sorting Order:\n{Order}",
            string.Join("\n", sortedParams.Select((kv, idx) => $"  {idx + 1}. {kv.Key}")));

        var signBuilder = new StringBuilder();
        var queryBuilder = new StringBuilder();

        foreach (var (key, value) in sortedParams)
        {
            var encodedValue = EncodeValue(value!);

            if (signBuilder.Length > 0)
                signBuilder.Append('&');
            signBuilder.Append(key);
            signBuilder.Append('=');
            signBuilder.Append(encodedValue);

            if (queryBuilder.Length > 0)
                queryBuilder.Append('&');
            queryBuilder.Append(key);
            queryBuilder.Append('=');
            queryBuilder.Append(encodedValue);
        }

        signData = signBuilder.ToString();
        secureHash = ComputeHmacSha512(signData, _hashSecret);

        queryBuilder.Append("&vnp_SecureHashType=HMACSHA512&vnp_SecureHash=");
        queryBuilder.Append(secureHash);

        return queryBuilder.ToString();
    }

    private static string BuildSignatureData(Dictionary<string, string> parameters)
    {
        return string.Join("&",
            parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value) &&
                             !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                             !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{kv.Key}={EncodeValue(kv.Value!)}"));
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

    private static string EncodeValue(string? value)
    {
        var encoded = (WebUtility.UrlEncode(value ?? string.Empty) ?? string.Empty)
            .Replace("%20", "+");

        if (encoded.IndexOf('%') < 0)
        {
            return encoded;
        }

        var builder = new StringBuilder(encoded.Length);
        for (var i = 0; i < encoded.Length; i++)
        {
            var ch = encoded[i];
            if (ch == '%' && i + 2 < encoded.Length)
            {
                builder.Append('%');
                builder.Append(char.ToUpperInvariant(encoded[++i]));
                builder.Append(char.ToUpperInvariant(encoded[++i]));
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    private static string MaskSensitiveData(string signData)
    {
        if (string.IsNullOrWhiteSpace(signData))
        {
            return signData;
        }

        var pairs = signData.Split('&', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < pairs.Length; i++)
        {
            var kvp = pairs[i].Split('=', 2);
            if (kvp.Length != 2)
            {
                continue;
            }

            var key = kvp[0];
            var value = kvp[1];

            switch (key)
            {
                case "vnp_Bill_Email":
                    value = MaskEmail(value);
                    break;
                case "vnp_Bill_Mobile":
                    value = MaskPhone(value);
                    break;
            }

            pairs[i] = $"{key}={value}";
        }

        return string.Join('&', pairs);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "****";
        }

        var prefixLength = Math.Min(3, atIndex);
        var prefix = email[..prefixLength];
        var domain = email[(atIndex + 1)..];
        return $"{prefix}***@{domain}";
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length <= 4)
        {
            return "****";
        }

        var visibleLength = Math.Max(0, phone.Length - 6);
        return phone[..visibleLength] + new string('*', phone.Length - visibleLength);
    }

    private static TimeZoneInfo ResolveVietnamTimeZone(string? configuredId)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuredId))
        {
            candidates.Add(configuredId);
        }

        candidates.Add("SE Asia Standard Time"); // Windows
        candidates.Add("Asia/Ho_Chi_Minh"); // Linux

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate);
            }
            catch (TimeZoneNotFoundException)
            {
                continue;
            }
            catch (InvalidTimeZoneException)
            {
                continue;
            }
        }

        return TimeZoneInfo.Utc;
    }

    private static string SanitizeReference(string? paymentCode, int paymentId)
    {
        var raw = string.IsNullOrWhiteSpace(paymentCode)
            ? paymentId.ToString(CultureInfo.InvariantCulture)
            : paymentCode;

        var builder = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToUpperInvariant(ch));
            }
        }

        if (builder.Length == 0)
        {
            builder.Append(paymentId.ToString("D8", CultureInfo.InvariantCulture));
        }

        var sanitized = builder.ToString();

        if (sanitized.Length < 6)
        {
            sanitized = sanitized.PadLeft(6, '0');
        }
        else if (sanitized.Length > 32)
        {
            sanitized = sanitized[^32..];
        }

        return sanitized;
    }

    #endregion

    private static string MaskSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret) || secret.Length <= 8)
            return "****";
        return secret[..4] + "..." + secret[^4..];
    }

    private static string MaskSensitiveUrl(string url)
    {
        // Mask vnp_SecureHash for logging
        if (string.IsNullOrWhiteSpace(url))
            return url;

        var hashIndex = url.IndexOf("vnp_SecureHash=", StringComparison.Ordinal);
        if (hashIndex < 0)
            return url;

        var hashStart = hashIndex + 15;
        var nextParam = url.IndexOf('&', hashStart);
        var hashEnd = nextParam > 0 ? nextParam : url.Length;
        
        var hash = url[hashStart..hashEnd];
        var maskedHash = hash.Length > 16 ? hash[..8] + "..." + hash[^8..] : "****";
        
        return url[..hashStart] + maskedHash + (nextParam > 0 ? url[nextParam..] : "");
    }
}



























































































































































































































































































































































































































































































































































































































































































































































































































































