namespace EVServiceCenter.Core.Domains.Payments.Interfaces
{
    /// <summary>
    /// ✅ FIX GAP #15: Webhook/callback support for payment status updates
    /// Service to handle payment gateway webhooks and callbacks
    /// </summary>
    public interface IPaymentWebhookService
    {
        /// <summary>
        /// Handle VNPay webhook callback
        /// </summary>
        Task<bool> HandleVNPayWebhookAsync(
            Dictionary<string, string> webhookData,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Handle MoMo webhook callback
        /// </summary>
        Task<bool> HandleMoMoWebhookAsync(
            Dictionary<string, string> webhookData,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify webhook signature/authenticity
        /// </summary>
        Task<bool> VerifyWebhookSignatureAsync(
            string provider,
            Dictionary<string, string> webhookData,
            string signature);
    }
}
