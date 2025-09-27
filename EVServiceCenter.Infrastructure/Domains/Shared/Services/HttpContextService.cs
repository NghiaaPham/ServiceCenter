using EVServiceCenter.Core.Domains.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EVServiceCenter.Infrastructure.Domains.Shared.Services
{
  public class HttpContextService : IHttpContextService
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetClientIpAddress()
    {
      var context = _httpContextAccessor.HttpContext;
      if (context == null) return "Unknown";

      // Check for forwarded headers (when behind proxy/load balancer/nginx)
      if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
      {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
          // Take the first IP if there are multiple
          return forwardedFor.Split(',')[0].Trim();
        }
      }

      // Check X-Real-IP header (common with nginx)
      if (context.Request.Headers.ContainsKey("X-Real-IP"))
      {
        var realIp = context.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(realIp))
        {
          return realIp;
        }
      }

      // Fall back to remote IP address
      var remoteIp = context.Connection?.RemoteIpAddress?.ToString();

      // Handle IPv6 localhost
      if (remoteIp == "::1")
      {
        return "127.0.0.1";
      }

      return remoteIp ?? "Unknown";
    }

    public string GetUserAgent()
    {
      var context = _httpContextAccessor.HttpContext;
      if (context == null) return "Unknown";

      return context.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    public string GetRequestHeader(string headerName)
    {
      var context = _httpContextAccessor.HttpContext;
      if (context == null) return string.Empty;

      return context.Request.Headers[headerName].ToString() ?? string.Empty;
    }

    public string GetCurrentUserId()
    {
      var context = _httpContextAccessor.HttpContext;
      if (context?.User == null) return string.Empty;

      return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    public string GetCurrentUserName()
    {
      var context = _httpContextAccessor.HttpContext;
      if (context?.User == null) return string.Empty;

      return context.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public bool IsAuthenticated()
    {
      var context = _httpContextAccessor.HttpContext;
      return context?.User?.Identity?.IsAuthenticated ?? false;
    }
  }
}