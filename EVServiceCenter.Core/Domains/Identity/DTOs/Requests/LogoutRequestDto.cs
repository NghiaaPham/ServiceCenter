namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests;

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
