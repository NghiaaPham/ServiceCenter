using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services
{
    /// <summary>
    /// ✅ Enhancement #2: Production-ready payment webhook handler
    /// Handles VNPay and MoMo payment gateway callbacks with signature verification
    /// </summary>
    public class PaymentWebhookService : IPaymentWebhookService
    {
        private readonly EVDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentWebhookService> _logger;

        public PaymentWebhookService(
            EVDbContext context,
            IConfiguration configuration,
            ILogger<PaymentWebhookService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HandleVNPayWebhookAsync(
            Dictionary<string, string> webhookData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "✅ Enhancement #2 - Received VNPay webhook with {Count} parameters",
                webhookData.Count);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Extract key parameters
                if (!webhookData.TryGetValue("vnp_TxnRef", out var txnRef) ||
                    !webhookData.TryGetValue("vnp_TransactionStatus", out var transactionStatus) ||
                    !webhookData.TryGetValue("vnp_Amount", out var amountStr))
                {
                    _logger.LogWarning("❌ Missing required VNPay webhook parameters");
                    return false;
                }

                // 2. Verify signature
                var secureHash = webhookData.GetValueOrDefault("vnp_SecureHash", "");
                if (!await VerifyVNPaySignatureAsync(webhookData, secureHash))
                {
                    _logger.LogWarning("❌ VNPay webhook signature verification failed");
                    return false;
                }

                // 3. Find PaymentIntent by transaction reference
                var paymentIntent = await _context.PaymentIntents
                    .Include(pi => pi.Appointment)
                    .FirstOrDefaultAsync(pi => pi.IntentCode == txnRef, cancellationToken);

                if (paymentIntent == null)
                {
                    _logger.LogWarning("⚠️ PaymentIntent not found for VNPay txnRef: {TxnRef}", txnRef);
                    return false;
                }

                // 4. Check idempotency (already processed?)
                if (paymentIntent.Status == PaymentIntentStatusEnum.Completed.ToString())
                {
                    _logger.LogInformation("ℹ️ PaymentIntent {IntentCode} already processed, skipping", paymentIntent.IntentCode);
                    return true;
                }

                // 5. Update status based on transaction status
                var amount = decimal.Parse(amountStr) / 100; // VNPay returns amount * 100

                if (transactionStatus == "00") // Success
                {
                    paymentIntent.Status = PaymentIntentStatusEnum.Completed.ToString();
                    paymentIntent.ConfirmedDate = DateTime.UtcNow;
                    paymentIntent.UpdatedDate = DateTime.UtcNow;

                    // Update Appointment if exists
                    if (paymentIntent.Appointment != null)
                    {
                        paymentIntent.Appointment.PaidAmount = (paymentIntent.Appointment.PaidAmount ?? 0) + amount;
                        paymentIntent.Appointment.PaymentStatus = PaymentStatusEnum.Completed.ToString();
                    }

                    _logger.LogInformation(
                        "✅ VNPay payment successful: {IntentCode}, Amount: {Amount}đ",
                        paymentIntent.IntentCode, amount);
                }
                else
                {
                    paymentIntent.Status = PaymentIntentStatusEnum.Failed.ToString();
                    paymentIntent.UpdatedDate = DateTime.UtcNow;

                    _logger.LogWarning(
                        "❌ VNPay payment failed: {IntentCode}, Status: {Status}",
                        paymentIntent.IntentCode, transactionStatus);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "❌ Error processing VNPay webhook");
                return false;
            }
        }

        public async Task<bool> HandleMoMoWebhookAsync(
            Dictionary<string, string> webhookData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "✅ Enhancement #2 - Received MoMo webhook with {Count} parameters",
                webhookData.Count);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Extract key parameters
                if (!webhookData.TryGetValue("orderId", out var orderId) ||
                    !webhookData.TryGetValue("resultCode", out var resultCodeStr) ||
                    !webhookData.TryGetValue("amount", out var amountStr))
                {
                    _logger.LogWarning("❌ Missing required MoMo webhook parameters");
                    return false;
                }

                // 2. Verify signature
                var signature = webhookData.GetValueOrDefault("signature", "");
                if (!await VerifyMoMoSignatureAsync(webhookData, signature))
                {
                    _logger.LogWarning("❌ MoMo webhook signature verification failed");
                    return false;
                }

                // 3. Find PaymentIntent
                var paymentIntent = await _context.PaymentIntents
                    .Include(pi => pi.Appointment)
                    .FirstOrDefaultAsync(pi => pi.IntentCode == orderId, cancellationToken);

                if (paymentIntent == null)
                {
                    _logger.LogWarning("⚠️ PaymentIntent not found for MoMo orderId: {OrderId}", orderId);
                    return false;
                }

                // 4. Check idempotency
                if (paymentIntent.Status == PaymentIntentStatusEnum.Completed.ToString())
                {
                    _logger.LogInformation("ℹ️ PaymentIntent {IntentCode} already processed, skipping", paymentIntent.IntentCode);
                    return true;
                }

                // 5. Update status
                var resultCode = int.Parse(resultCodeStr);
                var amount = decimal.Parse(amountStr);

                if (resultCode == 0) // Success
                {
                    paymentIntent.Status = PaymentIntentStatusEnum.Completed.ToString();
                    paymentIntent.ConfirmedDate = DateTime.UtcNow;
                    paymentIntent.UpdatedDate = DateTime.UtcNow;

                    if (paymentIntent.Appointment != null)
                    {
                        paymentIntent.Appointment.PaidAmount = (paymentIntent.Appointment.PaidAmount ?? 0) + amount;
                        paymentIntent.Appointment.PaymentStatus = PaymentStatusEnum.Completed.ToString();
                    }

                    _logger.LogInformation(
                        "✅ MoMo payment successful: {IntentCode}, Amount: {Amount}đ",
                        paymentIntent.IntentCode, amount);
                }
                else
                {
                    paymentIntent.Status = PaymentIntentStatusEnum.Failed.ToString();
                    paymentIntent.UpdatedDate = DateTime.UtcNow;

                    _logger.LogWarning(
                        "❌ MoMo payment failed: {IntentCode}, ResultCode: {ResultCode}",
                        paymentIntent.IntentCode, resultCode);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "❌ Error processing MoMo webhook");
                return false;
            }
        }

        public async Task<bool> VerifyWebhookSignatureAsync(
            string provider,
            Dictionary<string, string> webhookData,
            string signature)
        {
            return provider.ToLower() switch
            {
                "vnpay" => await VerifyVNPaySignatureAsync(webhookData, signature),
                "momo" => await VerifyMoMoSignatureAsync(webhookData, signature),
                _ => false
            };
        }

        #region Private Helper Methods

        private async Task<bool> VerifyVNPaySignatureAsync(
            Dictionary<string, string> webhookData,
            string providedSignature)
        {
            await Task.CompletedTask; // Async consistency

            try
            {
                var hashSecret = _configuration["Payment:VNPay:HashSecret"] ?? "DEFAULTSECRET";

                // Build signature data from sorted parameters (exclude vnp_SecureHash)
                var sortedParams = webhookData
                    .Where(kv => kv.Key != "vnp_SecureHash" && !string.IsNullOrEmpty(kv.Value))
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key}={kv.Value}");

                var signData = string.Join("&", sortedParams);
                var computedHash = ComputeHmacSha512(signData, hashSecret);

                var isValid = computedHash.Equals(providedSignature, StringComparison.OrdinalIgnoreCase);

                _logger.LogDebug(
                    "🔐 VNPay signature verification: {Result}",
                    isValid ? "VALID" : "INVALID");

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying VNPay signature");
                return false;
            }
        }

        private async Task<bool> VerifyMoMoSignatureAsync(
            Dictionary<string, string> webhookData,
            string providedSignature)
        {
            await Task.CompletedTask; // Async consistency

            try
            {
                var accessKey = _configuration["Payment:MoMo:AccessKey"] ?? "DEFAULTACCESSKEY";
                var secretKey = _configuration["Payment:MoMo:SecretKey"] ?? "DEFAULTSECRET";

                // Build raw signature for MoMo
                var rawData = $"accessKey={accessKey}" +
                              $"&amount={webhookData.GetValueOrDefault("amount", "")}" +
                              $"&extraData={webhookData.GetValueOrDefault("extraData", "")}" +
                              $"&message={webhookData.GetValueOrDefault("message", "")}" +
                              $"&orderId={webhookData.GetValueOrDefault("orderId", "")}" +
                              $"&orderInfo={webhookData.GetValueOrDefault("orderInfo", "")}" +
                              $"&orderType={webhookData.GetValueOrDefault("orderType", "")}" +
                              $"&partnerCode={webhookData.GetValueOrDefault("partnerCode", "")}" +
                              $"&payType={webhookData.GetValueOrDefault("payType", "")}" +
                              $"&requestId={webhookData.GetValueOrDefault("requestId", "")}" +
                              $"&responseTime={webhookData.GetValueOrDefault("responseTime", "")}" +
                              $"&resultCode={webhookData.GetValueOrDefault("resultCode", "")}" +
                              $"&transId={webhookData.GetValueOrDefault("transId", "")}";

                var computedSignature = ComputeHmacSha256(rawData, secretKey);

                var isValid = computedSignature.Equals(providedSignature, StringComparison.OrdinalIgnoreCase);

                _logger.LogDebug(
                    "🔐 MoMo signature verification: {Result}",
                    isValid ? "VALID" : "INVALID");

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying MoMo signature");
                return false;
            }
        }

        private static string ComputeHmacSha512(string data, string secret)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private static string ComputeHmacSha256(string data, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        #endregion
    }
}
