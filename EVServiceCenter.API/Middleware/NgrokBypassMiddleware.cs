using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EVServiceCenter.API.Middleware;

/// <summary>
/// Middleware to add ngrok-skip-browser-warning header for all responses
/// This helps bypass ngrok's browser warning page
/// </summary>
public class NgrokBypassMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NgrokBypassMiddleware> _logger;

    public NgrokBypassMiddleware(RequestDelegate next, ILogger<NgrokBypassMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if request is coming through ngrok
        var host = context.Request.Host.Host;
        var isNgrok = host.Contains("ngrok", StringComparison.OrdinalIgnoreCase);

        if (isNgrok)
        {
            // Add header to bypass ngrok browser warning
            // This works for API calls but NOT for browser redirects
            context.Response.Headers.Add("ngrok-skip-browser-warning", "1");
            
            _logger.LogDebug("Added ngrok-skip-browser-warning header for request to {Host}", host);
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method for registering NgrokBypassMiddleware
/// </summary>
public static class NgrokBypassMiddlewareExtensions
{
    public static IApplicationBuilder UseNgrokBypass(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NgrokBypassMiddleware>();
    }
}
