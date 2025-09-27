namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
    public class EmailVerificationResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public bool IsNewUser { get; set; }

        }
}
