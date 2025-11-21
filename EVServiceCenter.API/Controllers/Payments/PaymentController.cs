using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
using EVServiceCenter.Core.Domains.Payments.DTOs.Requests;
using EVServiceCenter.Core.Domains.Payments.DTOs.Responses;
using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Domains.Payments.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EVServiceCenter.API.Controllers.Payments;

/// <summary>
/// Payment Management
/// Handles payment creation, gateway callbacks (webhooks), and payment queries
/// </summary>
[ApiController]
[Route("api/payments")]
[ApiExplorerSettings(GroupName = "Invoice & Payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IVNPayService _vnPayService;
    private readonly IInvoiceService _invoiceService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IVNPayService vnPayService,
        IInvoiceService invoiceService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _vnPayService = vnPayService;
        _invoiceService = invoiceService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// [Create] Create payment for invoice (gateway redirect or manual recording)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Verify invoice ownership before creating payment
            if (!await VerifyInvoiceOwnershipAsync(request.InvoiceId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to create payment for invoice {InvoiceId} without ownership",
                    GetCurrentUserId(), request.InvoiceId);
                return Forbid();
            }

            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            var result = await _paymentService.CreatePaymentAsync(
                request,
                userId,
                ipAddress,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = string.IsNullOrEmpty(result.PaymentUrl)
                    ? "Payment recorded successfully"
                    : "Payment URL created successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new { success = false, message = "Error creating payment" });
        }
    }

    /// <summary>
    /// VNPay Return URL - redirect customer back to frontend
    /// </summary>
    [HttpGet("vnpay/return")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn(
        [FromQuery] VNPayCallbackDto callback,
        [FromQuery] string? redirect, // Frontend redirect URL from query params
        CancellationToken cancellationToken)
    {
        if (callback == null || string.IsNullOrWhiteSpace(callback.vnp_TxnRef))
        {
            _logger.LogWarning("VNPay return missing required parameters");
            return BadRequest("Missing VNPay parameters");
        }

        _logger.LogInformation(
            "VNPay Return: TxnRef={TxnRef}, ResponseCode={ResponseCode}, Redirect={Redirect}",
            callback.vnp_TxnRef, callback.vnp_ResponseCode, redirect);

     
        var result = await _paymentService.ProcessVNPayCallbackAsync(callback, cancellationToken);

        var success = result.Status == PaymentCallbackStatus.Success || result.Status == PaymentCallbackStatus.AlreadyProcessed;
        
      
        var redirectUrl = BuildFrontendRedirectUrl(callback.vnp_TxnRef, success, redirect);

        if (Uri.IsWellFormedUriString(redirectUrl, UriKind.Absolute))
        {
            _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

      
        return Ok(new
        {
            success,
            transaction = callback.vnp_TxnRef,
            message = result.Message ?? (success ? "Payment completed successfully." : "Payment failed or cancelled.")
        });
    }

    /// <summary>
    /// VNPay IPN (server to server) - update payment status
    /// </summary>
    [HttpGet("vnpay/ipn")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpn(
        [FromQuery] VNPayCallbackDto callback,
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ LOG: Bắt đầu xử lý IPN
            _logger.LogInformation(
                "🔔 VNPay IPN received: TxnRef={TxnRef}, ResponseCode={ResponseCode}, Amount={Amount}",
                callback?.vnp_TxnRef,
                callback?.vnp_ResponseCode,
                callback?.vnp_Amount);

            if (callback == null || string.IsNullOrWhiteSpace(callback.vnp_TxnRef))
            {
                _logger.LogWarning("❌ VNPay IPN: Missing required parameters");
                return VnPayContent("99", "Input data required");
            }

            // ✅ LOG: Gọi ProcessVNPayCallbackAsync
            _logger.LogInformation("🔄 Processing VNPay callback for TxnRef={TxnRef}", callback.vnp_TxnRef);
            
            var result = await _paymentService.ProcessVNPayCallbackAsync(callback, cancellationToken);

            // ✅ LOG: Kết quả xử lý
            _logger.LogInformation(
                "✅ VNPay IPN processed: Status={Status}, Message={Message}",
                result.Status,
                result.Message);

            switch (result.Status)
            {
                case PaymentCallbackStatus.Success:
                case PaymentCallbackStatus.AlreadyProcessed:
                    return VnPayContent("00", "Confirm Success");
                case PaymentCallbackStatus.InvalidSignature:
                    return VnPayContent("97", "Invalid signature");
                case PaymentCallbackStatus.PaymentNotFound:
                    return VnPayContent("01", "Order not found");
                case PaymentCallbackStatus.InvalidAmount:
                    return VnPayContent("01", "Invalid amount");
                case PaymentCallbackStatus.Failed:
                    return VnPayContent("02", "Payment failed");
                default:
                    _logger.LogWarning(
                        "⚠️ VNPay IPN returned status {Status} for transaction {TxnRef}. Message: {Message}",
                        result.Status,
                        callback.vnp_TxnRef,
                        result.Message);
                    return VnPayContent("99", "Unknown error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR processing VNPay IPN for TxnRef={TxnRef}", callback?.vnp_TxnRef);
            return VnPayContent("99", "Unknown error");
        }
    }

    /// <summary>
    /// [Mock] Hoàn tất thanh toán trong chế độ MOCK (VNPay/MoMo)
    /// Giúp demo nội bộ khi không có callback từ gateway thực
    /// </summary>
    [HttpPost("mock/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteMockPayment(
        [FromBody] MockPaymentCompleteRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsMockMode())
        {
            return BadRequest(new { success = false, message = "Mock payment endpoint is only available in Mock mode." });
        }

        if (string.IsNullOrWhiteSpace(request.PaymentCode))
        {
            return BadRequest(new { success = false, message = "PaymentCode is required." });
        }

        var payment = await _paymentService.GetPaymentByCodeAsync(request.PaymentCode, cancellationToken);
        if (payment == null)
        {
            return NotFound(new { success = false, message = $"Payment with code {request.PaymentCode} not found." });
        }

        var amount = request.Amount ?? payment.Amount;
        var gateway = string.IsNullOrWhiteSpace(request.Gateway)
            ? PaymentMethodType.VNPay
            : request.Gateway.Trim();

        bool success;
        if (gateway.Equals(PaymentMethodType.MoMo, StringComparison.OrdinalIgnoreCase))
        {
            var callback = new MoMoCallbackDto
            {
                partnerCode = "MOCKPARTNER",
                orderId = request.PaymentCode,
                requestId = Guid.NewGuid().ToString("N"),
                amount = (long)Math.Round(amount),
                orderInfo = $"Mock thanh toan {request.PaymentCode}",
                orderType = "momo_mock",
                transId = Guid.NewGuid().ToString("N"),
                resultCode = request.Success ? 0 : 9000,
                message = request.Success ? "Success" : "Mock failure",
                payType = "mock",
                responseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                extraData = string.Empty,
                signature = "MOCK_SIGNATURE"
            };

            success = await _paymentService.ProcessMoMoCallbackAsync(callback, cancellationToken);
        }
        else
        {
            var vnpAmount = ((long)Math.Round(amount * 100m)).ToString();
            var callback = new VNPayCallbackDto
            {
                vnp_TmnCode = "MOCKTMNCODE",
                vnp_Amount = vnpAmount,
                vnp_BankCode = "MOCKBANK",
                vnp_BankTranNo = Guid.NewGuid().ToString("N")[..12],
                vnp_CardType = "MOCK",
                vnp_PayDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                vnp_OrderInfo = $"Mock thanh toan {request.PaymentCode}",
                vnp_TransactionNo = Guid.NewGuid().ToString("N")[..12],
                vnp_ResponseCode = request.Success ? "00" : "24",
                vnp_TransactionStatus = request.Success ? "00" : "24",
                vnp_TxnRef = request.PaymentCode,
                vnp_SecureHash = "MOCK_SIGNATURE"
            };

            var vnPayResult = await _paymentService.ProcessVNPayCallbackAsync(callback, cancellationToken);
            success = vnPayResult.IsSuccess;
        }

        return Ok(new { success });
    }

    /// <summary>
    /// [Webhook] MoMo IPN callback (server-to-server)
    /// No authorization required - verified via signature
    /// </summary>
    [HttpPost("momo/ipn")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoMoIPN([FromBody] MoMoCallbackDto callback)
    {
        try
        {
            _logger.LogInformation("Received MoMo IPN for {OrderId}", callback.orderId);

            var success = await _paymentService.ProcessMoMoCallbackAsync(callback);

            // MoMo expects specific response format
            if (success)
            {
                return Ok(new { resultCode = 0, message = "Success" });
            }

            return Ok(new { resultCode = 1, message = "Invalid Signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return Ok(new { resultCode = 99, message = "Unknown error" });
        }
    }

    /// <summary>
    /// [Webhook] MoMo Return URL (browser redirect after payment)
    /// No authorization required - verified via signature
    /// </summary>
    [HttpPost("momo/return")]
    [AllowAnonymous]
    public async Task<IActionResult> MoMoReturn([FromBody] MoMoCallbackDto callback)
    {
        try
        {
            _logger.LogInformation("Received MoMo Return for {OrderId}", callback.orderId);

            var success = await _paymentService.ProcessMoMoCallbackAsync(callback);

            // Redirect to frontend with payment result
            var redirectUrl = BuildFrontendRedirectUrl(callback.orderId, success);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo Return");
            var redirectUrl = BuildFrontendRedirectUrl(callback.orderId, false);
            return Redirect(redirectUrl);
        }
    }

    /// <summary>
    /// [Details] Get payment by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);

            if (result == null)
                return NotFound(new { success = false, message = $"Payment {id} not found" });

            // SECURITY: Verify payment ownership via invoice
            if (!await VerifyInvoiceOwnershipAsync(result.InvoiceId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access payment {PaymentId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving payment" });
        }
    }

    /// <summary>
    /// [Details] Get payment by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [AllowAnonymous] // Cho phép FE kiểm tra kết quả sau redirect từ cổng thanh toán mà không cần access token
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentService.GetPaymentByCodeAsync(code, cancellationToken);

            if (result == null)
                return NotFound(new { success = false, message = $"Payment {code} not found" });

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by code {Code}", code);
            return StatusCode(500, new { success = false, message = "Error retrieving payment" });
        }
    }

    /// <summary>
    /// [List] Get all payments for an invoice
    /// </summary>
    [HttpGet("by-invoice/{invoiceId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentsByInvoice(int invoiceId, CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Verify invoice ownership before showing payments
            if (!await VerifyInvoiceOwnershipAsync(invoiceId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access payments for invoice {InvoiceId} without ownership",
                    GetCurrentUserId(), invoiceId);
                return Forbid();
            }

            var result = await _paymentService.GetPaymentsByInvoiceIdAsync(invoiceId, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for invoice {InvoiceId}", invoiceId);
            return StatusCode(500, new { success = false, message = "Error retrieving payments" });
        }
    }

    /// <summary>
    /// [Manual] Record manual payment (Cash, BankTransfer)
    /// Staff/Admin only
    /// </summary>
    [HttpPost("manual")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordManualPayment(
        [FromBody] CreatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            var result = await _paymentService.RecordManualPaymentAsync(
                request,
                userId,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = "Manual payment recorded successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording manual payment");
            return StatusCode(500, new { success = false, message = "Error recording payment" });
        }
    }

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    private string GetClientIpAddress()
    {
        // Check for X-Forwarded-For header (when behind proxy/load balancer)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fallback to RemoteIpAddress
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(remoteIp) ||
            remoteIp == "127.0.0.1" ||
            remoteIp == "::1")
        {
            // ✅ FIX: Use real public IP for VNPay (they reject 1.1.1.1)
            // For localhost testing, use a valid Vietnam public IP
            return "118.69.182.149"; // Vietnam IP for testing
        }

        return remoteIp;
    }

    private ContentResult VnPayContent(string rspCode, string message)
    {
        var payload = $"{{\"RspCode\":\"{rspCode}\",\"Message\":\"{message}\"}}";
        return Content(payload, "application/json");
    }

    private string BuildFrontendRedirectUrl(string paymentCode, bool success, string? requestedRedirect = null)
    {
        // Use provided redirect URL or fallback to query parameter or config
        var baseUrl = ResolveFrontendRedirectUrl(requestedRedirect);

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return string.Empty;
        }

        var queryParams = new Dictionary<string, string?>
        {
            ["paymentCode"] = paymentCode,
            ["status"] = success ? "success" : "failed"
        };

        return BuildUrlWithQuery(baseUrl, queryParams);
    }

    private string? ResolveFrontendRedirectUrl(string? requestedUrl)
    {
        var fallback = _configuration["PaymentGateway:FrontendReturnUrl"];
        if (string.IsNullOrWhiteSpace(fallback))
        {
            var website = _configuration["AppSettings:WebsiteUrl"];
            if (!string.IsNullOrWhiteSpace(website))
            {
                fallback = $"{website.TrimEnd('/')}/payment/result";
            }
        }

        if (string.IsNullOrWhiteSpace(requestedUrl))
        {
            return fallback;
        }

        if (IsUrlInAllowList(requestedUrl))
        {
            return requestedUrl;
        }

        _logger.LogWarning(
            "Redirect URL {RedirectUrl} is not in the allow list. Using fallback {Fallback}.",
            requestedUrl,
            fallback);

        return fallback;
    }

    private static string BuildUrlWithQuery(string baseUrl, IDictionary<string, string?> queryParameters)
    {
        if (string.IsNullOrWhiteSpace(baseUrl) || queryParameters == null)
        {
            return baseUrl;
        }

        var encodedParameters = queryParameters
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
            .Select(kv => string.Join("=", Uri.EscapeDataString(kv.Key), Uri.EscapeDataString(kv.Value ?? string.Empty)))
            .ToList();

        if (encodedParameters.Count == 0)
        {
            return baseUrl;
        }

        var separator = baseUrl.Contains('?') ? '&' : '?';
        return baseUrl + separator + string.Join("&", encodedParameters);
    }

    private bool IsUrlInAllowList(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var candidate = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}".TrimEnd('/');
        var allowedUrls = _configuration.GetSection("VNPay:AllowedReturnUrls").Get<string[]>() ?? Array.Empty<string>();

        foreach (var allowed in allowedUrls)
        {
            if (string.IsNullOrWhiteSpace(allowed))
            {
                continue;
            }

            if (Uri.TryCreate(allowed, UriKind.Absolute, out var allowedUri))
            {
                var normalizedAllowed = $"{allowedUri.Scheme}://{allowedUri.Host}{allowedUri.AbsolutePath}".TrimEnd('/');
                if (string.Equals(candidate, normalizedAllowed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (string.Equals(candidate, allowed.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsMockMode()
    {
        var mode = _configuration["PaymentGateway:Mode"];
        return string.IsNullOrWhiteSpace(mode) ||
               mode.Equals("Mock", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that the current user owns the invoice
    /// Staff/Admin/Manager bypass this check (via role)
    /// Customers must own the invoice via CustomerId claim
    /// </summary>
    private async Task<bool> VerifyInvoiceOwnershipAsync(int invoiceId, CancellationToken cancellationToken)
    {
        // Staff and above can access any invoice/payment
        if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
        {
            return true;
        }

        // For customers, verify ownership via CustomerId
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
        {
            _logger.LogWarning("CustomerId claim not found for user {UserId}", GetCurrentUserId());
            return false;
        }

        // Get invoice and verify CustomerId matches
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found during ownership verification", invoiceId);
            return false;
        }

        return invoice.CustomerId == customerId;
    }

    #endregion
}
