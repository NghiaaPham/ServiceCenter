namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
    public class PasswordResetResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
        public string LoginUrl { get; set; } = string.Empty;
    }
}
