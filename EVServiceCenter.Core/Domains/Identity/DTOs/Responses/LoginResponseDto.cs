namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
  public class LoginResponseDto
  {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserResponseDto? User { get; set; }
    public string? Token { get; set; }
    public string? ErrorCode { get; set; }

    // Email Verification specific fields
    public bool RequireEmailVerification { get; set; }
    public string? Email { get; set; }
    public bool IsEmailVerificationLinkSent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  }
}