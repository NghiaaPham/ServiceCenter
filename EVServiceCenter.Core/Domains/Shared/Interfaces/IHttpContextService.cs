namespace EVServiceCenter.Core.Domains.Shared.Interfaces
{
  public interface IHttpContextService
  {
    string GetClientIpAddress();
    string GetUserAgent();
    string GetRequestHeader(string headerName);
    string GetCurrentUserId();
    string GetCurrentUserName();
    bool IsAuthenticated();
  }
}