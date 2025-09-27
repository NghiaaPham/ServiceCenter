namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class ExternalLoginRequestDto
    {
        public string Provider { get; set; } = string.Empty; 
        public string IdToken { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
    }
}
