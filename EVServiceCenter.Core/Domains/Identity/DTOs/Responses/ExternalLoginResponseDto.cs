namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
    public class ExternalLoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserResponseDto? User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsNewUser { get; set; }
        public bool RequiresAdditionalInfo { get; set; }
        public string? ErrorCode { get; set; }
    }
}
