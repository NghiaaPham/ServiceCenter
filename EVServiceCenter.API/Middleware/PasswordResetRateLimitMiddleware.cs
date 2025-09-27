using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EVServiceCenter.API.Middleware
{
    public class PasswordResetRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PasswordResetRateLimitMiddleware> _logger;

        private static readonly Dictionary<string, (int count, DateTime resetTime)> _requests = new();
        private static readonly int MaxRequestsPerHour = 5;
        private static readonly int MaxRequestsPer15Minutes = 2;

        public PasswordResetRateLimitMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<PasswordResetRateLimitMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply to password reset endpoints
            if (!IsPasswordResetEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);
            var cacheKey = $"pwd_reset_rate_limit_{clientIp}";

            // Check rate limit
            if (_cache.TryGetValue(cacheKey, out int requestCount))
            {
                if (requestCount >= MaxRequestsPerHour)
                {
                    _logger.LogWarning("Rate limit exceeded for password reset from IP: {IP}", clientIp);

                    context.Response.StatusCode = 429;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Quá nhiều yêu cầu đặt lại mật khẩu. Vui lòng thử lại sau 1 giờ.",
                        errorCode = "RATE_LIMIT_EXCEEDED"
                    }));
                    return;
                }
            }

            // Continue with request
            await _next(context);

            // Increment counter after successful request
            if (context.Response.StatusCode == 200)
            {
                _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromHours(1));
            }
        }

        private static bool IsPasswordResetEndpoint(string path)
        {
            return path.Contains("/forgot-password") ||
                   path.Contains("/reset-password") ||
                   path.Contains("/validate-reset-token");
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
