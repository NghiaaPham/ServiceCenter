using EVServiceCenter.Core.Domains.Identity.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.API.Middlewares
{
    /// <summary>
    /// Middleware ki?m tra JWT token có b? revoke (blacklist) không
    /// Ch?y sau JWT authentication middleware
    /// </summary>
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenBlacklistMiddleware> _logger;

        public TokenBlacklistMiddleware(
            RequestDelegate next,
            ILogger<TokenBlacklistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklistService)
        {
            // Ch? check n?u có Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Check if token is blacklisted
                var isRevoked = await blacklistService.IsTokenRevokedAsync(token, context.RequestAborted);

                if (isRevoked)
                {
                    _logger.LogWarning(
                        "Blocked request with revoked token. Path: {Path}, IP: {IP}",
                        context.Request.Path,
                        context.Connection.RemoteIpAddress);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        message = "Token ?ã b? thu h?i. Vui lòng ??ng nh?p l?i.",
                        errorCode = "TOKEN_REVOKED",
                        timestamp = DateTime.UtcNow
                    };

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }

            // Token không b? revoke, cho request ti?p t?c
            await _next(context);
        }
    }
}
