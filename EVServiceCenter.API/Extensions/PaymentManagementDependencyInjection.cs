using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Payments.Services;
using Microsoft.Extensions.Configuration;

namespace EVServiceCenter.API.Extensions;

/// <summary>
/// Dependency Injection configuration for Payment Management module
/// ✅ HYBRID PAYMENT: Supports Mock/Sandbox/Production modes
/// </summary>
public static class PaymentManagementDependencyInjection
{
    /// <summary>
    /// Register Payment Management services with intelligent mode detection
    /// Modes: Mock (demo) | Sandbox (test) | Production (real)
    /// </summary>
    public static IServiceCollection AddPaymentManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var paymentMode = configuration["PaymentGateway:Mode"] ?? "Mock";
        var vnpayTmnCode = configuration["VNPay:TmnCode"];
        var momoPartnerCode = configuration["MoMo:PartnerCode"];

        // Smart detection: Use Mock if credentials are placeholders
        var hasVNPayCredentials = !string.IsNullOrEmpty(vnpayTmnCode) &&
                                 !vnpayTmnCode.StartsWith("YOUR_");
        var hasMoMoCredentials = !string.IsNullOrEmpty(momoPartnerCode) &&
                                !momoPartnerCode.StartsWith("YOUR_");

        // Determine final mode
        var useRealGateway = paymentMode != "Mock" && (hasVNPayCredentials || hasMoMoCredentials);

        if (useRealGateway)
        {
            // REAL MODE: Sandbox or Production with actual credentials
            Console.WriteLine($"💳 Payment Gateway Mode: {paymentMode} (Real Gateway)");
            services.AddScoped<IVNPayService, VNPayService>();
            services.AddScoped<IMoMoService, MoMoService>();
        }
        else
        {
            // MOCK MODE: Demo/Development without credentials
            Console.WriteLine("🎭 Payment Gateway Mode: MOCK (Demo Mode - No Real Credentials)");
            Console.WriteLine("   ✓ VNPay: Mock Service");
            Console.WriteLine("   ✓ MoMo: Mock Service");
            Console.WriteLine("   ℹ️ To use real gateway: Set PaymentGateway:Mode='Sandbox' and provide credentials");

            services.AddScoped<IVNPayService, MockVNPayService>();
            services.AddScoped<IMoMoService, MockMoMoService>();
        }

        // Orchestration service (same for all modes)
        services.AddScoped<IPaymentService, PaymentService>();

        // Refund service
        services.AddScoped<IRefundService, RefundService>();

        // Webhook service
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

        return services;
    }
}
